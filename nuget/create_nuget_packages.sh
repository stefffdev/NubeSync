#!/bin/bash

dotnet pack ../NubeSync.sln -c Release
mkdir ../publish

yes | cp -rf ../src/NubeSync.Client/bin/Release/*nupkg ../publish
yes | cp -rf ../src/NubeSync.Client.SQLiteStore/bin/Release/*nupkg ../publish
yes | cp -rf ../src/NubeSync.Core/bin/Release/*nupkg ../publish
yes | cp -rf ../src/NubeSync.Server/bin/Release/*nupkg ../publish

pause