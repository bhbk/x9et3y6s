cd Bhbk.Cli.Identity
dotnet publish "Bhbk.Cli.Identity.csproj" --output "bin\\Release\\netcoreapp2.1\\publish\\" --configuration Release --framework netcoreapp2.1
octo pack --id="Bhbk.Cli.Identity" --basePath="bin\\Release\\netcoreapp2.1\\publish\\" --outFolder="..\\"

cd ..\Bhbk.WebApi.Identity.Admin
dotnet publish "Bhbk.WebApi.Identity.Admin.csproj" --output "bin\\Release\\netcoreapp2.1\\publish\\" --configuration Release --framework netcoreapp2.1
octo pack --id="Bhbk.WebApi.Identity.Admin" --basePath="bin\\Release\\netcoreapp2.1\\publish\\" --outFolder="..\\"

cd ..\Bhbk.WebApi.Identity.Me
dotnet publish "Bhbk.WebApi.Identity.Me.csproj" --output "bin\\Release\\netcoreapp2.1\\publish\\" --configuration Release --framework netcoreapp2.1
octo pack --id="Bhbk.WebApi.Identity.Me" --basePath="bin\\Release\\netcoreapp2.1\\publish\\" --outFolder="..\\"

cd ..\Bhbk.WebApi.Identity.Sts
dotnet publish "Bhbk.WebApi.Identity.Sts.csproj" --output "bin\\Release\\netcoreapp2.1\\publish\\" --configuration Release --framework netcoreapp2.1
octo pack --id="Bhbk.WebApi.Identity.Sts" --basePath="bin\\Release\\netcoreapp2.1\\publish\\" --outFolder="..\\"