
powershell -command "& { Remove-Item *.nupkg }"
powershell -command "& { Remove-Item *.tmp }"
powershell -command "& { if (Test-Path .\TestResults) { Remove-Item .\TestResults -Recurse -Force } }"
rem powershell -command "& { if (Test-Path .\Packages) { Remove-Item .\Packages -Recurse -Force } }"

pushd Bhbk.Cli.Alert
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Cli.Identity
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Lib.Identity
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Lib.Identity.Data.EF6
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Lib.Identity.Data.EF6.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Lib.Identity.Data.EFCore
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Lib.Identity.Data.EFCore.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Lib.Identity.Domain
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.Mssql.Identity
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
rem powershell -command "& { Remove-Item *.dbmdl }"
popd

pushd Bhbk.WebApi.Alert
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"
popd

pushd Bhbk.WebApi.Alert.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.WebApi.Identity.Admin
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"
popd

pushd Bhbk.WebApi.Identity.Admin.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.WebApi.Identity.Me
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"
popd

pushd Bhbk.WebApi.Identity.Me.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd

pushd Bhbk.WebApi.Identity.Sts
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
powershell -command "& { if (Test-Path .\*.log) { Remove-Item .\*.log -Recurse -Force } }"
popd

pushd Bhbk.WebApi.Identity.Sts.Tests
powershell -command "& { if (Test-Path .\bin) { Remove-Item .\bin -Recurse -Force } }"
powershell -command "& { if (Test-Path .\obj) { Remove-Item .\obj -Recurse -Force } }"
popd
