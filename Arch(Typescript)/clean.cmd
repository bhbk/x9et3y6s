
powershell -command "& { Remove-Item *.nupkg }"
powershell -command "& { Remove-Item *.tmp }"

powershell -command "& { if (test-path .\dist) { remove-item .\dist -recurse -force } }"
rem powershell -command "& { if (Test-Path .\node_modules) { Remove-Item .\node_modules -Recurse -Force } }"
