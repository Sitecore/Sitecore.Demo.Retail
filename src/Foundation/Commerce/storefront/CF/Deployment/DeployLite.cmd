SET ROOT_PATH=%1
SET DESTINATION_PATH=%2

IF NOT DEFINED ROOT_PATH SET ROOT_PATH=%~dp0
IF NOT DEFINED DESTINATION_PATH SET DESTINATION_PATH=C:\inetpub\CFRefStorefront\Website

CLS

ECHO "Root Path is %ROOT_PATH%"

PUSHD %ROOT_PATH%

@REM Copy Views...

XCOPY /S /I /R /Y "..\..\Common\CommonSettings\Views" "%DESTINATION_PATH%\Views\"
XCOPY /S /I /R /Y "..\CSF\Views" "%DESTINATION_PATH%\Views\"
XCOPY /S /I /R /Y "..\..\Common\CommonSettings\Scripts" "%DESTINATION_PATH%\Scripts\"
XCOPY /S /I /R /Y "..\CSF\Scripts" "%DESTINATION_PATH%\Scripts\"
XCOPY /S /I /R /Y "..\CSF\Content" "%DESTINATION_PATH%\Content\"
XCOPY /S /I /R /Y "..\CSF\img" "%DESTINATION_PATH%\img\"
XCOPY /S /I /R /Y "..\CSF\Images" "%DESTINATION_PATH%\Images\"
XCOPY /S /I /R /Y "..\CSF\App_Config" "%DESTINATION_PATH%\App_Config\"

@REM Copy Binaries...

copy /Y "..\TDSCommerceServer_Master\Debug\bin\Sitecore.CommerceServer.Storefront.dll" "%DESTINATION_PATH%\bin\"

POPD