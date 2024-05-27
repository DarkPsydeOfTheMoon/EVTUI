@echo OFF
cd /D "%~dp0"

:: cd back up to root
cd ..

:: Grab dependencies
git submodule update --init --recursive

:: Apply XV2-Tools patch
echo Applying patch to XV2-Tools...
cd external/XV2-Tools
git apply --reject --whitespace=fix ../../patches/XV2-Tools.patch
cd ..
cd ..

echo Applying patch to GFDLibrary.Rendering.OpenGL...
cd external/GFD-Studio/GFDLibrary.Rendering.OpenGL
git apply --reject --whitespace=fix ../../../patches/GFDLibrary.Rendering.OpenGL.patch
cd ..
cd ..
cd ..

echo Finished patching dependencies.
