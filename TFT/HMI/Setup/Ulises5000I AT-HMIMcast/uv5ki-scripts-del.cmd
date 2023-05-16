@echo off
REM %1 : Directorio de instalacion
REM %2 : Version para marcar.
cd %1
cd 
REM Borrando las tareas
rem schtasks.exe /delete /TN "UV5K-HMI-START" /F
rem schtasks.exe /delete /TN "UV5K-WAUDIO-START" /F
rem schtasks.exe /delete /TN "TEAMVIEWER-START" /F 
@echo on