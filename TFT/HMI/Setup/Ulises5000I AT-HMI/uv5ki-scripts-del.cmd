@echo off
REM %1 : Directorio de instalacion
REM %2 : Version para marcar.
cd %1
cd 
REM Borrando las tareas
schtasks.exe /delete /TN "UV5K-HMI-START" /F
schtasks.exe /delete /TN "UV5K-WAUDIO-START" /F
schtasks.exe /delete /TN "TEAMVIEWER-START" /F 
REM Borro ficheros logs
rem del logs\*.csv /Q
@echo on