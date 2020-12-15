using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NubeSync.Core;
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
        private readonly IChangeTracker _changeTracker;

        public TodoItemsController(
            IAuthentication authentication,
            IOperationService operationService,
            IChangeTracker changeTracker,
            DataContext context)
        {
            _authentication = authentication;
            _operationService = operationService;
            _changeTracker = changeTracker;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetItems(
            DateTimeOffset? laterThan, 
            int? pageNumber,
            int? pageSize)        
        {
            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //HttpContext.VerifyUserHasAnyAcceptedScope(_authentication.ScopeRequiredByApi);

            var userId = _authentication.GetUserIdentifier(User);
            var installationId = Request.GetInstallationId();

            var tableName = typeof(TodoItem).Name;

            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //if (laterThan.HasValue)
            //{
            //    return await _context.TodoItems
            //        .Where(i => i.UserId == userId && i.ServerUpdatedAt >= laterThan)
            //        .Skip((pageNumber.Value - 1) * pageSize.Value)
            //        .Take(pageSize.Value)
            //        .ToListAsync();
            //}
            //else
            //{
            //    return await _context.TodoItems.Where(i => i.UserId == userId).ToListAsync();
            //}

            if (laterThan.HasValue)
            {
                return await _context.TodoItems
                    .Where(i => i.ServerUpdatedAt >= laterThan)
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync();
            }
            else
            {
                return await _context.TodoItems
                    .Where(i => !i.DeletedAt.HasValue)
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> Get(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return NotFound();
                }

                return Ok(await _context.TodoItems.FindAsync(id));
            }
            catch (Exception)
            {
                // log exception here
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(TodoItem item)
        {
            string userId = _authentication.GetUserIdentifier(User);
            string installationId = Request.GetInstallationId();

            try
            {
                var operations = await _changeTracker.TrackAddAsync(item);
                await _operationService.ProcessOperationsAsync(_context, operations, userId, installationId);

                return Ok();
            }
            catch (Exception)
            {
                // log exception here
                return StatusCode(500);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(TodoItem item)
        {
            string userId = _authentication.GetUserIdentifier(User);
            string installationId = Request.GetInstallationId();

            try
            {
                var currentItem = await _context.TodoItems.FindAsync(item.Id);
                List<NubeOperation> operations = await _changeTracker.TrackModifyAsync(currentItem, item);
                await _operationService.ProcessOperationsAsync(_context, operations, userId, installationId);

                return Ok();
            }
            catch (Exception)
            {
                // log exception here
                return StatusCode(500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            string userId = _authentication.GetUserIdentifier(User);
            string installationId = Request.GetInstallationId();

            try
            {
                var item = await _context.TodoItems.FindAsync(id);
                if (item != null)
                {
                    List<NubeOperation> operations = await _changeTracker.TrackDeleteAsync(item);
                    await _operationService.ProcessOperationsAsync(_context, operations, userId, installationId);
                }
                return Ok();
            }
            catch (Exception)
            {
                // log exception here
                return StatusCode(500);
            }
        }
    }
}
