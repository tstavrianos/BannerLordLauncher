@echo off

dotnet publish -c Release -p:PublishSingleFile=true -p:AssemblyVersion=%1.0 -p:FileVersion=%1.0 -p:Version=%1
cd bin\Release\netcoreapp3.1\win-x64\publish\
if exist BannerLordLauncher.7z del BannerLordLauncher.7z
if exist BannerLordLauncher.exe "c:\Program Files\7-Zip\7z.exe" a BannerLordLauncher.7z BannerLordLauncher.exe BannerLordLauncher.pdb
cd ..\..\..\..\..