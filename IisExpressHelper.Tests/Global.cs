using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mjollnir.Testing.Helpers.Tests
{
    [SetUpFixture]
    public class Global
    {
        public static class Config
        {
            public static readonly string TestDirectoryPath = Path.GetDirectoryName(typeof(Global).Assembly.Location);
            public static readonly string ProjectDirectoryPath = Path.GetFullPath(Path.Combine(TestDirectoryPath, @"..\.."));
            public static readonly string SolutionDirectoryPath = Path.GetFullPath(Path.Combine(ProjectDirectoryPath, @".."));

            public const string ExecutablePath = @"C:\Program Files\IIS Express\iisexpress.EXE";
            public static readonly string AppPath = Path.GetFullPath(Path.Combine(SolutionDirectoryPath, "E2ETestTarget"));
            public const int Port = 20002;
            public const string Clr = "v4.0";
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            var path = Config.ExecutablePath;
            var apppath = Config.AppPath;
            var port = Config.Port;

            var helper = new IisExpressHelper(path, apppath, port);

            helper.StopAll();
            helper.Start();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            var path = Config.ExecutablePath;
            var apppath = Config.AppPath;
            var port = Config.Port;

            var helper = new IisExpressHelper(path, apppath, port);

            helper.StopAll();
        }

    }
}
