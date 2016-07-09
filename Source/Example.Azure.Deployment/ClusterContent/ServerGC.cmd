REM Check if the script is running in the Azure Emulator, and if so, do not run
REM IF "%IsEmulated%"=="true" goto :EOF 
If "%UseServerGC%"=="False" GOTO :ValidateBackground
If "%UseServerGC%"=="0" GOTO :ValidateBackground

SET UseServerGC="True"

:ValidateBackground
If "%UseBackgroundGC%"=="False" GOTO :CommandExecution
If "%UseBackgroundGC%"=="0" GOTO :CommandExecution
SET UseBackgroundGC="True"

:CommandExecution
PowerShell.exe -executionpolicy unrestricted -command ".\GCSettingsManagement.ps1" -serverGC %UseServerGC% -backgroundGC %UseBackgroundGC%

Exit /b