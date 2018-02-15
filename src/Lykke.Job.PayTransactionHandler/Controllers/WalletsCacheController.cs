using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Job.PayTransactionHandler.Controllers
{
    [Route("api/[controller]")]
    public class WalletsCacheController : Controller
    {
        private readonly IWalletsStateCacheManager _walletsStateCacheManager;
        private readonly ILog _log;

        public WalletsCacheController(IWalletsStateCacheManager walletsStateCacheManager, ILog log)
        {
            _walletsStateCacheManager = walletsStateCacheManager ??
                                        throw new ArgumentNullException(nameof(walletsStateCacheManager));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [HttpGet]
        [SwaggerOperation("GetCacheState")]
        [ProducesResponseType(typeof(IEnumerable<WalletState>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetCacheState()
        {
            try
            {
                return Ok(await _walletsStateCacheManager.GetState());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(WalletsCacheController), nameof(GetCacheState), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
