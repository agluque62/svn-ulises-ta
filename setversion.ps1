Write-Host "Searching ..."
$configFiles = Get-ChildItem . AssemblyInfo.cs -rec
foreach ($file in $configFiles)
{
    (Get-Content $file.PSPath) |
    Foreach-Object { $_ -replace "2.5.5", "2.5.6" } |
    Set-Content $file.PSPath
}
Write-Host "Press any key to continue ..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
