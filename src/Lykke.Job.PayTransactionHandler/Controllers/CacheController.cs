using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Job.PayTransactionHandler.Controllers
{
    [Route("api/[controller]")]
    public class CacheController : Controller
    {
        private readonly ICacheMaintainer<WalletState> _walletsStateCacheManager;
        private readonly ICacheMaintainer<TransactionState> _transactionStateCacheManager;
        private readonly ILog _log;

        public CacheController(
            [NotNull] ICacheMaintainer<WalletState> walletsStateCacheManager,
            [NotNull] ICacheMaintainer<TransactionState> transactionStateCacheManager,
            [NotNull] ILogFactory logFactory)
        {
            _walletsStateCacheManager = walletsStateCacheManager ??
                                        throw new ArgumentNullException(nameof(walletsStateCacheManager));
            _transactionStateCacheManager = transactionStateCacheManager ??
                                            throw new ArgumentNullException(nameof(transactionStateCacheManager));
            _log = logFactory.CreateLog(this);
        }

        [HttpGet]
        [Route("wallets")]
        [SwaggerOperation("GetWalletsCacheState")]
        [ProducesResponseType(typeof(IEnumerable<WalletState>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetWalletsCacheState()
        {
            try
            {
                return Ok(await _walletsStateCacheManager.GetAsync());
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        [Route("transactions")]
        [SwaggerOperation("GetTransactionsCacheState")]
        [ProducesResponseType(typeof(IEnumerable<TransactionState>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTransactionsCacheState()
        {
            try
            {
                return Ok(await _transactionStateCacheManager.GetAsync());
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
