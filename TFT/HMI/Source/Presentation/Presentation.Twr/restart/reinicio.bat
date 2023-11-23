SCHTASKS /end /TN UV5K-HMI-START
start /MAX "C:\Program Files\Internet Explorer\iexplore.exe" "trabajando.png"
:inicio

CHOICE /C N /N /T 3 /D N /M "Espero 3 segundos"
SCHTASKS /Run /TN UV5K-HMI-START

:test
CHOICE /C N /N /T 5 /D N /M "Espero 5 segundos"
@ECHO off
SCHTASKS /Query /TN UV5K-HMI-START|find "Running" >hmi.txt
for %%A in (hmi.txt) do set size=%%~zA
if %size%==0 (goto :INVALID) else (goto :VALID)

:INVALID
echo "espero un rato"
goto :inicio

:VALID
echo size=%size%
echo "Todo correcto"
del hmi.txt

taskkill /F /IM dllhost.exe


SCHTASKS.EXE /CREATE /TN "UV5K-HMI-START" /F /XML "UV5K-HMI-START.XML"