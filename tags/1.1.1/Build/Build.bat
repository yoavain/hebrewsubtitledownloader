@echo off
cls
Title Building HebrewSubtitleDownloader
cd ..

cd SubsCenterOrg
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=RELEASE SubsCenterOrg.csproj
cd ..

cd Build