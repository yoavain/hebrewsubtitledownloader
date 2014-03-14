@echo off
cls
Title Building HebrewSubtitleDownloader
cd ..

cd SratimUtils
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /target:Rebuild /property:Configuration=RELEASE SratimUtils.csproj
cd ..
cd Sratim
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /target:Rebuild /property:Configuration=RELEASE Sratim.csproj
cd ..
cd SubsCenterOrg
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /target:Rebuild /property:Configuration=RELEASE SubsCenterOrg.csproj
cd ..
cd HebrewSubtitleDownloader
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" /target:Rebuild /property:Configuration=RELEASE HebrewSubtitleDownloader.csproj
cd ..

echo +++++ Merging +++++
cd HebrewSubtitleDownloader\bin\Release
if exist HebrewSubtitleDownloader_UNMERGED.dll del HebrewSubtitleDownloader_UNMERGED.dll
ren HebrewSubtitleDownloader.dll HebrewSubtitleDownloader_UNMERGED.dll
..\..\..\build\Tools\ilmerge.exe /out:HebrewSubtitleDownloader.dll HebrewSubtitleDownloader_UNMERGED.dll SratimUtils.dll /target:dll /targetplatform:"v4,%WINDIR%\Microsoft.NET\Framework\v4.0.30319"
cd ..\..\..
cd Sratim\bin\Release
if exist Sratim_UNMERGED.dll del Sratim_UNMERGED.dll
ren Sratim.dll Sratim_UNMERGED.dll
..\..\..\build\Tools\ilmerge.exe /out:Sratim.dll Sratim_UNMERGED.dll SratimUtils.dll /target:dll /targetplatform:"v4,%WINDIR%\Microsoft.NET\Framework\v4.0.30319"
cd ..\..\..

cd Build