# Creating a ASP.NET Core Backend
This documentation assumes that you already have a ASP.NET Core WebApi project with a working database connection via the Entity Framework Core.
If you're having trouble setting this up, see the docs: [https://dotnet.microsoft.com/apps/aspnet](https://dotnet.microsoft.com/apps/aspnet)

The NubeSync framework is based on the client tracking all changes made to the synced data in "operations", which are pushed to the server, where they are processed to update the database.

## Add the NubeSync framework
1. Install the **NubeSync.Server** nuget package into your project.
2. Register the NubeSync OperationService in the **ConfigureServices** method in the file **Startup.cs**:
```C#
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddTransient<IOperationService, OperationService>();
}
```
3. Add a table for the operations in your DbContext:
```C#
public DbSet<NubeOperation> Operations { get; set; }
```

## Add your DTOs
The DTO (Data Transfer Object) is a class that represents the data you want to sync to the clients.
1. Create a file **TodoItem.cs** in your project and derive it from **NubeTable**:
```C#
public class TodoItem : NubeTable
{
    public string Name { get; set; }

    public bool IsChecked { get; set; }
}
```
2. Add a table for our TodoItem in your DbContext:
```C#
public DbSet<TodoItem> TodoItems { get; set; }
```
3. Create the migrations for the added table and apply them to your database

## Add the Operations controller
1. Create a new file **OperationsController.cs** in the **Controllers** folder
2. Add the following usings to the file:
```C#
using Nube.Server;
using Nube.Server.Data;
```
3. Change the class to:
```C#
    [ApiController]
    [Route("[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IOperationService _operationService;

        public OperationsController(
            DataContext context,
            IOperationService operationService)
        {
            _context = context;
            _operationService = operationService;
        }

        [HttpPost]
        public async Task<IActionResult> PostOperationsAsync(List<NubeOperation> operations)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                await _operationService.ProcessOperationsAsync(_context, operations);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
```

What that code does is read all operations the client posts and update the database accordingly.
All this is done inside a transaction, which means that either all operations succeed or all operations fail. This is so that not only some changes get applied, or even worse only modified operations get applied, but not the initial add operation of this record.


## Add the TodoItem controller
Clients should not only push their changes to the server, but also receive all items, that have changed since the last sync.
1. Create a new file **TodoItemsController.cs** in the **Controllers** folder
2. Change the class to:
```C#
[ApiController]
[Route("[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly DataContext _context;

    public TodoItemsController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetItems(DateTimeOffset? laterThan)
    {
        var tableName = typeof(TodoItem).Name;

        if (laterThan.HasValue)
        {
            return await _context.TodoItems.Where(i => i.ServerUpdatedAt >= laterThan).ToListAsync();
        }
        else
        {
            return await _context.TodoItems.ToListAsync();
        }
    }
}
```
All we do here is return all the items that have a **ServerUpdatedAt** timestamp later than the timestamp the client sent. This is necessary to implement an **incremental sync** where the client only receives records that were changed since the last sync.
If the client does not include a timestamp in the request all records are returned.


That is all that has to be done at the server side.