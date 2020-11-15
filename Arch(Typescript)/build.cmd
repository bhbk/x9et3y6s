
pushd Bhbk.WebUi.Identity

rem call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
rem dotnet tool install Octopus.DotNet.Cli --global
powershell -command "& { write-output 2020.11.12.1900 | out-file -filepath .\version.tmp -nonewline -encoding ascii }"
rem powershell -command "& { get-date -format yyyy.M.d.HHmm | out-file -filepath .\version.tmp -nonewline -encoding ascii }"
set /p VERSION=< .\version.tmp

rem execute build phase...
call "%PROGRAMFILES%\nodejs\npm" config set registry https://registry.npmjs.org/
call "%PROGRAMFILES%\nodejs\npm" prune 
call "%PROGRAMFILES%\nodejs\npm" cache verify
call "%PROGRAMFILES%\nodejs\npm" install --silent
call "%APPDATA%\npm\ng" build --prod --output-path ".\dist" --base-href "/ui/identity/" 

rem execute test phase...
rem call "%APPDATA%\npm\ng" test --browsers ChromeHeadless --watch=false

rem execute so chrome will ignore self signed certificates...
rem chrome://flags/#allow-insecure-localhost

popd

rem execute package (less deploy) phase...
dotnet octo pack --id=Bhbk.WebUi.Identity --version=%VERSION% --basePath="Bhbk.WebUi.Identity\dist" --outFolder="." --overwrite

rem execute as live to see extra things...
rem call "%APPDATA%\npm\ng" serve --prod --port 4300 --base-href /ui/identity/ --deploy-url /ui/identity/

set VERSION=

rem dotnet tool install Octopus.DotNet.Cli --global
