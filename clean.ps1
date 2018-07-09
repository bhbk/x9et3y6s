
Remove-Item *.nupkg

cd Bhbk.Cli.Identity
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.Dal.Identity.Mssql
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.Lib.Identity
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.WebApi.Identity.Admin
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.WebApi.Identity.Admin.Tests
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.WebApi.Identity.Me
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.WebApi.Identity.Me.Tests
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.WebApi.Identity.Sts
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd ..\Bhbk.WebApi.Identity.Sts.Tests
if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force }
if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force }

cd..
