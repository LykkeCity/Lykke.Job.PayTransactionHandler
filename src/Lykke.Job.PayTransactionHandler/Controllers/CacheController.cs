using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
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
        private readonly ITransactionStateCacheManager<WalletState, PaymentBcnTransaction> _walletsStateCacheManager;
        private readonly ITransactionStateCacheManager<TransactionState, BcnTransaction> _transactionStateCacheManager;
        private readonly ILog _log;

        public CacheController(
            ITransactionStateCacheManager<WalletState, PaymentBcnTransaction> walletsStateCacheManager,
            ITransactionStateCacheManager<TransactionState, BcnTransaction> transactionStateCacheManager,
            ILog log)
        {
            _walletsStateCacheManager = walletsStateCacheManager ??
                                        throw new ArgumentNullException(nameof(walletsStateCacheManager));
            _transactionStateCacheManager = transactionStateCacheManager ??
                                            throw new ArgumentNullException(nameof(transactionStateCacheManager));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        [HttpGet]
        [SwaggerOperation("GetWalletsCacheState")]
        [ProducesResponseType(typeof(IEnumerable<WalletState>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetWalletsCacheState()
        {
            try
            {
                return Ok(await _walletsStateCacheManager.GetState());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(CacheController), nameof(GetWalletsCacheState), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        [SwaggerOperation("GetTransactionsCacheState")]
        [ProducesResponseType(typeof(IEnumerable<TransactionState>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTransactionsCacheState()
        {
            try
            {
                return Ok(await _transactionStateCacheManager.GetState());
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(CacheController), nameof(GetTransactionsCacheState), ex);
            }

            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
    }
}
