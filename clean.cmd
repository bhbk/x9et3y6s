
powershell -command "& { Remove-Item *.nupkg }"
powershell -command "& { Remove-Item *.tmp }"

cd Bhbk.Cli.Identity
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Lib.Identity
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Lib.Identity.Data
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Lib.Identity.Domain
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Lib.Identity.Domain.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.WebApi.Alert
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"

cd ..\Bhbk.WebApi.Alert.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.Mssql.Identity
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { Remove-Item *.dbmdl }"

cd ..\Bhbk.WebApi.Identity.Admin
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"

cd ..\Bhbk.WebApi.Identity.Admin.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.WebApi.Identity.Me
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"

cd ..\Bhbk.WebApi.Identity.Me.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd ..\Bhbk.WebApi.Identity.Sts
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"

cd ..\Bhbk.WebApi.Identity.Sts.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"

cd..
