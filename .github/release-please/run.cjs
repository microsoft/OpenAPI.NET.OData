const {GitHub, Manifest, registerPlugin} = require('release-please');
const {ManifestPlugin} = require('release-please/build/src/plugin');
const {Version} = require('release-please/build/src/version');
const {buildStrategy} = require('release-please/build/src/factory');
const {FilePullRequestOverflowHandler} = require('release-please/build/src/util/pull-request-overflow-handler');

const HIDI_PENDING = 'autorelease: hidi-pending';
const HIDI_VERSIONED = 'autorelease: hidi-versioned';
const HIDI_FLOORS = {'main': '3.10.2', 'support/v2': '2.12.2'};

async function ensureHidiLabels(token, owner, repo, request = fetch) {
  const api = (process.env.GITHUB_API_URL || 'https://api.github.com').replace(/\/$/, '');
  const url = `${api}/repos/${encodeURIComponent(owner)}/${encodeURIComponent(repo)}/labels`;
  const headers = {
    Authorization: `Bearer ${token}`,
    Accept: 'application/vnd.github+json',
    'X-GitHub-Api-Version': '2022-11-28'
  };
  for (const label of [
    {name: HIDI_PENDING, color: 'fbca04', description: 'Pending independent Hidi version pull request'},
    {name: HIDI_VERSIONED, color: '0e8a16', description: 'Merged Hidi version pull request; no production release'}
  ]) {
    const labelUrl = `${url}/${encodeURIComponent(label.name)}`;
    let response = await request(labelUrl, {headers});
    let expectedStatus = 200;
    if (response.status === 404) {
      response = await request(url, {
        method: 'POST', headers: {...headers, 'Content-Type': 'application/json'},
        body: JSON.stringify(label)
      });
      if (response.status === 422) {
        // Another branch job may have created the same label concurrently.
        response = await request(labelUrl, {headers});
      } else if (response.status !== 201) {
        throw new Error(`Cannot create Hidi lifecycle label '${label.name}': HTTP ${response.status}`);
      } else {
        expectedStatus = 201;
      }
    }
    if (response.status !== expectedStatus || (await response.json()).name !== label.name) {
      throw new Error(`Cannot verify Hidi lifecycle label '${label.name}': HTTP ${response.status}`);
    }
  }
}

class ExactPathExclusions extends ManifestPlugin {
  async preconfigure(strategies, commitsByPath) {
    for (const [path, config] of Object.entries(this.repositoryConfig)) {
      const excludes = config.excludePaths || [];
      commitsByPath[path] = commitsByPath[path].filter(commit => {
        if (!Array.isArray(commit.files)) {
          throw new Error(`Missing changed files for commit ${commit.sha}`);
        }
        return commit.files.some(file =>
          (path === '.' || file.startsWith(`${path}/`)) &&
          !excludes.some(exclude => file === exclude || file.startsWith(`${exclude}/`))
        );
      });
    }
    return strategies;
  }

  async run(candidates) {
    for (const candidate of candidates) {
      if (candidate.config.component !== 'hidi') continue;
      const floor = HIDI_FLOORS[this.targetBranch];
      const version = candidate.pullRequest.version;
      if (!floor || !version || version.major !== Version.parse(floor).major ||
          version.preRelease || version.build || version.compare(Version.parse(floor)) <= 0) {
        throw new Error('Generated Hidi version is outside the branch major or migration floor');
      }
    }
    return candidates;
  }
}

registerPlugin('exact-path-exclusions', options =>
  new ExactPathExclusions(options.github, options.targetBranch, options.repositoryConfig)
);

