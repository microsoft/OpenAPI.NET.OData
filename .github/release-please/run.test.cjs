const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const {test} = require('node:test');
const {GitHub, Manifest, setLogger} = require('release-please');
const {runReleasePlease, main, ExactPathExclusions} = require('./run.cjs');

setLogger({debug() {}, info() {}, warn() {}, error() {}});
const root = path.resolve(__dirname, '..', '..');
const branch = JSON.parse(fs.readFileSync(path.join(root, '.hidi-release-please-manifest.json')))['.'].startsWith('3.') ? 'main' : 'support/v2';
const hidiProject = 'src/Microsoft.OpenApi.Hidi/Microsoft.OpenApi.Hidi.csproj';
const hidiReadme = 'src/Microsoft.OpenApi.Hidi/readme.md';
const hidiChangelog = 'src/Microsoft.OpenApi.Hidi/CHANGELOG.md';
const hidiManifest = '.hidi-release-please-manifest.json';
const odataManifest = '.release-please-manifest.json';
const inputPaths = [
  'release-please-config.json', 'hidi-release-please-config.json',
  hidiManifest, odataManifest, hidiProject, hidiReadme, hidiChangelog,
  'Directory.Build.props', 'CHANGELOG.md'
];

class MemoryGitHub {
  constructor() {
    this.repository = {owner: 'microsoft', repo: 'OpenAPI.NET.OData', defaultBranch: branch};
    this.files = Object.fromEntries(inputPaths.map(file => [file, fs.readFileSync(path.join(root, file), 'utf8')]));
    this.commits = ['hidi-release-please-config.json', 'release-please-config.json'].map(file => ({
      sha: JSON.parse(this.files[file])['bootstrap-sha'], message: 'chore: baseline', files: []
    }));
    this.prs = [];
    this.releases = [];
    this.tags = [];
    this.labelWrites = [];
    this.nextPr = 1;
  }
  async getFileJson(file) { return JSON.parse(this.files[file]); }
  async getFileContentsOnBranch(file) {
    assert.ok(Object.hasOwn(this.files, file), `Unexpected read of ${file}`);
    return {parsedContent: this.files[file], content: Buffer.from(this.files[file]).toString('base64'), sha: 'file-sha'};
  }
  async *releaseIterator() { yield* this.releases; }
  async *tagIterator() { yield* this.tags; }
  async *mergeCommitIterator() { yield* this.commits; }
  async *pullRequestIterator(target, state) {
    assert.equal(target, branch);
    yield* this.prs.filter(pr => pr.state === state);
  }
  change(files, message = 'fix: representative change') {
    this.commits.unshift({sha: `change-${this.commits.length}`, message, files});
  }
  materialize(updates) {
    return Object.fromEntries(updates
      .filter(update => update.createIfMissing || Object.hasOwn(this.files, update.path))
      .map(update => [update.path, update.updater.updateContent(this.files[update.path] || '')]));
  }
  async createPullRequest(pr, target, message, updates) {
    assert.equal(target, branch);
    const created = {...pr, number: this.nextPr++, state: 'OPEN', output: this.materialize(updates)};
    this.prs.unshift(created);
    return created;
  }
  async updatePullRequest(number, plan) {
    const pr = this.prs.find(item => item.number === number);
    Object.assign(pr, {title: plan.title.toString(), body: plan.body.toString(), output: this.materialize(plan.updates)});
    return pr;
  }
  async addIssueLabels(labels, number) {
    this.labelWrites.push(['add', number, labels]);
    const pr = this.prs.find(item => item.number === number);
    pr.labels = [...new Set([...pr.labels, ...labels])];
  }
  async removeIssueLabels(labels, number) {
    this.labelWrites.push(['remove', number, labels]);
    const pr = this.prs.find(item => item.number === number);
    pr.labels = pr.labels.filter(label => !labels.includes(label));
  }
  async commentOnIssue() {}
  async createRelease(release) {
    const result = {tagName: release.tag.toString(), sha: release.sha, notes: release.notes, url: 'https://example.invalid/release'};
    this.releases.unshift(result);
    this.tags.unshift({name: result.tagName, sha: result.sha});
    return result;
  }
  merge(pr) {
    Object.assign(this.files, pr.output);
    Object.assign(pr, {state: 'MERGED', sha: `merge-${pr.number}`});
    this.commits.unshift({sha: pr.sha, message: pr.title, files: Object.keys(pr.output), pullRequest: pr});
  }
}

