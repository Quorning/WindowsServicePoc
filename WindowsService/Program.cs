using System;
using System.IO;
using Topshelf;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace WindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{environment}.json", optional: true)
                            .AddEnvironmentVariables()
                            .Build();

            Log.Logger = CreateLogger(configuration);

            Log.Information("Program:Main()");

            var windoewService = CreateWindoewService();

            var exitCode = (int)Convert.ChangeType(windoewService, windoewService.GetTypeCode());  
            Environment.ExitCode = exitCode;
        }

        private static TopshelfExitCode CreateWindoewService()
        {
            //TODO: https://topshelf.readthedocs.io/en/latest/configuration/quickstart.html
            var rc = HostFactory.Run(x =>
            {
                x.Service<WindowsServiceControl>(s =>
                {
                    s.ConstructUsing(name => new WindowsServiceControl());
                    s.WhenStarted((s, hostControl) => s.Start(hostControl));
                    s.WhenStopped((s, hostControl) => s.Stop(hostControl));
                    //s.WhenPaused((s, hostControl) => s.Paused(hostControl));
                    //s.WhenContinued((s, hostControl) => s.Continued(hostControl));
                    s.WhenShutdown((s, hostControl) => s.Shutdown(hostControl));
                });

                x.SetDisplayName("LoggingServiceWithSeriLog");
                x.SetServiceName("LoggingServiceWithSeriLog");
                x.SetInstanceName("LoggingServiceWithSeriLog");
                x.SetDescription("LoggingServiceWithSeriLog");

                //DOC: Service Recovery, https://topshelf.readthedocs.io/en/latest/configuration/config_api.html#service-recovery
                //x.EnableServiceRecovery(r =>
                //{
                //    //you can have up to three of these
                //    r.RestartComputer(5, "message");
                //    r.RestartService(0);
                //    //the last one will act for all subsequent failures
                //    r.RunProgram(7, "ping google.com");

                //    //should this be true for crashed or non-zero exits
                //    r.OnCrashOnly();

                //    //number of days until the error count resets
                //    r.SetResetPeriod(1);
                //});

                //DOC: Service Identity, https://topshelf.readthedocs.io/en/latest/configuration/config_api.html#service-identity 
                x.RunAsLocalSystem();
                //x.RunAs("username", "password");

                //DOC: Service Start Modes, https://topshelf.readthedocs.io/en/latest/configuration/config_api.html#service-start-modes
                x.StartAutomatically();

                //DOC: Advanced Settings
                //https://topshelf.readthedocs.io/en/latest/configuration/config_api.html#advanced-settings
                x.OnException(ex =>
                {
                    // Do something with the exception
                });
                x.UseSerilog();
            });

            return rc;
        }

        private static Serilog.Core.Logger CreateLogger(IConfigurationRoot configuration)
        {
            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }
    }
}
