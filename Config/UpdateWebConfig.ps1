#
#   Created by BHennelly@jaas.net
#   Rebuild Web.Config from Web.Config.* files
#

$devFileName = 'web.config.DEV'
$baseWebConfigName = 'web.config.BASE'
$scriptBranchRoot = (get-item $PSScriptRoot ).parent.FullName
$erpFolder = Join-Path $scriptBranchRoot 'ERP'

Function CheckExistsWebConfig($rootDir)
{
    $siteWebConfig = Join-Path $rootDir 'web.config'
    $baseWebConfig = Join-Path $rootDir $baseWebConfigName

    #if not exists create it
    if(!(Test-Path $siteWebConfig) -and (Test-Path $baseWebConfig))
    {
        Write-Host ('Creating {0} from base' -f $siteWebConfig)
        #copy base as normal for first run...
        Copy-Item $baseWebConfig $siteWebConfig
    }
}

# Check for the DEV settings file and if not exist create it
Function CheckDevFile($devFile)
{
    #if not exists create it
    if(!(Test-Path $devFile))
    {
        #read current web.config and copy the db string
        $webConfigFilePath = Join-Path $siteRoot 'web.config'

        $xml = [xml](Get-Content $webConfigFilePath)
     
        $connNodes = $xml.configuration.connectionStrings.add
        $connectionString = ''
        #starting in 6.0 there are 2 connection strings in the web config...
        foreach($node in $connNodes)
        {
            if($node.name -eq "ProjectX")
            {
                $connectionString = $node.connectionString
                break
            }
        }

        New-Item $devFile -type file
        Add-Content $devFile $connectionString
    }
}

# Take the Web.Config.BASE and make it the new Web.Config file
#  Also copy in the connection string from Web.Config.DEV and update some folder locations
Function ReplaceWebConfigFromBase($siteDirRoot, $devFile, $configFoldersRoot)
{
    $sourcePath = Join-Path $siteDirRoot $baseWebConfigName
    $destinationPath = Join-Path $siteDirRoot 'web.config'
    
    if(Test-Path $sourcePath)
    {
        Copy-Item -Force $sourcePath $destinationPath
    }
    else 
    {
        Write-Host "$sourcePath not found." -ForegroundColor Yellow   
        return  
    }

    if(!(Test-path $devFile))
    {
        throw [System.IO.FileNotFoundException] "$devFile not found"
    }

    $devContent = Get-Content $devFile
    $dbString = [string]$devContent

    # copy in the db string
    $xml = [xml](Get-Content $destinationPath)
     
    # update connection string
    $connNodes = $xml.configuration.connectionStrings.add
    #starting in 6.0 there are 2 connection strings in the web config...
    foreach($node in $connNodes)
    {
        if($node.name -eq "ProjectX")
        {
            $node.connectionString = $dbString
        }
    }

    # ERP folder shared for all sub (sites) folders
    if(!(Test-Path $configFoldersRoot))
    {
        Write-Host ('[Dev Web Config] Creating ERP Folder: {0}' -f $configFoldersRoot)
        New-Item $configFoldersRoot -type directory | Out-Null
    }

    [string]$snapshotsFolder = Join-Path $configFoldersRoot 'Snapshots'
    [string]$customizationTempFolder = Join-Path $configFoldersRoot 'Customization'
    [string]$backupFolder = Join-Path $configFoldersRoot 'Backup'

    # START SITE SPECIFIC FOLDERS
    if(!(Test-Path $snapshotsFolder))
    {
        Write-Host ('[Dev Web Config] Creating Snapshot Folder: {0}' -f $snapshotsFolder)
        New-Item $snapshotsFolder -type directory | Out-Null
    }

    if(!(Test-Path $customizationTempFolder))
    {
        Write-Host ('[Dev Web Config] Creating Customization Temp Folder: {0}' -f $customizationTempFolder)
        New-Item $customizationTempFolder -type directory | Out-Null
    }

    if(!(Test-Path $backupFolder))
    {
        Write-Host ('[Dev Web Config] Creating Backup Folder: {0}' -f $backupFolder)
        New-Item $backupFolder -type directory | Out-Null
    }

    # make sure debug set to true
    $node2 = $xml.configuration.'system.web'.compilation
    $node2.debug = "True"

    $nodeCustomizationPaths = $xml.selectNodes('//configuration//appSettings//add')
    foreach ($path in $nodeCustomizationPaths) {
      if($path.key -eq "CustomizationTempFilesPath"){
        $path.value = $customizationTempFolder
      }

      if($path.key -eq "SnapshotsFolder"){
        $path.value = $snapshotsFolder
      }

      if($path.key -eq "BackupFolder"){
        $path.value = $backupFolder
      }
    }

    $xml.Save($destinationPath)
}

$sleepTimeSeconds = 5

Try
{
    Write-Host "Branch: $scriptBranchRoot"

    # SETUP STANDARD SITE
    $siteRoot = Join-Path $scriptBranchRoot 'Site'
    CheckExistsWebConfig $siteRoot
    $devFile = Join-Path $siteRoot $devFileName
    CheckDevFile $devFile
    $siteErpFolder = Join-Path $erpFolder 'Site'
    ReplaceWebConfigFromBase $siteRoot $devFile $siteErpFolder
}
Catch
{
    Write-Warning '*************************************'
    Write-Warning 'script failed'
    Write-Error $_.Exception.Message
    Write-Warning '*************************************'
    
    $sleepTimeSeconds = 30
}
Finally
{
    Write-Host ''
    Write-Host 'End of process'
    #for short display to the user...
    Start-Sleep -s $sleepTimeSeconds
}