async function plans(github, component) {
  return (await Manifest.fromManifest(github, branch,
    component === 'hidi' ? 'hidi-release-please-config.json' : 'release-please-config.json',
    component === 'hidi' ? hidiManifest : odataManifest)).buildPullRequests();
}

const routingCases = [
  ['src/Microsoft.OpenApi.Hidi/Program.cs', false, true],
  ['src/Microsoft.OpenApi.Hidi/CsdlFilter.xslt', false, true],
  [hidiProject, false, true],
  [hidiReadme, false, true],
  [hidiChangelog, false, true],
  ['test/Microsoft.OpenApi.Hidi.Tests/StatsVisitorTests.cs', false, true],
  ['test/Microsoft.OpenApi.Hidi.Tests/UtilityFiles/SampleOpenApi.yml', false, true],
  ['test/Microsoft.OpenApi.Hidi.Tests/check-nuget-package-published.Tests.ps1', false, true],
  ['scripts/check-nuget-package-published.ps1', false, true],
  ['test/scripts/check-nuget-package-published.Tests.ps1', false, true],
  ['Dockerfile', false, true],
  ['.dockerignore', false, true],
  ['install-tool.ps1', false, true],
  ['tool/hidi-local-nuget.config', false, true],
  ['tool/Microsoft.OpenApi.Hidi.public.snk', false, true],
  ['hidi-release-please-config.json', false, true],
  [hidiManifest, false, true],
  ['.azure-pipelines/hidi-release.yml', false, true],
  ['src/Microsoft.OpenApi.OData.Reader/OpenApiConvertSettings.cs', true, false],
  ['test/Microsoft.OpenAPI.OData.Reader.Tests/OpenApiConvertSettingsTests.cs', true, false],
  ['src/OoasGui/Program.cs', true, false],
  ['src/OoasUtil/Program.cs', true, false],
  ['docs/graphSecurityScheme/GraphSchemes.md', true, false],
  ['Directory.Build.props', true, false],
  ['redocly.yaml', true, false],
  ['.redocly.lint-ignore.yaml', true, false],
  ['src/AssemblyInfo/AssemblyInfoCommon.cs', true, false],
  ['CHANGELOG.md', true, false],
  ['release-please-config.json', true, false],
  [odataManifest, true, false],
  ['tool/Microsoft.OpenApi.OData.snk', true, false],
  ['tool/35MSSharedLib1024.snk', true, false],
  ['tool/Microsoft.OpenApi.OData.public.snk', false, true],
  ['README.md', true, true],
  ['CONTRIBUTING.md', true, true],
  ['Build.props', true, true],
  ['src/Build.props', true, true],
  ['build.root', true, true],
  ['build.ps1', true, true],
  ['build.cmd', true, true],
  ['global.json', true, true],
  ['Microsoft.OpenApi.OData.sln', true, true],
  ['.github/workflows/ci-cd.yml', false, false],
  ['.github/release-please/run.cjs', false, false],
  ['.azure-pipelines/ci-build.yml', false, false],
  ['src/Microsoft.OpenApi.HidiSibling/Program.cs', true, true]
];

for (const [file, odata, hidi] of routingCases) {
  test(`real engine routing: ${file}`, async () => {
    const github = new MemoryGitHub();
    github.change([file]);
    assert.equal((await plans(github, 'odata')).length, Number(odata));
    assert.equal((await plans(github, 'hidi')).length, Number(hidi));
  });
}

