@echo off
setlocal

SET ROOT_PATH=%1
SET DESTINATION_PATH=%2

IF NOT DEFINED ROOT_PATH SET ROOT_PATH=%~dp0
IF NOT DEFINED DESTINATION_PATH SET DESTINATION_PATH=C:\inetpub\CFRefStorefront\Website

CLS

ECHO "Root Path is %ROOT_PATH%"

PUSHD %ROOT_PATH%

@REM Copy Assemblies

XCOPY /S /R /Y "..\CSF\bin\Sitecore.Reference.Storefront.Powered.by.SitecoreCommerce.dll" "%DESTINATION_PATH%\bin\"
XCOPY /S /R /Y "..\CSF\bin\Sitecore.Reference.Storefront.Powered.by.SitecoreCommerce.pdb" "%DESTINATION_PATH%\bin\"
XCOPY /S /R /Y "..\..\Common\Project\bin\debug\Sitecore.Reference.Storefront.Common.dll" "%DESTINATION_PATH%\bin\"
XCOPY /S /R /Y "..\..\Common\Project\bin\debug\Sitecore.Reference.Storefront.Common.pdb" "%DESTINATION_PATH%\bin\"

POPD

echo off