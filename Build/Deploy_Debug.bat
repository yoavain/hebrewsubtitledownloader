@echo off
cls
Title Deploying HebrewSubtitleDownloader
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

copy /y HebrewSubtitleDownloader\bin\Debug\HebrewSubtitleDownloader.* "%PROGS%\Team MediaPortal\MediaPortal\plugins\Windows\"
copy /y SubsCenterOrg\bin\Debug\SubsCenterOrg.* "%PROGS%\Team MediaPortal\MediaPortal\SubtitleDownloaders\"
copy /y Sratim\bin\Debug\Sratim.* "%PROGS%\Team MediaPortal\MediaPortal\SubtitleDownloaders\"

cd Build
