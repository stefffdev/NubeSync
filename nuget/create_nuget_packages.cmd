dotnet pack ..\NubeSync.sln -c Release
mkdir ..\publish
copy /y ..\src\NubeSync.Client\bin\Release\*nupkg ..\publish
copy /y ..\src\NubeSync.Client.SQLiteStore\bin\Release\*nupkg ..\publish
copy /y ..\src\NubeSync.Core\bin\Release\*nupkg ..\publish
copy /y ..\src\NubeSync.Server\bin\Release\*nupkg ..\publish

pause