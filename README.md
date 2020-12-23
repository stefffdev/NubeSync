# NubeSync
[![License](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/stefffdev/NubeSync/blob/master/LICENSE)
![GitHub last commit](https://img.shields.io/github/last-commit/stefffdev/NubeSync)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/stefffdev/NubeSync)
![GitHub Repo stars](https://img.shields.io/github/stars/stefffdev/NubeSync)

NubeSync is a bi-directional offline data sync framework

## Features
* Fast operations based transmission of the changes to the server
* Incremental sync when downloading new and changed records from the server
* Automatic merge conflict resolution (last change wins) on per field basis is performed on the server
* Based on standard technologies (ASP.NET Core, SQLite, REST)
* Client side indexes and encryption are supported 
* Server side storage can use all Entity Framework Core compatible storages (e.g. Microsoft SQL, CosmosDB)
* NubeSync can be implemented as a successor to the Azure Mobile App Service offline sync capabilities

## Supported Platforms
* Every platform that supports .NET Standard 2.0 (Xamarin, Blazor, UWP, WPF, ...)
* Flutter

## Documentation & Getting Started
See the [Wiki page](https://github.com/stefffdev/NubeSync/wiki) for getting started and have a look at the [samples](https://github.com/stefffdev/NubeSync/tree/master/samples).

For some more detailed infos on certain topics check out our blog over at  [https://www.lakedice.com/blog/category/Sync](https://www.lakedice.com/blog/category/Sync)
