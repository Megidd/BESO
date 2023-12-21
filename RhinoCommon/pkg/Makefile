### Make targets of this file by Windows NMAKE tool.
### NMAKE tool is installed alongside Visual Studio 2022, for example.
### By Windows start menu, launch:
### `Developer Command Prompt for VS 2022`

# To delete outdated files.
clean:
	cd %userprofile%\go\src\BESO\RhinoCommon\pkg\dist
	del beso-*.yak 2> NUL
	del BESO.rhp 2> NUL
	del System* 2> NUL
	del Microsoft* 2> NUL
	del temp.bat 2> NUL
	del NULL 2> NUL

# To create the manifest file, if `manifest.yml` doesn't exist already.
# If manifest exists, the version must be incremented before each push to server.
manifest:
	"C:\Program Files\Rhino 7\System\Yak.exe" spec

# Install the binaries inside the distribution folder.
install:
	cd %userprofile%\go\src\BESO\RhinoCommon\pkg\dist
	copy "%userprofile%\go\src\BESO\RhinoCommon\BESO\BESO\bin\Release\net48\*" .
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

# To create and distribute the trial release as a Rhino3D plugin package.
distribute-trial: clean trial install pkg push search

# To create and distribute the release as a Rhino3D plugin ZIP file.
distribute: clean install zip
