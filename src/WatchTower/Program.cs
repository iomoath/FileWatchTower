using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace WatchTower
{
    internal class Program
    {
        private IMonitoringService _monitoringService;
        private PostProcessor _postProcessor;

        public Program()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.MaxServicePointIdleTime = 20000;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        }

        private async Task<int> RunProgram()
        {
            var initialized = Init();

            if (!initialized || _monitoringService == null)
                return 0;

            _postProcessor.Start();
            _monitoringService.Start();
            await _monitoringService.WaitAsync();
            return 0;
        }

        private static async Task<int> Main(string[] args)
        {
            // Parse command line here.. args
            // Install/uninstall/update config..etc

            var c = new Program();
            return await c.RunProgram();
        }

        private bool Init()
        {
            Helpers.CreateEventSource();

            var configPath = @"config-test.xml";
            var xmlConfig = File.ReadAllText(configPath);

            IConfigParser<Configuration> parser = ConfigParserFactory.GetParser<Configuration>(xmlConfig);
            IConfiguration configuration = parser.Parse(xmlConfig);

            if (configuration == null)
            {
                // Print usage help
                return false;
            }

            LogHelper.InitLogger();


            _monitoringService = new SysmonEventWatcher();
            _postProcessor = new PostProcessor(_monitoringService, configuration);

            return true;
        }



        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (args?.ExceptionObject == null)
                return;

            try
            {
                var ex = (Exception)args.ExceptionObject;
                var msg = $"OnUnhandledException(): Terminating: {args.IsTerminating} | {ex.Message}";
                Log.Error(ex, msg);
            }
            catch (Exception e)
            {
                var msg = $"OnUnhandledException().Exception: {e.Message}";
                Log.Error(e, msg);
            }
        }

    }
}
