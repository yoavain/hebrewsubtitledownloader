@echo off
cls
Title Building HebrewSubtitleDownloader
cd ..

cd SubsCenterOrg
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG SubsCenterOrg.csproj
cd ..
cd Sratim
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG Sratim.csproj
cd ..

cd Build