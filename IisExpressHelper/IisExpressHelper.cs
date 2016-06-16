using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Threading;

namespace Mjollnir.Testing.Helpers
{
    public static class IisExpressHelper
    {
        public class Settings
        {
            const string defaultClr = "v4.0";

            static readonly TimeSpan defaultHealthCheckTimeout = TimeSpan.FromMinutes(3);

            public Settings(string path, int port, string clr, Uri healthCheckUri, TimeSpan healthCheckTimeout)
            {
                if (path == null) throw new ArgumentNullException("path");
                if (port < 1) throw new ArgumentOutOfRangeException("port");
                if (clr == null) throw new ArgumentNullException("clr");
                if (healthCheckUri == null) throw new ArgumentNullException("healthCheckUri");

                this.Path = path;
                this.Port = port;
                this.Clr = clr;
                this.HealthCheckUri = healthCheckUri;
                this.HealthCheckTimeout = healthCheckTimeout;
            }

            public Settings(string path, int port, string clr, Uri healthCheckUri)
                : this(path, port, clr, healthCheckUri, defaultHealthCheckTimeout)
            {
                // nothing to do.
            }

            public Settings(string path, int port, Uri healthCheckUri, TimeSpan healthCheckTimeout)
                : this(path, port, defaultClr, healthCheckUri)
            {
                // nothing to do.
            }

            public Settings(string path, int port, Uri healthCheckUri)
                : this(path, port, defaultClr, healthCheckUri, defaultHealthCheckTimeout)
            {
                // nothing to do.
            }

            public string Path { get; private set; }

            public int? Port { get; private set; }

            public string Clr { get; private set; }

            public Uri HealthCheckUri { get; private set; }

            public TimeSpan? HealthCheckTimeout { get; private set; }
        }

        public static void Start(Settings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            var fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "IIS Express",
                "iisexpress.exe");

            var arguments = string.Join(" ", new[]
            {
                string.Format(@"/path:""{0}""", settings.Path),
                string.Format("/port:{0}", settings.Port),
                string.Format("/clr:{0}", settings.Clr),
            });

            using (var process = Process.Start(fileName, arguments))
            {
                var timeout = DateTime.Now + settings.HealthCheckTimeout;

                while (DateTime.Now < timeout)
                {
                    using (var client = new HttpClient())
                    {
                        Thread.Sleep(1000);

                        var response = client.GetAsync(settings.HealthCheckUri).ConfigureAwait(false).GetAwaiter().GetResult();

                        if ((((int)response.StatusCode) / 100) != 5) return;
                    }
                }

                throw new TimeoutException();
            }
        }

        public static void StopAll(Settings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            var path = string.Format(@"/path:""{0}""", settings.Path);
            var port = string.Format("/port:{0}", settings.Port);
            var clr = string.Format("/clr:{0}", settings.Clr);

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE Name='iisexpress.exe'"))
            using (var processes = searcher.Get())
            {
                foreach (var process in processes.OfType<ManagementObject>())
                {
                    using (process)
                    {
                        var commandLine = Convert.ToString(process["CommandLine"]);

                        if (commandLine.Contains(path) && commandLine.Contains(port) && commandLine.Contains(clr))
                        {
                            try
                            {
                                process.InvokeMethod("Terminate", new object[] { });
                            }
                            catch
                            {
                                // ignore.
                            }
                        }
                    }
                }
            }
        }
    }
}
