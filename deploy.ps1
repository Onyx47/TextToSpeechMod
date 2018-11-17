#INSTRUCTIONS
#1. place in the root directory of the text to speech mod project
#2. open a command shell
#3. change the shell's directory to the project's root directory, using a "cd" command.
#4. profit!

$ErrorActionPreference = "stop"

$outputdir = $env:APPDATA + "\SpaceEngineers\Mods"
$inputdir = "./mirrored workshop build"
$outputfolder = "texttospeechmod"
$projectfile = "SE TextToSpeechMod.csproj"
$forwardslashchar = "\"
$excludes = @('.git', 'bin', '.vs', '.vscode', 'obj', 'Properties', "phonemes", "mirrored workshop build", "Logging", 'documentation')

if ($outputdir.EndsWith("\"))
{
    $outputfolder = $outputdir + $outputfolder + $forwardslashchar
}

else
{
    $outputfolder = $outputdir + $forwardslashchar + $outputfolder + $forwardslashchar
}
Write-Host "using output folder: $outputfolder"

Write-Host "deleting previous deployment..."

Try
{
    Remove-Item -Path $outputfolder
}

Catch{}

Write-Host "deploying text to speech mod..."

$scriptdir = $outputfolder + "Data/Scripts/folder required here" + $forwardslashchar

Write-Host "building the project..."
Invoke-Expression $("MSBuild.exe '" + $projectfile + "' -nologo -verbosity:minimal")

if ($LASTEXITCODE -ne 0)
{
    Write-Host "build failed! get it compiling in visual studio before continuing."
    Write-Host "script stopped."
    Exit
}
Write-Host "build succeeded."

Write-Host "copying mirrored directory..."

$mirrorfoldersitems = Get-ChildItem $inputdir

foreach($item in $mirrorfoldersitems)
{
    Try
    {
        Write-Host "Copying:" $item.FullName "to" $outputfolder
        Copy-Item $item.FullName -Destination $outputfolder -Recurse -Container -Force        
    }
    Catch
    {
        $outputdestinationcreated = New-Item -Path $outputfolder -ItemType 'directory' -Force
        Write-Host "Copying:" $item.FullName "to" $outputdestinationcreated
        Copy-Item $item.FullName -Destination $outputdestinationcreated -Recurse -Container -Force
    }
    
}
Write-Host "mirror directory copied."

Write-Host "overwriting with fresh script files..."

function copyacrossallincludedscripts([string]$currentdirectory, [string]$currentdestination)
{   
    Write-Host $("traversing directory: " + $currentdirectory)   
    $currentfiles = Get-ChildItem -Path $($currentdirectory + "\*") -Include '*.cs' -File
    $currentsubdirs = Get-ChildItem -Path $currentdirectory -Directory -Exclude $excludes     

    foreach ($item in $currentfiles)
    {
        Write-Host $("copying across script: " + $item.Name)
        
        Try
        {
            Get-Item -Path $currentdestination
        }

        Catch
        {
            New-Item -Path $currentdestination -ItemType 'directory'
        }
        Copy-Item $item.FullName -Destination $currentdestination -Force    
    }

    foreach ($subdir in $currentsubdirs)
    {
        copyacrossallincludedscripts -currentdirectory $subdir.FullName -currentdestination $($scriptdir + $subdir.Name)
    }
}
copyacrossallincludedscripts -currentdirectory $PSScriptRoot -currentdestination $scriptdir
Write-Host "mod deployed."