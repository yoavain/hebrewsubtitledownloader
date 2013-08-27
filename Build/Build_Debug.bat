@echo off
cls
Title Building HebrewSubtitleDownloader
cd ..

cd SratimUtils
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG SratimUtils.csproj
cd ..
cd Sratim
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG Sratim.csproj
cd ..
cd SubsCenterOrg
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG SubsCenterOrg.csproj
cd ..
cd HebrewSubtitleDownloader
"%WINDIR%\Microsoft.NET\Framework\v3.5\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG HebrewSubtitleDownloader.csproj
cd ..

echo +++++ Merging +++++
cd HebrewSubtitleDownloader\bin\Debug
if exist HebrewSubtitleDownloader_UNMERGED.dll del HebrewSubtitleDownloader_UNMERGED.dll
ren HebrewSubtitleDownloader.dll HebrewSubtitleDownloader_UNMERGED.dll
..\..\..\build\Tools\ilmerge.exe /out:HebrewSubtitleDownloader.dll HebrewSubtitleDownloader_UNMERGED.dll SratimUtils.dll
cd ..\..\..
cd Sratim\bin\Debug
if exist Sratim_UNMERGED.dll del Sratim_UNMERGED.dll
ren Sratim.dll Sratim_UNMERGED.dll
..\..\..\build\Tools\ilmerge.exe /out:Sratim.dll Sratim_UNMERGED.dll SratimUtils.dll
cd ..\..\..

cd Build