test('mixed changes affect both components; excluded-only and empty changes affect neither', async () => {
  const github = new MemoryGitHub();
  github.change(['Dockerfile', 'src/Microsoft.OpenApi.OData.Reader/OpenApiConvertSettings.cs']);
  assert.equal((await plans(github, 'odata')).length, 1);
  assert.equal((await plans(github, 'hidi')).length, 1);
  github.commits.shift();
  github.change(['Dockerfile', 'test/Microsoft.OpenApi.Hidi.Tests/StatsVisitorTests.cs']);
  assert.equal((await plans(github, 'odata')).length, 0);
  github.commits.shift();
  github.change([]);
  assert.equal((await plans(github, 'odata')).length, 0);
  assert.equal((await plans(github, 'hidi')).length, 0);
});

test('actual Hidi patch output changes only its version, manifest, changelog and install examples', async () => {
  const github = new MemoryGitHub();
  const baseline = github.files[hidiProject];
  const oldVersion = JSON.parse(github.files[hidiManifest])['.'];
  const next = oldVersion.replace(/\d+$/, patch => Number(patch) + 1);
  github.change(['Dockerfile'], 'fix(hidi): independent distribution fix');
  const [pr] = await plans(github, 'hidi');
  const output = github.materialize(pr.updates);
  assert.deepEqual(Object.keys(output).sort(), [hidiChangelog, hidiProject, hidiReadme, hidiManifest].sort());
  assert.equal(pr.headRefName, `release-please--branches--${branch}--components--hidi`);
  assert.equal(pr.version.toString(), next);
  assert.ok(output[hidiProject].includes(`<Version>${next}</Version>`));
  const dependencies = text => [...text.matchAll(/<PackageReference Include="([^"]+)" Version="([^"]+)"/g)].map(match => match.slice(1));
  assert.deepEqual(dependencies(output[hidiProject]), dependencies(baseline));
  assert.match(output[hidiChangelog], new RegExp(`hidi-v${oldVersion}\\.\\.\\.hidi-v${next}`));
  assert.equal(JSON.parse(output[hidiManifest])['.'], next);
  assert.ok(output[hidiReadme].includes(`--version ${next}`));
  assert.ok(output[hidiReadme].includes(`YamlReader\` ${branch === 'main' ? '3.10.2' : '2.12.2'}`));
});

test('actual OData output and componentless tags retain existing behavior', async () => {
  const github = new MemoryGitHub();
  const oldVersion = JSON.parse(github.files[odataManifest])['.'];
  const next = oldVersion.replace(/\d+$/, patch => Number(patch) + 1);
  github.releases.push({tagName: `v${oldVersion}`, sha: github.commits.at(-1).sha, notes: ''});
  github.change(['Directory.Build.props'], 'fix(odata): independent library fix');
  const [pr] = await plans(github, 'odata');
  const output = github.materialize(pr.updates);
  assert.deepEqual(Object.keys(output).sort(), ['CHANGELOG.md', 'Directory.Build.props', odataManifest].sort());
  assert.equal(pr.headRefName, `release-please--branches--${branch}--components--Microsoft.OpenApi.OData`);
  assert.ok(output['Directory.Build.props'].includes(`<Version>${next}</Version>`));
  assert.match(output['CHANGELOG.md'], new RegExp(`v${oldVersion}\\.\\.\\.v${next}`));
  const created = await runReleasePlease(github, branch, 'odata');
  github.merge(created.prs[0]);
  const released = await runReleasePlease(github, branch, 'odata');
  assert.equal(released.releases[0].tagName, `v${next}`);
  assert.equal(released.prs.length, 0);
});

test('Hidi feature release increments minor; breaking changes fail before writing a PR or release', async () => {
  const github = new MemoryGitHub();
  const [major, minor] = JSON.parse(github.files[hidiManifest])['.'].split('.').map(Number);
  github.change(['src/Microsoft.OpenApi.Hidi/Program.cs'], 'feat(hidi): independent feature');
  assert.equal((await plans(github, 'hidi'))[0].version.toString(), `${major}.${minor + 1}.0`);
  github.commits.shift();
  github.change(['src/Microsoft.OpenApi.Hidi/Program.cs'], 'feat(hidi)!: incompatible feature');
  await assert.rejects(runReleasePlease(github, branch, 'hidi'), /outside the branch major/);
  assert.equal(github.prs.length, 0);
  assert.equal(github.releases.length, 0);
});

