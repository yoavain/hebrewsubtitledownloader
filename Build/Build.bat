@echo off
cls
Title Building HebrewSubtitleDownloader
cd ..

: Prepare version
for /f "tokens=*" %%a in ('git rev-list HEAD --count') do set REVISION=%%a 
set REVISION=%REVISION: =%
sed -i "s/\$WCREV\$/%REVISION%/g" HebrewSubtitleDownloader\Properties\AssemblyInfo.cs
sed -i "s/\$WCREV\$/%REVISION%/g" HebrewSubtitleDownloaderTest\Properties\AssemblyInfo.cs
sed -i "s/\$WCREV\$/%REVISION%/g" Sratim\Properties\AssemblyInfo.cs
sed -i "s/\$WCREV\$/%REVISION%/g" SratimTest\Properties\AssemblyInfo.cs
sed -i "s/\$WCREV\$/%REVISION%/g" SratimUtils\Properties\AssemblyInfo.cs
sed -i "s/\$WCREV\$/%REVISION%/g" SubsCenterOrg\Properties\AssemblyInfo.cs
sed -i "s/\$WCREV\$/%REVISION%/g" SubsCenterOrgTest\Properties\AssemblyInfo.cs

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

: Revert version
git checkout HebrewSubtitleDownloader\Properties\AssemblyInfo.cs           
git checkout HebrewSubtitleDownloaderTest\Properties\AssemblyInfo.cs       
git checkout Sratim\Properties\AssemblyInfo.cs                             
git checkout SratimTest\Properties\AssemblyInfo.cs                         
git checkout SratimUtils\Properties\AssemblyInfo.cs                        
git checkout SubsCenterOrg\Properties\AssemblyInfo.cs                      
git checkout SubsCenterOrgTest\Properties\AssemblyInfo.cs                  

cd Build