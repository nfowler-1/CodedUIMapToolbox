@echo off

set msbuild=C:\Program Files (x86)\MSBuild\15.0\bin\MSBuild.exe

"%msbuild%" UIMapToolbox.sln /t:Rebuild /p:Configuration=Release "/p:Platform=Any CPU"

rd /s /q BuildOutput
xcopy UIMapToolbox.VSIX\bin\Release\UIMapToolbox.vsix BuildOutput\VSIX\
xcopy UIMapToolbox.UI\bin\Release\*.dll BuildOutput\Binaries\
xcopy UIMapToolbox.UI\bin\Release\*.exe BuildOutput\Binaries\
xcopy UIMapToolbox.UI\bin\Release\Samples\*.* BuildOutput\Binaries\Samples\

pause