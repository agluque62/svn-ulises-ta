SCHTASKS /end /TN UV5K-HMI-START
start /MAX "C:\Program Files\Internet Explorer\iexplore.exe" "resources\trabajando.png"
:inicio

CHOICE /C N /N /T 3 /D N /M "Espero 3 segundos"
SCHTASKS /Run /TN UV5K-HMI-START

:test
CHOICE /C N /N /T 5 /D N /M "Espero 5 segundos"
@ECHO off
SCHTASKS /Query /TN UV5K-HMI-START|find "En ejecución" >hmi.txt
more hmi.txt

set "wordToCheck=ejecutándose"ejecutándose
set "filePath=hmi.txt"

set "found="
for /f %%A in ('type "%filePath%" ^| find "%wordToCheck%"') do set "found=1"

if defined found (goto :INVALID) else (goto :VALID)


:INVALID
echo "espero un rato"
goto :inicio

:VALID
echo size=%size%
echo "Todo correcto"
del hmi.txt

taskkill /F /IM dllhost.exe


SCHTASKS.EXE /CREATE /TN "UV5K-HMI-START" /F /XML "UV5K-HMI-START.XML"