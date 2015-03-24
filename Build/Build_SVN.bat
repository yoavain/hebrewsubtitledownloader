@echo off
cls
Title Building HebrewSubtitleDownloader
cd ..

:: Prepare version
subwcrev . HebrewSubtitleDownloader\Properties\AssemblyInfo.cs HebrewSubtitleDownloader\Properties\AssemblyInfo.cs
subwcrev . HebrewSubtitleDownloaderTest\Properties\AssemblyInfo.cs HebrewSubtitleDownloaderTest\Properties\AssemblyInfo.cs
subwcrev . Sratim\Properties\AssemblyInfo.cs Sratim\Properties\AssemblyInfo.cs
subwcrev . SratimTest\Properties\AssemblyInfo.cs SratimTest\Properties\AssemblyInfo.cs
subwcrev . SratimUtils\Properties\AssemblyInfo.cs SratimUtils\Properties\AssemblyInfo.cs
subwcrev . SubsCenterOrg\Properties\AssemblyInfo.cs SubsCenterOrg\Properties\AssemblyInfo.cs
subwcrev . SubsCenterOrgTest\Properties\AssemblyInfo.cs SubsCenterOrgTest\Properties\AssemblyInfo.cs

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

:: Revert version
svn revert HebrewSubtitleDownloader\Properties\AssemblyInfo.cs           
svn revert HebrewSubtitleDownloaderTest\Properties\AssemblyInfo.cs       
svn revert Sratim\Properties\AssemblyInfo.cs                             
svn revert SratimTest\Properties\AssemblyInfo.cs                         
svn revert SratimUtils\Properties\AssemblyInfo.cs                        
svn revert SubsCenterOrg\Properties\AssemblyInfo.cs                      
svn revert SubsCenterOrgTest\Properties\AssemblyInfo.cs                  

cd Build