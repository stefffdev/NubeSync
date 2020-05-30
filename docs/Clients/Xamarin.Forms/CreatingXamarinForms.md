# Creating a Xamarin Forms client app
This documentation assumes that you have a working Xamarin Forms app.
We will add the NubeSync client framework and a SQLite storage for the offline cache.

## Add the packages
Add the **NubeSync.Client.SQLiteStore** nuget package to all projects, including the client projects.

## Create the DTO
Similar to the server project the DTO (Data Transfer Object) contains the structure of the records to be synced with the server.
1. Add a file **TodoItem.cs** to the project
2. Add the following using:
```C#
using Nube.Client.Data;
```
3. Add the following content to the class:
```C#
public class TodoItem : NubeTable
{
    public string Name { get; set; }

    public bool IsChecked { get; set; }
}
```

## Create the data store
The NubeSync framework is using Microsoft Entity Framework Core to store the client cache.
1. Add a file **MyDataStore.cs** to the project
2. Add the following usings:
```C#
using Nube.Client.SQLiteStore;
using Microsoft.EntityFrameworkCore;
```
3. Add the following content to the class:
```C#
public class MyDataStore : NubeSQLiteDataStore
{
    public MyDataStore()
    {

    }

    public MyDataStore(string databasePath) : base(databasePath)
    {

    }

    public DbSet<TodoItem> TodoItem { get; set; }
}
```
4. Build your app

## Enable Database Migrations
Now comes the tricky part, since Entity Framework Core migrations cannot be run on .NET standard projects (because .NET standard is a contract and not a runtime) or Xamarin projects we have to change our shared project to multi-target .NET Core.
See [https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet#other-target-frameworks](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet#other-target-frameworks) and [https://stackoverflow.com/a/44434134](https://stackoverflow.com/a/44434134) for further information.

1. Edit the csproj file or your shared project and find the line stating the target framework:
```csproj
<TargetFramework>netstandard2.0</TargetFramework>
```
2. Change that line to also target .NET Core 
```csproj
<TargetFrameworks>netcoreapp2.0;netstandard2.0</TargetFrameworks>
```
3. Restart Visual Studio
4. Install the nuget packages **Microsoft.EntityFrameworkCore.Design** and **Microsoft.EntityFrameworkCore.Tools**
5. Open a console window in the directory of your shared project and create the migration:
´´´console
dotnet ef migrations add Initial
´´´
This will create a folder **Migrations** in your project, you have to add a migration every time you change or add something on your DTOs. But there is no need to apply those migrations to the client database, as this is done by the NubeSync client on initialization.