using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Threading;

namespace Mjollnir.Testing.Helpers
{
    /// <summary>
    /// Helps iisexpress.exe automation.
    /// </summary>
    public class IisExpressHelper
    {
        /// <summary>
        /// Initializes a new instance of the IisExpressHelper class with the specified parametters.
        /// </summary>
        /// <param name="path">The path of a iisexpress.exe file.</param>
        /// <param name="apppath">The full physical path of the application to run.</param>
        /// <param name="port">The port to which the application will bind.</param>
        /// <param name="clr">The .NET Framework version (e.g. v2.0) to use to run the application.</param>
        public IisExpressHelper(string path, string apppath, int port, string clr)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (apppath == null) throw new ArgumentNullException(nameof(apppath));
            if (port < 1) throw new ArgumentOutOfRangeException(nameof(port));
            if (clr == null) throw new ArgumentNullException(nameof(clr));

            this.ExecutablePath = path;
            this.AppPath = apppath;
            this.Port = port;
            this.Clr = clr;
        }

        /// <summary>
        /// Initializes a new instance of the IisExpressHelper class with the specified parametters.
        /// </summary>
        /// <param name="path">The path of a iisexpress.exe file.</param>
        /// <param name="apppath">The full physical path of the application to run.</param>
        /// <param name="port">The port to which the application will bind.</param>
        public IisExpressHelper(string path, string apppath, int port)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (apppath == null) throw new ArgumentNullException(nameof(apppath));
            if (port < 1) throw new ArgumentOutOfRangeException(nameof(port));

            this.ExecutablePath = path;
            this.AppPath = apppath;
            this.Port = port;
            this.Clr = null;
        }

        /// <summary>
        /// Gets the path of a sqlcmd.exe file.
        /// </summary>
        public string ExecutablePath { get; private set; }

        /// <summary>
        /// Gets the full physical path of the application to run.
        /// </summary>
        public string AppPath { get; private set; }

        /// <summary>
        /// Gets the port to which the application will bind.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the .NET Framework version (e.g. v2.0) to use to run the application.
        /// </summary>
        public string Clr { get; private set; }

        /// <summary>
        /// Start the web application.
        /// </summary>
        public void Start()
        {
            var arguments = string.Join(" ", this.CreateArguments());

            using (var process = Process.Start(this.ExecutablePath, arguments))
            {
                var uri = new Uri($"http://localhost:{this.Port}/");
                var timeout = DateTime.Now + TimeSpan.FromMinutes(3);

                while (DateTime.Now < timeout)
                {
                    using (var client = new HttpClient())
                    {
                        Thread.Sleep(1000);

                        var response = client.GetAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();

                        if ((((int)response.StatusCode) / 100) != 5) return;
                    }
                }

                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Stop the web application.
        /// </summary>
        public void StopAll()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE Name='iisexpress.exe'"))
            using (var processes = searcher.Get())
            {
                foreach (var process in processes.OfType<ManagementObject>())
                {
                    using (process)
                    {
                        var commandLine = Convert.ToString(process["CommandLine"]);

                        if (commandLine.Contains(this.AppPath) && commandLine.Contains(this.Port.ToString()))
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

        protected virtual IEnumerable<string> CreateArguments()
        {
            yield return $"/path:\"{this.AppPath}\"";
            yield return $"/port:{this.Port}";

            if (!string.IsNullOrEmpty(this.Clr))
            {
                yield return $"/clr:{this.Clr}";
            }
        }
    }
}
