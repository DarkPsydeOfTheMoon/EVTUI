@echo OFF
cd /D "%~dp0"

:: cd back up to root
cd ..

:: Grab dependencies
git submodule update --init --recursive

echo Applying patch to GFDLibrary.Rendering.OpenGL...
cd external/GFD-Studio/GFDLibrary.Rendering.OpenGL
git apply --reject --whitespace=fix ../../../patches/GFDLibrary.Rendering.OpenGL.patch
cd ..
cd ..
echo Applying patch to Atlus-Script-Tools...
cd Atlus-Script-Tools
git apply --reject --whitespace=fix ../../../patches/Atlus-Script-Tools.patch
cd ..
cd ..

echo Finished patching dependencies.
