using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NubeSync.Server;
using NubeSync.Service.Data;
using NubeSync.Service.DTO;

namespace NubeSync.Service.Controllers
{
    // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TodoItemsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IAuthentication _authentication;
        private readonly IOperationService _operationService;

        public TodoItemsController(
            IAuthentication authentication,
            IOperationService operationService,
            DataContext context)
        {
            _authentication = authentication;
            _operationService = operationService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetItems(DateTimeOffset? laterThan)
        {
            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //HttpContext.VerifyUserHasAnyAcceptedScope(_authentication.ScopeRequiredByApi);

            var userId = _authentication.GetUserIdentifier(User);
            var installationId = Request.GetInstallationId();

            var tableName = typeof(TodoItem).Name;

            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //if (laterThan.HasValue)
            //{
            //    var allItems = await _context.TodoItems.Where(
            //        i => i.UserId == userId &&
            //        i.ServerUpdatedAt >= laterThan).ToListAsync();
            //    return allItems.Where(
            //        i => _operationService.LastChangedByOthers(_context, tableName, i.Id, installationId, laterThan.Value)).ToList();
            //}
            //else
            //{
            //    return await _context.TodoItems.Where(i => i.UserId == userId).ToListAsync();
            //}

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
}
