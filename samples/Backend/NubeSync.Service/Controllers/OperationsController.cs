using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Nube.SampleService.Hubs;
using NubeSync.Server;
using NubeSync.Server.Data;
using NubeSync.Service.Data;

namespace NubeSync.Service.Controllers
{
    // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IOperationService _operationService;
        private readonly IHubContext<UpdateHub> _hubContext;
        private readonly IAuthentication _authentication;

        public OperationsController(
            IAuthentication authentication,
            DataContext context,
            IOperationService operationService,
            IHubContext<UpdateHub> hubContext)
        {
            _authentication = authentication;
            _context = context;
            _operationService = operationService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostOperationsAsync(List<NubeOperation> operations)
        {
            // UNCOMMENT THIS IF YOU WANT TO ACTIVATE AUTHENTICATION
            //HttpContext.VerifyUserHasAnyAcceptedScope(_authentication.ScopeRequiredByApi);
            var userId = _authentication.GetUserIdentifier(User);

            var installationId = Request.GetInstallationId();

            try
            {
                operations.ForEach(o =>
                {
                    o.UserId = userId;
                    o.InstallationId = installationId;
                });

                await _operationService.ProcessOperationsAsync(_context, operations);
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                return BadRequest(ex.Message);
            }

            await _hubContext.Clients.All.SendAsync("Update", "user", "message");
            return Ok();
        }
    }
}
