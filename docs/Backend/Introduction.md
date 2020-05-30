# Introduction
The NubeSync server framework is designed to work with a Entity Framework Core DbContext and can therefore use all the storage types supported by Entity Framework Core, such as Microsoft SQL or CosmosDb.

The most common way to use the NubeSync framework is within a ASP.NET Core WebApi site, although other hosting options would be possible (such as a single process, ASP.NET classic etc.)

This documentation is focused on an ASP.NET Core site using a Microsoft SQL database for storage.

## Samples
Working samples for this documentation can be found under [https://github.com/stefffdev/NubeSync.Samples](https://github.com/stefffdev/NubeSync.Samples)