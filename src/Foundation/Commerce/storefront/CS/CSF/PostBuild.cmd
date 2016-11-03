@Echo OFF
SETLOCAL
set CPrefix=%~d0%~p0

REM if NOT '%OfficialBuildMachine%' == '1' ()
pushd %CPrefix%
call XCOPY /Y /R /S ".\SitecoreCommerce\Data" "..\Database"

if '%OfficialBuildMachine%' == '1' (
  call powershell.exe -ExecutionPolicy Unrestricted -NOPROFILE -FILE ..\..\build\SignSingle.ps1 -FileToSign %1
)

popd
ENDLOCAL
EXIT /B %ERRORLEVEL%