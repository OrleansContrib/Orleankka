@ECHO OFF

CALL "dotnet" restore Tools/Tools.csproj

SET DIR=%~dp0.
SET NAKE_RUNNER=%DIR%\Packages\Nake\2.4.0\tools\net45\Nake.exe

"%NAKE_RUNNER%" -f "%DIR%\Nake.csx" -d "%DIR%" %*