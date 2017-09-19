@ECHO OFF

CALL ".paket\paket.bootstrapper.exe" -s
CALL ".paket\paket.exe" restore

SET DIR=%~dp0.
SET NAKE_RUNNER=%DIR%\Packages\Nake\tools\net45\Nake.exe

"%NAKE_RUNNER%" -f "%DIR%\Nake.csx" -d "%DIR%" %*