async function prepareHidiVersionPrs(github, manifest, branch) {
  const floor = HIDI_FLOORS[branch];
  const manifestConfig = await github.getFileJson('hidi-release-please-config.json', branch);
  const config = manifest.repositoryConfig['.'];
  const version = manifest.releasedVersions['.'];
  if (!floor || Object.keys(manifest.repositoryConfig).length !== 1 ||
      Object.keys(manifest.releasedVersions).length !== 1 ||
      config?.component !== 'hidi' || config.releaseType !== 'simple' ||
      !config.includeComponentInTag || !config.includeVInTag ||
      (config.tagSeparator && config.tagSeparator !== '-') ||
      config.skipGithubRelease ||
      !config.separatePullRequests ||
      manifestConfig.label !== HIDI_PENDING ||
      !version || version.major !== Version.parse(floor).major ||
      version.preRelease || version.build || version.compare(Version.parse(floor)) < 0) {
    throw new Error('Unsafe Hidi branch, manifest, component, labels or version configuration');
  }
  const project = await github.getFileContentsOnBranch('src/Microsoft.OpenApi.Hidi/Microsoft.OpenApi.Hidi.csproj', branch);
  const projectVersions = [...project.parsedContent.matchAll(/<Version>\s*([^<]+?)\s*<\/Version>/g)];
  if (projectVersions.length !== 1 || projectVersions[0][1] !== version.toString()) {
    throw new Error('Hidi project version does not match its manifest');
  }

  const strategy = await buildStrategy({...config, github, path: '.', targetBranch: branch});
  const overflow = new FilePullRequestOverflowHandler(github);
  const expectedBranch = `release-please--branches--${branch}--components--hidi`;
  const pending = [];
  let checkpoint;
  for await (const pr of github.pullRequestIterator(branch, 'MERGED', 200, false)) {
    if (!pr.labels.some(label => label === HIDI_PENDING || label === HIDI_VERSIONED)) continue;
    if (pr.headBranchName !== expectedBranch || !pr.sha) {
      throw new Error(`Unsafe Hidi version PR #${pr.number}: unexpected branch or missing merge SHA`);
    }
    const releases = await strategy.buildReleases(pr);
    if (releases.length !== 1 || releases[0].tag.component !== 'hidi') {
      throw new Error(`Cannot parse merged Hidi version PR #${pr.number}`);
    }
    const release = releases[0];
    const body = await overflow.parseOverflow(pr);
    if (body?.releaseData.length !== 1 ||
        (body.releaseData[0].component && body.releaseData[0].component !== 'hidi') ||
        body.releaseData[0].version?.compare(release.tag.version) !== 0) {
      throw new Error(`Hidi version PR #${pr.number} has inconsistent title and body versions`);
    }
    if (release.tag.version.compare(version) > 0 ||
        release.tag.version.compare(Version.parse(floor)) <= 0) {
      throw new Error(`Merged Hidi PR #${pr.number} disagrees with the current manifest or migration floor`);
    }
    if (release.tag.version.compare(version) === 0) {
      if (checkpoint && checkpoint.sha !== pr.sha) {
        throw new Error(`Multiple merged Hidi PRs claim version ${version}`);
      }
      checkpoint = pr;
    }
    if (pr.labels.includes(HIDI_PENDING)) pending.push(pr);
  }
  if (!checkpoint && (version.toString() !== floor || pending.length)) {
    throw new Error(`No merged Hidi version PR matches manifest version ${version}`);
  }
  if (checkpoint) {
    // A version PR merge, not a production tag, is the boundary for the next cycle.
    let reachable = false;
    for await (const commit of github.mergeCommitIterator(branch, {
      maxResults: manifest.commitSearchDepth, backfillFiles: false
    })) {
      if (commit.sha === checkpoint.sha) {
        reachable = true;
        break;
      }
    }
    if (!reachable) throw new Error(`Hidi checkpoint ${checkpoint.sha} is outside the branch commit search`);
  }
  for (const pr of pending) {
    // Adding first makes an interrupted label transition safe to retry.
    await github.addIssueLabels([HIDI_VERSIONED], pr.number);
    await github.removeIssueLabels([HIDI_PENDING], pr.number);
  }
  return checkpoint?.sha;
}

async function runReleasePlease(github, branch, component) {
  if (!['odata', 'hidi'].includes(component)) throw new Error(`Unknown component: ${component}`);
  const configFile = component === 'hidi' ? 'hidi-release-please-config.json' : 'release-please-config.json';
  const manifestFile = component === 'hidi' ? '.hidi-release-please-manifest.json' : '.release-please-manifest.json';
  const load = (options = {}) => Manifest.fromManifest(github, branch, configFile, manifestFile, options);
  const releases = component === 'odata' ? await (await load()).createReleases() : [];
  let manifest = await load();
  if (component === 'hidi') {
    const lastReleaseSha = await prepareHidiVersionPrs(github, manifest, branch);
    if (lastReleaseSha) manifest = await load({lastReleaseSha});
  }
  const prs = await manifest.createPullRequests();
  return {releases, prs};
}

async function main() {
  const core = require('@actions/core');
  const token = process.env.RELEASE_PLEASE_TOKEN;
  const branch = process.env.GITHUB_REF_NAME;
  const [owner, repo] = (process.env.GITHUB_REPOSITORY || '').split('/');
  if (!token || !branch || !owner || !repo) throw new Error('Missing release automation environment');
  if (process.argv[2] === 'hidi') await ensureHidiLabels(token, owner, repo);
  const github = await GitHub.create({owner, repo, token, defaultBranch: branch});
  const {releases, prs} = await runReleasePlease(github, branch, process.argv[2]);
  core.setOutput('releases_created', releases.length > 0);
  core.setOutput('prs_created', prs.length > 0);
  core.setOutput('prs', JSON.stringify(prs));
  core.setOutput('paths_released', JSON.stringify(releases.map(release => release.path)));
  if (prs.length) core.setOutput('pr', prs[0]);
  for (const release of releases) {
    const prefix = release.path === '.' ? '' : `${release.path}--`;
    core.setOutput(`${prefix}release_created`, true);
    for (const [name, value] of Object.entries(release)) {
      const key = {tagName: 'tag_name', uploadUrl: 'upload_url', notes: 'body', url: 'html_url'}[name] || name;
      core.setOutput(`${prefix}${key}`, value);
    }
  }
}

module.exports = {ExactPathExclusions, prepareHidiVersionPrs, runReleasePlease, ensureHidiLabels, main};
if (require.main === module) {
  main().catch(error => require('@actions/core').setFailed(error.message));
}
