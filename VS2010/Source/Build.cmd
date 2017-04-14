@echo off

set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe

"%msbuild%" UIMapToolbox.sln /t:Rebuild /p:Configuration=Release

rd /s /q BuildOutput
xcopy UIMapToolbox.VSIX\bin\Release\UIMapToolbox.vsix BuildOutput\VSIX\
xcopy UIMapToolbox.UI\bin\Release\*.dll BuildOutput\Binaries\
xcopy UIMapToolbox.UI\bin\Release\*.exe BuildOutput\Binaries\
xcopy UIMapToolbox.UI\bin\Release\Samples\*.* BuildOutput\Binaries\Samples\

pause