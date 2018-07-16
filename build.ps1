
Set-Variable -Name version -Value "2018.7.15.3163"

dotnet build "Bhbk.WebApi.Identity.sln" --configuration Release
dotnet pack "Bhbk.Lib.Identity\Bhbk.Lib.Identity.csproj" -p:PackageVersion=$version --output ".." --configuration Release -p:TargetFrameworks=netstandard2.0
dotnet publish "Bhbk.Cli.Identity\Bhbk.Cli.Identity.csproj" --output "bin\Release\netcoreapp2.1\publish" --configuration Release --framework netcoreapp2.1
dotnet publish "Bhbk.WebApi.Identity.Admin\Bhbk.WebApi.Identity.Admin.csproj" --output "bin\Release\netcoreapp2.1\publish" --configuration Release --framework netcoreapp2.1
dotnet publish "Bhbk.WebApi.Identity.Me\Bhbk.WebApi.Identity.Me.csproj" --output "bin\Release\netcoreapp2.1\publish" --configuration Release --framework netcoreapp2.1
dotnet publish "Bhbk.WebApi.Identity.Sts\Bhbk.WebApi.Identity.Sts.csproj" --output "bin\Release\netcoreapp2.1\publish" --configuration Release --framework netcoreapp2.1
octo pack --id="Bhbk.Cli.Identity" --version=$version --basePath="Bhbk.Cli.Identity\bin\Release\netcoreapp2.1\publish" --outFolder="."
octo pack --id="Bhbk.WebApi.Identity.Admin" --version=$version --basePath="Bhbk.WebApi.Identity.Admin\bin\Release\netcoreapp2.1\publish" --outFolder="."
octo pack --id="Bhbk.WebApi.Identity.Me" --version=$version --basePath="Bhbk.WebApi.Identity.Me\bin\Release\netcoreapp2.1\publish" --outFolder="."
octo pack --id="Bhbk.WebApi.Identity.Sts" --version=$version --basePath="Bhbk.WebApi.Identity.Sts\bin\Release\netcoreapp2.1\publish" --outFolder="."
