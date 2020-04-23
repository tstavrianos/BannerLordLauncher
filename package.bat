@echo off
msbuild BannerLordLauncher.sln /p:Configuration=Release;VersionAssembly=%1.0;Platform="Any CPU"
cd BannerLordLauncher\bin\Release\
if exist BannerLordLauncher.7z del BannerLordLauncher.7z
if exist BannerLordLauncher.exe "c:\Program Files\7-Zip\7z.exe" a BannerLordLauncher.7z BannerLordLauncher.exe BannerLordLauncher.pdb
cd ..\..\..