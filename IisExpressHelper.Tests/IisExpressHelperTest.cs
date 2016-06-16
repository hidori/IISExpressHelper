using NUnit.Framework;
using System.IO;
using System.Net;

namespace Mjollnir.Testing.Helpers.Tests
{
    [TestFixture]
    public class IisExpressHelperTest
    {
        [Test]
        public void ConstructorTest()
        {
            var path = Global.Config.ExecutablePath;
            var apppath = Global.Config.AppPath;
            var port = Global.Config.Port;
            var clr = Global.Config.Clr;

            {
                var target = new IisExpressHelper(path, apppath, port);

                target.ExecutablePath.Is(path);
                target.AppPath.Is(apppath);
                target.Port.Is(port);
                target.Clr.IsNull();
            }

            {
                var target = new IisExpressHelper(path, apppath, port, clr);

                target.ExecutablePath.Is(path);
                target.AppPath.Is(apppath);
                target.Port.Is(port);
                target.Clr.Is(clr);
            }
        }

        [Test]
        public void DownloadTextFile1Test()
        {
            var port = Global.Config.Port;

            using (var client = new WebClient())
            {
                var text = client.DownloadString($"http://localhost:{port}/TextFile1.txt");
                text.Is("ABCZ");
            }
        }
    }
}
