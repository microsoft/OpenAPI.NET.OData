# reference to System.*
$SysDirectory = [System.IO.Directory]
$SysPath = [System.IO.Path]
$SysFile = [System.IO.File]

# Default to Debug
$Configuration = 'Debug'

# Color
$Success = 'Green'
$Warning = 'Yellow'
$Err = 'Red'

if ($args.Count -eq 0) 
{
    $TestType = 'All'
    $Configuration = 'Release'
}
elseif ($args[0] -match 'DisableSkipStrongName')
{
    $TestType = "DisableSkipStrongName"
}
elseif ($args[0] -match 'EnableSkipStrongName')
{
    $TestType = "EnableSkipStrongName"
}
elseif ($args[0] -match 'SkipStrongName')
{
    # SkipStrongName is a legacy options.
    $TestType = "EnableSkipStrongName"
}
else 
{
    Write-Host 'Please choose Test or StrongName!' -ForegroundColor $Err
    exit
}

$PROGRAMFILESX86 = [Environment]::GetFolderPath("ProgramFilesX86")
$env:ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition



# Figure out the directory and path for SN.exe
$SN = $null
$SNx64 = $null
$SNVersions = @()
ForEach ($directory in $SysDirectory::EnumerateDirectories($PROGRAMFILESX86 + "\Microsoft SDKs\Windows", "*A"))
{
    # remove the first char 'v'
    $directoryName = $SysPath::GetFileName($directory).substring(1)

    # remove the last char 'A'
    $directoryName = $directoryName.substring(0, $directoryName.LastIndexOf('A'))

    # parse to double "10.0"
    $versionNo = [System.Double]::Parse($directoryName)

    $fileobject = $null
    $fileobject = New-Object System.Object
    $fileobject | Add-Member -type NoteProperty -Name version -Value $versionNo
    $fileobject | Add-Member -type NoteProperty -Name directory -Value $directory

    $SNVersions += $fileobject
}

# using the latest version
$SNVersions = $SNVersions | Sort-Object -Property version -Descending

ForEach ($ver in $SNVersions)
{
    # only care about the folder has "bin" subfolder
    $snBinDirectory = $ver.directory + "\bin"
    if(!$SysDirectory::Exists($snBinDirectory))
    {
        continue
    }

    if($SysFile::Exists($snBinDirectory + "\sn.exe") -and $SysFile::Exists($snBinDirectory + "\x64\sn.exe"))
    {
        $SN = $snBinDirectory + "\sn.exe"
        $SNx64 = $snBinDirectory + "\x64\sn.exe"
        break
    }
    else
    {
        ForEach ($netFxDirectory in $SysDirectory::EnumerateDirectories($snBinDirectory, "NETFX * Tools") | Sort -Descending)
        {
            # currently, sorting descending for the NETFX version looks good.
            if($SysFile::Exists($netFxDirectory + "\sn.exe") -and $SysFile::Exists($netFxDirectory + "\x64\sn.exe"))
            {
                $SN = $netFxDirectory + "\sn.exe"
                $SNx64 = $netFxDirectory + "\x64\sn.exe"
                break
            }
        }
    }
    
    if ($SN -ne $null -and $SNx64 -ne $null)
    {
        break
    }
}

# Other variables
$ProductProj = $ENLISTMENT_ROOT + "\src\Microsoft.OpenAPI.OData.Reader\Microsoft.OpenApi.OData.Reader.csproj"
$TESTProj = $ENLISTMENT_ROOT + "\test\Microsoft.OpenAPI.OData.Reader.Tests\Microsoft.OpenApi.OData.Reader.Tests.csproj"

$TESTDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\Test\net472"
$PRODUCTDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\net472"

$ProductDlls = "Microsoft.OpenApi.OData.Reader.dll"
$XUnitTestDlls = "Microsoft.OpenApi.OData.Reader.Tests.dll"

Function GetDlls
{
    $dlls = @()

    ForEach($dll in $ProductDlls)
    {
        $dlls += $PRODUCTDIR + "\" + $dll
    }

    ForEach($dll in $XUnitTestDlls)
    {
        $dlls += $TESTDIR + "\" + $dll
    }
	
    return $dlls
}

Function SkipStrongName
{
    $SnLog = $LOGDIR + "\SkipStrongName.log"
    Out-File $SnLog

    Write-Host 'Skip strong name validations for Microsoft.OpenApi.OData assemblies...'

    $dlls = GetDlls
    ForEach ($dll in $dlls)
    {
        & $SN /Vr $dll | Out-File $SnLog -Append
    }

    ForEach ($dll in $dlls)
    {
        & $SNx64 /Vr $dll | Out-File $SnLog -Append
    }

    Write-Host "SkipStrongName Done" -ForegroundColor $Success
}

Function DisableSkipStrongName
{
    $SnLog = $LOGDIR + "\DisableSkipStrongName.log"
    Out-File $SnLog

    Write-Host 'Disable skip strong name validations for Microsoft.OpenApi.OData assemblies...'

    $dlls = GetDlls
    ForEach ($dll in $dlls)
    {
        & $SN /Vu $dll | Out-File $SnLog -Append
    }

    ForEach ($dll in $dlls)
    {
        & $SNx64 /Vu $dll | Out-File $SnLog -Append
    }

    Write-Host "DisableSkipStrongName Done" -ForegroundColor $Success
}

Function Cleanup 
{    
    #TODO: Add some clean tasks	
    Write-Host "Clean Done" -ForegroundColor $Success
}

Function CleanBeforeScorch
{
    #TODO: Add some clean tasks	
    Write-Host "Clean Done" -ForegroundColor $Success
}

Function BuildProcess
{
    Write-Host '**********Start To Build The Project*********'
    
    $script:BUILD_START_TIME = Get-Date
	
	Write-Host "Build Product ..."
	
	& dotnet.exe build $ProductProj -c $Configuration
	
	Write-Host "Build Test ..."
	
	& dotnet.exe build $TESTProj -c $Configuration
	
    Write-Host "Build Done" -ForegroundColor $Success
    $script:BUILD_END_TIME = Get-Date
}

Function TestProcess
{
    Write-Host '**********Start To Run The Test*********'

    $script:TEST_START_TIME = Get-Date
	
    & dotnet test $TESTProj -c $Configuration  

    Write-Host "Test Done" -ForegroundColor $Success
    $script:TEST_END_TIME = Get-Date
}

# Main Process
if ($TestType -eq 'EnableSkipStrongName')
{
    CleanBeforeScorch
    BuildProcess
    SkipStrongName
    Exit
}
elseif ($TestType -eq 'DisableSkipStrongName')
{
    CleanBeforeScorch
    BuildProcess
    DisableSkipStrongName
    Exit
}

CleanBeforeScorch
BuildProcess
SkipStrongName
TestProcess
Cleanup

$buildTime = New-TimeSpan $script:BUILD_START_TIME -end $script:BUILD_END_TIME
$testTime = New-TimeSpan $script:TEST_START_TIME -end $script:TEST_END_TIME
Write-Host("Build time:`t" + $buildTime)
Write-Host("Test time:`t" + $testTime)