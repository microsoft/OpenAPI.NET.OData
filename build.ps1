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

$Build = 'build'
if ($args -contains 'rebuild')
{
    $Build = 'rebuild'
}

$PROGRAMFILESX86 = [Environment]::GetFolderPath("ProgramFilesX86")
$env:ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ENLISTMENT_ROOT = Split-Path -Parent $MyInvocation.MyCommand.Definition
$LOGDIR = $ENLISTMENT_ROOT + "\bin"

# Default to use Visual Studio 2017
$VS15MSBUILD=$PROGRAMFILESX86 + "\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
$VSTEST = $PROGRAMFILESX86 + "\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
$SN = $PROGRAMFILESX86 + "\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\sn.exe"
$SNx64 = $PROGRAMFILESX86 + "\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\x64\sn.exe"

# Other variables
$FXCOP = $FXCOPDIR + "\FxCopCmd.exe"
$BUILDLOG = $LOGDIR + "\msbuild.log"
$TESTLOG = $LOGDIR + "\mstest.log"
$TESTDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\Test\net46"
$PRODUCTDIR = $ENLISTMENT_ROOT + "\bin\$Configuration\net46"
$NUGETEXE = $PROGRAMFILESX86 + "\Microsoft Visual Studio\2017\Enterprise\MSBuild\ReadyRoll\OctoPack\build\NuGet.exe"
$NUGETPACK = $ENLISTMENT_ROOT + "\packages"

$ProductDlls = "Microsoft.OpenApi.OData.Reader.dll"

$XUnitTestDlls = "Microsoft.OpenApi.OData.Reader.Tests.dll"

$AllTestSuite = @()
ForEach($dll in $XUnitTestDlls)
{
    $AllTestSuite += $TESTDIR + "\" + $dll
}

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

# Incremental build and rebuild
Function RunBuild ($sln)
{
    Write-Host "*** Building $sln ***"
    $slnpath = $ENLISTMENT_ROOT + "\$sln"
    $Conf = "/p:Configuration=" + "$Configuration"

    # Default to VS2017
    $MSBUILD = $VS15MSBUILD
   
    & $MSBUILD $slnpath /t:$Build /m /nr:false /fl "/p:Platform=Any CPU" $Conf /p:Desktop=true `
        /flp:LogFile=$LOGDIR/msbuild.log /flp:Verbosity=Normal 1>$null 2>$null
		
    if($LASTEXITCODE -eq 0)
    {
        Write-Host "Build $sln SUCCESS" -ForegroundColor $Success
    }
    else
    {
        Write-Host "Build $sln FAILED" -ForegroundColor $Err
        Write-Host "For more information, please open the following test result files:"
        Write-Host "$LOGDIR\msbuild.log"
        Cleanup
        exit
    }
}

Function NugetRestoreSolution
{
    Write-Host '**********Pull NuGet Packages*********'
    & $NUGETEXE "restore" ($ENLISTMENT_ROOT + "\Microsoft.OpenApi.OData.sln")
}

Function BuildProcess
{
    Write-Host '**********Start To Build The Project*********'
    
    $script:BUILD_START_TIME = Get-Date
    if (Test-Path $BUILDLOG)
    {
        rm $BUILDLOG
    }

    RunBuild ('Microsoft.OpenApi.OData.sln')

    Write-Host "Build Done" -ForegroundColor $Success
    $script:BUILD_END_TIME = Get-Date
}

Function TestSummary
{
    Write-Host 'Collecting test results ...'
    
    $file = Get-Content -Path $TESTLOG
    $pass = 0
    $skipped = 0
    $fail = 0
    $trxfile = New-Object -TypeName System.Collections.ArrayList
    $failedtest1 = New-Object -TypeName System.Collections.ArrayList
    $failedtest2 = New-Object -TypeName System.Collections.ArrayList
    $part = 1
    foreach ($line in $file)
    {
        # Consolidate logic for retrieving number of passed and skipped tests. Failed tests is separate due to the way
        # VSTest and DotNet (for .NET Core tests) report results differently.
        if ($line -match "^Total tests: .*") 
        {
            # The line is in this format:
            # Total tests: 5735. Passed: 5735. Failed: 0. Skipped: 0.
            # We want to extract the total passed and total skipped.
            
            # Extract total passed by taking the substring between "Passed: " and "."
            # The regex first extracts the string after the hardcoded "Passed: " (i.e. "#. Failed: #. Skipped: #.")
            # Then we tokenize by "." and retrieve the first token which is the number for passed.
            $pattern = "Passed: (.*)"
            $extractedNumber = [regex]::match($line, $pattern).Groups[1].Value.Split(".")[0]
            $pass += $extractedNumber
            
            # Extract total failed by taking the substring between "Failed: " and "."
            # The regex first extracts the string after the hardcoded "Failed: " (i.e. "#.")
            # Then we tokenize by "." and retrieve the first token which is the number for skipped.
            $pattern = "Failed: (.*)"
            $extractedNumber = [regex]::match($line, $pattern).Groups[1].Value.Split(".")[0]
            $fail += $extractedNumber
			
			# Extract total skipped by taking the substring between "Skipped: " and "."
            # The regex first extracts the string after the hardcoded "Skipped: " (i.e. "#.")
            # Then we tokenize by "." and retrieve the first token which is the number for skipped.
            $pattern = "Skipped: (.*)"
            $extractedNumber = [regex]::match($line, $pattern).Groups[1].Value.Split(".")[0]
            $skipped += $extractedNumber
        }        
    }

    Write-Host "Test summary:" -ForegroundColor $Success
    Write-Host "Passed :`t$pass"  -ForegroundColor $Success

    if ($skipped -ne 0)
    {
        Write-Host "Skipped:`t$skipped"  -ForegroundColor $Warning
    }

    $color = $Success
    if ($fail -ne 0)
    {
        $color = $Err
    }
    Write-Host "Failed :`t$fail"  -ForegroundColor $color
    Write-Host "----------------------"  -ForegroundColor $Success
    Write-Host "Total :`t$($pass + $fail)"  -ForegroundColor $Success
    if ($fail -ne 0)
    {
		Write-Host "Find failed test information at:" $TESTLOG -ForegroundColor $Err
    }
    else
    {
        Write-Host "Congratulation! All of the tests passed!" -ForegroundColor $Success
    }
}