test('two tag-free Hidi version cycles do not replay commits or block OData', async () => {
  const github = new MemoryGitHub();
  const baseline = JSON.parse(github.files[hidiManifest])['.'];
  github.change(['Dockerfile'], 'fix(hidi): first cycle');
  const first = await runReleasePlease(github, branch, 'hidi');
  assert.deepEqual(first.releases, []);
  assert.deepEqual(first.prs[0].labels, ['autorelease: hidi-pending']);
  assert.equal((await runReleasePlease(github, branch, 'hidi')).prs.length, 0);
  github.merge(first.prs[0]);
  assert.equal((await runReleasePlease(github, branch, 'hidi')).prs.length, 0);
  assert.deepEqual(first.prs[0].labels, ['autorelease: hidi-versioned']);
  assert.deepEqual(github.labelWrites.slice(0, 2).map(write => write[0]), ['add', 'remove']);
  github.change(['scripts/check-nuget-package-published.ps1'], 'fix(hidi): second cycle');
  const second = await runReleasePlease(github, branch, 'hidi');
  const patch = Number(baseline.split('.')[2]);
  assert.equal(JSON.parse(second.prs[0].output[hidiManifest])['.'], baseline.replace(/\d+$/, String(patch + 2)));
  assert.ok(second.prs[0].body.includes('second cycle'));
  assert.ok(!second.prs[0].body.includes('first cycle'));
  github.merge(second.prs[0]);
  assert.equal((await runReleasePlease(github, branch, 'hidi')).prs.length, 0);
  github.change(['src/Microsoft.OpenApi.OData.Reader/OpenApiConvertSettings.cs'], 'fix(odata): after Hidi merges');
  assert.equal((await runReleasePlease(github, branch, 'hidi')).prs.length, 0);
  assert.equal((await runReleasePlease(github, branch, 'odata')).prs.length, 1);
  assert.deepEqual(github.tags, []);
  assert.deepEqual(github.releases, []);
});

test('open Hidi PR updates with additional commits instead of opening a duplicate', async () => {
  const github = new MemoryGitHub();
  github.change(['Dockerfile'], 'fix(hidi): first fix');
  const first = await runReleasePlease(github, branch, 'hidi');
  github.change(['install-tool.ps1'], 'fix(hidi): additional fix');
  const updated = await runReleasePlease(github, branch, 'hidi');
  assert.equal(updated.prs[0].number, first.prs[0].number);
  assert.equal(github.prs.length, 1);
  assert.ok(updated.prs[0].body.includes('additional fix'));
});

test('unsafe manifest and merged PR state fail without label or release writes', async t => {
  for (const kind of ['wrong-major', 'project-version-mismatch', 'missing-checkpoint', 'wrong-branch', 'bad-body', 'future-version', 'duplicate-checkpoint', 'unreachable-checkpoint', 'title-body-mismatch']) {
    await t.test(kind, async () => {
      const github = new MemoryGitHub();
      github.change(['Dockerfile'], 'fix(hidi): cycle');
      const {prs} = await runReleasePlease(github, branch, 'hidi');
      github.merge(prs[0]);
      if (kind === 'wrong-major') github.files[hidiManifest] = '{".":"9.0.0"}';
      if (kind === 'project-version-mismatch') github.files[hidiProject] = github.files[hidiProject].replace(/<Version>.*?<\/Version>/, '<Version>9.0.0</Version>');
      if (kind === 'missing-checkpoint') github.prs = [];
      if (kind === 'wrong-branch') prs[0].headBranchName = 'unrelated';
      if (kind === 'bad-body') prs[0].body = 'not a release PR';
      if (kind === 'future-version') github.files[hidiManifest] = fs.readFileSync(path.join(root, hidiManifest), 'utf8');
      if (kind === 'duplicate-checkpoint') github.prs.push({...prs[0], number: 99, sha: 'duplicate'});
      if (kind === 'unreachable-checkpoint') github.commits.shift();
      if (kind === 'title-body-mismatch') prs[0].title = 'chore(main): release hidi 9.0.0';
      await assert.rejects(runReleasePlease(github, branch, 'hidi'));
      assert.deepEqual(github.labelWrites, []);
      assert.deepEqual(github.releases, []);
    });

  }
});

