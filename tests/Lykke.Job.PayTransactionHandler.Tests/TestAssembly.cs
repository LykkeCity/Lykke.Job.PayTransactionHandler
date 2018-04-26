﻿using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lykke.Job.PayTransactionHandler.Tests
{
    [TestClass]
    public class TestAssembly
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            Mapper.AssertConfigurationIsValid();
        }
    }
}
