using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreConsoleApp
{
    using System.Collections.Specialized;
    using System.Reflection;
    using Autofac;
    using Autofac.Extras.Quartz;
    using Quartz;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using SimpleService.AppServices;
    using SimpleService.Jobs;

    public class Program
    {
        static IContainer _container;

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole(LogEventLevel.Verbose)
                .CreateLogger();
            
            Log.Logger.Information("Running application, press Enter to stop.");

            _container = ConfigureContainer(new ContainerBuilder()).Build();

            ConfigureScheduler(_container.Resolve<IScheduler>()).ConfigureAwait(true).GetAwaiter().GetResult();

            var service = _container.Resolve<ServiceCore>();
            service.Start();


            Console.ReadLine();

            Log.Logger.Information("Done");
            _container?.Dispose();
            Log.CloseAndFlush();
        }

        static Task ConfigureScheduler(IScheduler s)
        {
            var job = JobBuilder.Create<HeartbeatJob>()
                .WithIdentity("Heartbeat", "Maintenance")
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(2)).Build();
            return  s.ScheduleJob(job, trigger);
        }


        static ContainerBuilder ConfigureContainer(ContainerBuilder cb)
        {
            // configure and register Quartz
            var schedulerConfig = new NameValueCollection {
                {"quartz.threadPool.threadCount", "3"},
                //{"quartz.threadPool.threadNamePrefix", "SchedulerWorker"},
                {"quartz.scheduler.threadName", "Scheduler"}
            };

            cb.RegisterModule(new QuartzAutofacFactoryModule
            {
                ConfigurationProvider = c => schedulerConfig
            });
            cb.RegisterModule(new QuartzAutofacJobsModule(typeof(HeartbeatJob).GetTypeInfo().Assembly));

            RegisterComponents(cb);
            return cb;
        }

        internal static void RegisterComponents(ContainerBuilder cb)
        {
            // register Service instance
            cb.RegisterType<ServiceCore>().AsSelf();
            // register dependencies
            cb.RegisterType<HeartbeatService>().As<IHeartbeatService>();
        }
    }
}
