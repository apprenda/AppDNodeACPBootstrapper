using System;
using Apprenda.API.Extension.Bootstrapping;
using Apprenda.API.Extension.CustomProperties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppDNodeBootstrapper;

namespace BootstrapTest
{
    [TestClass]
    public class BootstrapTest
    {
        [TestMethod]
        public void BootstrapperInit()
        {
            var bootstrapper = new Bootstrapper();
            var properties = new[]
            {
                new CustomProperty { Name = "APMEnable", Values = new[] { "Yes" } },
                new CustomProperty { Name = "AppdAppTier", Values = new[] { "Pod" } }
            };
            var result = bootstrapper.Bootstrap(new BootstrappingRequest(
                BootstrappingType.PerComponent,
                @"d:\de\specs",
                ComponentType.Pod,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "kubernetes",
                "acpsec",
                "v1",
                properties));
        }
    }
}
