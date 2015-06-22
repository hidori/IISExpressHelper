using System;
using System.IO;
using System.Net;
using Mjollnir.Testing;
using NUnit.Framework;

namespace IisExpressHelper.Tests
{
    [TestFixture]
    public class IisExpressHelperTest
    {
        static readonly IisExpress.Settings iisExpressSettings = new IisExpress.Settings(
            path: Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(IisExpressHelperTest).Assembly.Location), @"..\..\..\IisExpressHelper.E2E.Tests")),
            port: 33115,
            healthCheckUri: new Uri("http://localhost:33115"));

        [TestFixtureSetUp]
        public void Initialize()
        {
            IisExpress.StopAll(iisExpressSettings);
            IisExpress.Start(iisExpressSettings);
        }

        [TestFixtureTearDown]
        public void Terminate()
        {
            IisExpress.StopAll(iisExpressSettings);
        }

        [Test]
        public void DownloadTextFile1Test()
        {
            using (var client = new WebClient())
            {
                var text = client.DownloadString("http://localhost:33115/TextFile1.txt");
                text.Is("ABCZ");
            }
        }
    }
}
