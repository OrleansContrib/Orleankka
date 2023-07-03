@ECHO OFF
SET DIR=%~dp0%
dotnet nake -- -f %DIR%\Nake.csx -d %DIR% %*