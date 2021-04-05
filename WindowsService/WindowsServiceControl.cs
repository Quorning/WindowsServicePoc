using System;
using Topshelf;
using ApplicationContainer;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Logging;
using System.Threading;

namespace WindowsService
{
    public class WindowsServiceControl : ServiceControl
    {
        private static Serilog.ILogger Log => Serilog.Log.ForContext<WindowsServiceControl>();
        private IServiceProvider _serviceProvider;
        private Timer _workTimer;    

        public bool Start(HostControl hostControl)
        {
            Log.Here().Information("Start WindoewService");

            try
            {
                //TODO: ikke den rigtige løsning
                _workTimer = new Timer(new TimerCallback(DoWork), null, 5000, 5000);
            } 
            catch(Exception ex)
            {
                Log.Here().Error(ex, "fejl");
            }

            return true;
        }

        private void DoWork(object state)
        {
            RegisterServices();

            IServiceScope scope = _serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<Worker>().Run();

            DisposeServices();
        }

        public bool Stop(HostControl hostControl)
        {
            Log.Here().Information("Stop WindoewService");
            _workTimer.Dispose();

            return true;
        }

        //public bool Paused(HostControl hostControl)
        //{
        //    Log.Here().Information("TestLogningInDomain LogDebug text.");
        //    return true;
        //}

        //public bool Continued(HostControl hostControl)
        //{
        //    Log.Here().Information("TestLogningInDomain LogDebug text.");
        //    return true;
        //}

        public bool Shutdown(HostControl hostControl)
        {
            Log.Here().Information("Shutdown WindoewService");

            _workTimer.Dispose();
            return true;
        }

        private void RegisterServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<Worker>();

            // add necessary services
            services.AddInfrastructure();
            services.AddDomaine();
            services.AddServiceProxy();

            _serviceProvider = services.BuildServiceProvider();
        }

        private void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
