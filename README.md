# NubeSync
NubeSync is a offline data sync framework designed for .NET applications.

## Features
* Fast operations based transmission of the changes to the server
* Incremental sync when downloading new and changed records from the server
* Automatic merge conflict resolution (last change wins) on per field basis is performed on the server
* Based on standard technologies (ASP.NET Core, SQLite, REST)
* Client side indexes and encryption are supported 
* Server side storage can use all Entity Framework Core compatible storages (e.g. Microsoft SQL, CosmosDB)
* NubeSync can be implemented as a successor to the Azure Mobile App Service offline sync capabilities

## Documentation
See the [Wiki page](https://github.com/stefffdev/NubeSync/wiki)

## Repositories
### Client SDK
* https://github.com/stefffdev/NubeSync.Client - Client SDK for .NET platforms

### Server SDK
* https://github.com/stefffdev/NubeSync.Server - Server SDK for .NET platforms

### Samples
* https://github.com/stefffdev/NubeSync.Samples - Full end-to-end samples of a backend service with authentication and (mobile) client apps