Function RunTest($title, $testdir)
{
    Write-Host "**********Running $title***********"
	
    & $VSTEST $testdir  >> $TESTLOG

    if($LASTEXITCODE -ne 0)
    {
        Write-Host "Run $title FAILED" -ForegroundColor $Err
    }
}

Function TestProcess
{
    Write-Host '**********Start To Run The Test*********'
    if (Test-Path $TESTLOG)
    {
        rm $TESTLOG
    }
    $script:TEST_START_TIME = Get-Date
    cd $TESTDIR
    if ($TestType -eq 'All')
    {
        RunTest -title 'All Tests' -testdir $AllTestSuite
    }
    else
    {
        Write-Host 'Error : TestType' -ForegroundColor $Err
        Cleanup
        exit
    }

    Write-Host "Test Done" -ForegroundColor $Success
    $script:TEST_END_TIME = Get-Date
	TestSummary
    cd $ENLISTMENT_ROOT
}

Function FxCopProcess
{
	# TODO:
}

# Main Process

if (! (Test-Path $LOGDIR))
{
    mkdir $LOGDIR 1>$null
}

if ($TestType -eq 'EnableSkipStrongName')
{
    CleanBeforeScorch
    NugetRestoreSolution
    BuildProcess
    SkipStrongName
    Exit
}
elseif ($TestType -eq 'DisableSkipStrongName')
{
    CleanBeforeScorch
    NugetRestoreSolution
    BuildProcess
    DisableSkipStrongName
    Exit
}

CleanBeforeScorch
NugetRestoreSolution
BuildProcess
SkipStrongName
TestProcess
FxCopProcess
Cleanup

$buildTime = New-TimeSpan $script:BUILD_START_TIME -end $script:BUILD_END_TIME
$testTime = New-TimeSpan $script:TEST_START_TIME -end $script:TEST_END_TIME
Write-Host("Build time:`t" + $buildTime)
Write-Host("Test time:`t" + $testTime)