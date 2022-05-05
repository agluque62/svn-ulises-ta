@echo off
REM %1 : Directorio de Instalacion 'Empresa'
REM %2 : Directorio de instalacion 'Producto'
REM %3 : Version para marcar.
cd %2
cd 
REM Instalando las tareas
echo "SCHTASKS.EXE /CREATE /TN UV5K-HMI-START /F /XML UV5K-HMI-START.XML" >> install.log.txt
SCHTASKS.EXE /CREATE /TN "UV5K-HMI-START" /F /XML "UV5K-HMI-START.XML" >> install.log.txt
rem echo "SCHTASKS.EXE /CREATE /TN UV5K-WAUDIO-START /F /XML UV5K-WAUDIO-START.XML /RU SYSTEM /RP" >> install.log.txt
rem SCHTASKS.EXE /CREATE /TN "UV5K-WAUDIO-START" /F /XML "UV5K-WAUDIO-START.XML" /RU SYSTEM /RP >> install.log.txt
echo "SCHTASKS.EXE /CREATE /TN TEAMVIEWER-START /F /XML TEAMVIEWER-START.xml" >> install.log.txt
SCHTASKS.EXE /CREATE /TN "TEAMVIEWER-START" /F /XML "TEAMVIEWER-START.xml" >> install.log.txt

REM Seleccionar el modo de alto rendimiento.
echo "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c" >> install.log.txt
powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c >> install.log.txt
REM Deshabilitar el paginado de memoria.
echo "wmic computersystem set AutomaticManagedPagefile=False" >> install.log.txt
wmic computersystem set AutomaticManagedPagefile=False >> install.log.txt
REM Borrar el fichero de página.
echo "wmic pagefileset delete" >> install.log.txt
wmic pagefileset delete >> install.log.txt

REM 
cd %1
cd
ATTRIB +I * /D /S

REM Ejecuto el marcado de la version
cd %2
echo "sfk.exe select . .exe .dll +md5gento md5mark_%3.md5 -rel" >> install.log.txt
sfk.exe select . .exe .dll +md5gento md5mark_%3.md5 -rel >> install.log.txt

@echo on