test('merged PR body version must agree with its title even when the title matches the manifest', async () => {
  const github = new MemoryGitHub();
  github.change(['Dockerfile'], 'fix(hidi): cycle');
  const {prs} = await runReleasePlease(github, branch, 'hidi');
  github.merge(prs[0]);
  const current = JSON.parse(github.files[hidiManifest])['.'];
  prs[0].body = prs[0].body.replaceAll(current, current.replace(/\d+$/, patch => Number(patch) + 1));
  await assert.rejects(runReleasePlease(github, branch, 'hidi'), /inconsistent title and body/);
  assert.deepEqual(github.labelWrites, []);
});

test('unknown component is an explicit failure', async () => {
  await assert.rejects(runReleasePlease(new MemoryGitHub(), branch, 'unknown'), /Unknown component/);
});

test('missing commit files are an explicit plugin failure', async () => {
  const plugin = new ExactPathExclusions(new MemoryGitHub(), branch, {'.': {excludePaths: []}});
  await assert.rejects(plugin.preconfigure({}, {'.': [{sha: 'bad'}]}), /Missing changed files/);
});

test('Hidi ignores pending OData PRs and interrupted label transitions are retryable', async () => {
  const github = new MemoryGitHub();
  github.change(['Directory.Build.props'], 'fix(odata): root cycle');
  const rootPr = (await runReleasePlease(github, branch, 'odata')).prs[0];
  github.merge(rootPr);
  github.change(['Dockerfile'], 'fix(hidi): independent cycle');
  const hidiPr = (await runReleasePlease(github, branch, 'hidi')).prs[0];
  assert.ok(hidiPr);
  github.merge(hidiPr);
  hidiPr.labels.push('autorelease: hidi-versioned');
  assert.equal((await runReleasePlease(github, branch, 'hidi')).prs.length, 0);
  assert.deepEqual(hidiPr.labels, ['autorelease: hidi-versioned']);
  assert.deepEqual(rootPr.labels, ['autorelease: pending']);
});

test('CLI wiring emits real library PR/release outputs and uses the app token', async t => {
  const github = new MemoryGitHub();
  const core = require('@actions/core');
  const saved = {...process.env};
  const argv = process.argv;
  t.after(() => { process.env = saved; process.argv = argv; });
  Object.assign(process.env, {RELEASE_PLEASE_TOKEN: 'offline-test-token', GITHUB_REF_NAME: branch, GITHUB_REPOSITORY: 'microsoft/OpenAPI.NET.OData'});
  t.mock.method(GitHub, 'create', async options => {
    assert.equal(options.token, 'offline-test-token');
    assert.equal(options.defaultBranch, branch);
    return github;
  });
  const outputs = {};
  t.mock.method(core, 'setOutput', (key, value) => { outputs[key] = value; });
  process.argv = ['node', 'run.cjs', 'hidi'];
  github.change(['Dockerfile'], 'fix(hidi): CLI cycle');
  await main();
  assert.equal(outputs.prs_created, true);
  assert.equal(outputs.releases_created, false);
  assert.equal(JSON.parse(outputs.prs)[0].headBranchName, `release-please--branches--${branch}--components--hidi`);
  assert.equal(outputs.paths_released, '[]');
  process.argv[2] = 'odata';
  github.change(['Directory.Build.props'], 'fix(odata): CLI cycle');
  await main();
  github.merge(github.prs.find(pr => pr.labels.includes('autorelease: pending')));
  await main();
  assert.equal(outputs.releases_created, true);
  assert.equal(outputs.release_created, true);
  assert.ok(outputs.tag_name.startsWith('v'));
  assert.ok(outputs.html_url);
  delete process.env.RELEASE_PLEASE_TOKEN;
  await assert.rejects(main(), /Missing release automation environment/);
});
