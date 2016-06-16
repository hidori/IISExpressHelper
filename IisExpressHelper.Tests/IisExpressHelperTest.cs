using System;
using System.IO;
using System.Net;
using Mjollnir.Testing;
using NUnit.Framework;

namespace Mjollnir.Testing.Helpers.Tests
{
    [TestFixture]
    public class IisExpressHelperTest
    {
        static readonly IisExpressHelper.Settings iisExpressSettings = new IisExpressHelper.Settings(
            path: Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(IisExpressHelperTest).Assembly.Location), @"..\..\..\IisExpressHelper.E2E.Target")),
            port: 20002,
            healthCheckUri: new Uri("http://localhost:20002"));

        [TestFixtureSetUp]
        public void Initialize()
        {
            IisExpressHelper.StopAll(iisExpressSettings);
            IisExpressHelper.Start(iisExpressSettings);
        }

        [TestFixtureTearDown]
        public void Terminate()
        {
            IisExpressHelper.StopAll(iisExpressSettings);
        }

        [Test]
        public void DownloadTextFile1Test()
        {
            using (var client = new WebClient())
            {
                var text = client.DownloadString("http://localhost:20002/TextFile1.txt");
                text.Is("ABCZ");
            }
        }
    }
}
