### Make targets of this file by Windows NMAKE tool.
### NMAKE tool is installed alongside Visual Studio 2022, for example.
### By Windows start menu, launch:
### `Developer Command Prompt for VS 2022`

pull-beso:
	cd %userprofile%\go\src\BESO\RhinoCommon\BESO\BESO\RuntimeEnv\beso
	xcopy /D /Y /E /H /I %userprofile%\repos\beso\.git .\.git
	git checkout .
	git pull origin master
	rmdir /s /q wiki_files
	rmdir /s /q .git

build:
	cd %userprofile%\go\src\BESO\RhinoCommon\BESO
	dotnet build BESO.sln -c Release -f net48 -p:Version=1.0.6.0

# To delete outdated files.
clean:
	cd %userprofile%\go\src\BESO\RhinoCommon\pkg\dist
	del *.yak 2> NUL
	del *.rhp 2> NUL
	del *.dll 2> NUL
	del *.exe 2> NUL
	del *.bat 2> NUL
	del NULL 2> NUL
	del *.fbd 2> NUL
	del *.pdb 2> NUL
	rmdir /s /q beso> NUL

# To create the manifest file, if `manifest.yml` doesn't exist already.
# If manifest exists, the version must be incremented before each push to server.
manifest:
	"C:\Program Files\Rhino 7\System\Yak.exe" spec

# Install the binaries inside the distribution folder.
install:
	cd %userprofile%\go\src\BESO\RhinoCommon\pkg\dist
	xcopy /E "%userprofile%\go\src\BESO\RhinoCommon\BESO\BESO\bin\Release\net48\*" .>NUL
	del BESO.pdb

# To create plugin package.
pkg:
	cd %userprofile%\go\src\BESO\RhinoCommon\pkg\dist
	"C:\Program Files\Rhino 7\System\Yak.exe" build --platform win

# To log in before push to server, if not already logged in:
login:
	"C:\Program Files\Rhino 7\System\Yak.exe" login

# To push plugin package to Rhino3D server.
push:
	cd %userprofile%\go\src\BESO\RhinoCommon\pkg\dist
	echo off > temp.bat
    for /f "delims=" %%i in ('dir /b /od /a-d beso-*-win.yak') do echo "C:\Program Files\Rhino 7\System\Yak.exe" push "%%i" > temp.bat
    call temp.bat
    del temp.bat

# To search for plugin on Rhino3D server.
search:
	"C:\Program Files\Rhino 7\System\Yak.exe" search --all --prerelease BESO

# To create a ZIP file to be uploaded to Patreon shop.
zip:
	cd %userprofile%\go\src\BESO\RhinoCommon\pkg
	powershell Compress-Archive -Force dist BESO.zip

# To create and distribute the release as a Rhino3D plugin package.
distribute-server: build clean install pkg push search

# To create and distribute the release as a Rhino3D plugin ZIP file.
distribute: build clean install pkg zip
