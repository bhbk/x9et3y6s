
rem call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\VsDevCmd.bat"
rem dotnet tool install Octopus.DotNet.Cli --global --version 5.2.2
powershell -command "& { get-date -format yyyy.M.d.HHmm | out-file -filepath .\version.tmp -nonewline -encoding ascii }"
set /p VERSION=< .\version.tmp

dotnet restore Bhbk.WebApi.Identity.sln --no-cache --verbosity quiet
dotnet build Bhbk.WebApi.Identity.sln --configuration Release --verbosity quiet

rem dotnet test Bhbk.Lib.Identity.Domain.Tests\Bhbk.Lib.Identity.Domain.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\
rem dotnet test Bhbk.WebApi.Alert.Tests\Bhbk.WebApi.Alert.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\
rem dotnet test Bhbk.WebApi.Identity.Admin.Tests\Bhbk.WebApi.Identity.Admin.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\
rem dotnet test Bhbk.WebApi.Identity.Me.Tests\Bhbk.WebApi.Identity.Me.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\
rem dotnet test Bhbk.WebApi.Identity.Sts.Tests\Bhbk.WebApi.Identity.Sts.Tests.csproj --configuration Release /p:CollectCoverage=true /p:CoverletOutput=bin\Release\

dotnet pack Bhbk.Lib.Identity\Bhbk.Lib.Identity.csproj -p:PackageVersion=%VERSION% --output . --configuration Release
dotnet publish Bhbk.Cli.Identity\Bhbk.Cli.Identity.csproj --output Bhbk.Cli.Identity\bin\Release\netcoreapp3.1\publish\ --configuration Release
dotnet publish Bhbk.WebApi.Alert\Bhbk.WebApi.Alert.csproj --output Bhbk.WebApi.Alert\bin\Release\netcoreapp3.1\publish\ --configuration Release
dotnet publish Bhbk.WebApi.Identity.Admin\Bhbk.WebApi.Identity.Admin.csproj --output Bhbk.WebApi.Identity.Admin\bin\Release\netcoreapp3.1\publish\ --configuration Release
dotnet publish Bhbk.WebApi.Identity.Me\Bhbk.WebApi.Identity.Me.csproj --output Bhbk.WebApi.Identity.Me\bin\Release\netcoreapp3.1\publish\ --configuration Release
dotnet publish Bhbk.WebApi.Identity.Sts\Bhbk.WebApi.Identity.Sts.csproj --output Bhbk.WebApi.Identity.Sts\bin\Release\netcoreapp3.1\publish\ --configuration Release
dotnet octo pack --id=Bhbk.Cli.Identity --version=%VERSION% --basePath=Bhbk.Cli.Identity\bin\Release\netcoreapp3.1\publish\ --outFolder=.
dotnet octo pack --id=Bhbk.WebApi.Alert --version=%VERSION% --basePath=Bhbk.WebApi.Alert\bin\Release\netcoreapp3.1\publish\ --outFolder=.
dotnet octo pack --id=Bhbk.WebApi.Identity.Admin --version=%VERSION% --basePath=Bhbk.WebApi.Identity.Admin\bin\Release\netcoreapp3.1\publish\ --outFolder=.
dotnet octo pack --id=Bhbk.WebApi.Identity.Me --version=%VERSION% --basePath=Bhbk.WebApi.Identity.Me\bin\Release\netcoreapp3.1\publish\ --outFolder=.
dotnet octo pack --id=Bhbk.WebApi.Identity.Sts --version=%VERSION% --basePath=Bhbk.WebApi.Identity.Sts\bin\Release\netcoreapp3.1\publish\ --outFolder=.

rem dotnet tool uninstall Octopus.DotNet.Cli --global
set VERSION=

rem powershell -command "& { update-package -reinstall }"