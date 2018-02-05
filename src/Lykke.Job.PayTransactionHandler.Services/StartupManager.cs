﻿using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly IWalletsStateCacheManager _walletsStateCacheWarmer;
        private readonly ILog _log;

        public StartupManager(IWalletsStateCacheManager walletsStateCacheWarmer, ILog log)
        {
            _walletsStateCacheWarmer = walletsStateCacheWarmer;
            _log = log;
        }

        public async Task StartAsync()
        {
            await _walletsStateCacheWarmer.Warmup();
        }
    }
}
