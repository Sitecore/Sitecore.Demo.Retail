@echo off
setlocal

SET ROOT_PATH=%1
SET DESTINATION_PATH=%2

IF NOT DEFINED ROOT_PATH SET ROOT_PATH=%~dp0
IF NOT DEFINED DESTINATION_PATH SET DESTINATION_PATH=C:\inetpub\CFRefStorefront\Website

CLS

ECHO "Root Path is %ROOT_PATH%"

PUSHD %ROOT_PATH%

@REM Create any required destination folders

MD "%DESTINATION_PATH%\bin\en-us\"
MD "%DESTINATION_PATH%\pipelines\"

@REM Copy Assemblies

XCOPY /S /R /Y "..\CSF\bin\Sitecore.Reference.Storefront.Powered.by.SitecoreCommerce.dll" "%DESTINATION_PATH%\bin\"
XCOPY /S /R /Y "..\CSF\bin\Sitecore.Reference.Storefront.Powered.by.SitecoreCommerce.pdb" "%DESTINATION_PATH%\bin\"
XCOPY /S /R /Y "..\..\Common\Project\bin\debug\Sitecore.Reference.Storefront.Common.dll" "%DESTINATION_PATH%\bin\"
XCOPY /S /R /Y "..\..\Common\Project\bin\debug\Sitecore.Reference.Storefront.Common.pdb" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\System.Web.Optimization.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\System.Web.Http.WebHost.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\System.Web.Http.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\System.Net.Http.Formatting.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\WebGrease.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\Newtonsoft.Json.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\en-us\CommerceMessageManager.resources.dll" "%DESTINATION_PATH%\bin\en-us\"
rem XCOPY /S /R /Y "..\CSF\bin\CommerceMessageManager.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\AntiXssLibrary.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\Castle.Core.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\Castle.Windsor.dll" "%DESTINATION_PATH%\bin\"
rem XCOPY /S /R /Y "..\CSF\bin\WebActivatorEx.dll" "%DESTINATION_PATH%\bin\"

@REM Copy Config Files

XCOPY /S /R /Y "..\CSF\App_Config\Include\*.config" "%DESTINATION_PATH%\App_Config\Include\"
XCOPY /R /Y "..\CSF\*.xml" "%DESTINATION_PATH%\"

@REM Copy Views...

XCOPY /S /I /R /Y "..\..\Common\CommonSettings\Views" "%DESTINATION_PATH%\Views\"
XCOPY /S /I /R /Y "..\CSF\Views" "%DESTINATION_PATH%\Views\"
XCOPY /S /I /R /Y "..\CSF\Scripts" "%DESTINATION_PATH%\Scripts\"
XCOPY /S /I /R /Y "..\..\Common\CommonSettings\Scripts" "%DESTINATION_PATH%\Scripts\"
XCOPY /S /I /R /Y "..\CSF\Content" "%DESTINATION_PATH%\Content\"
XCOPY /S /I /R /Y "..\..\Common\CommonSettings\Content" "%DESTINATION_PATH%\Content\"
XCOPY /S /I /R /Y "..\CSF\img" "%DESTINATION_PATH%\img\"
XCOPY /S /I /R /Y "..\CSF\Images" "%DESTINATION_PATH%\Images\"


@REM Copy Files...

XCOPY /S /R /Y "..\CSF\Global.asax" "%DESTINATION_PATH%\"
XCOPY /S /R /Y "..\CSF\Global.asax.solr" "%DESTINATION_PATH%\"

POPD

echo off