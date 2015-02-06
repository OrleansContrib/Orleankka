@ECHO OFF

SET DIR=%~dp0.
SET NAKE_VERSION=2.3.0
SET NAKE_RUNNER=%DIR%\Packages\Nake.%NAKE_VERSION%\tools\net45\Nake.exe

IF NOT EXIST "%NAKE_RUNNER%" "%DIR%\Tools\NuGet.exe" install Nake -Version %NAKE_VERSION% -o "%DIR%\Packages" 
"%NAKE_RUNNER%" -f "%DIR%\Nake.csx" -d "%DIR%" %*