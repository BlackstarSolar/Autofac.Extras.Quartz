#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

// ReSharper disable once CheckNamespace
namespace SimpleService.AppServices
{
    using System;
    using JetBrains.Annotations;
    using Quartz;
    using Quartz.Logging;

    /// <summary>
    /// Host class for windows service.
    /// </summary>
    [UsedImplicitly]
    public class ServiceCore
    {
        static readonly ILog s_log = LogProvider.GetLogger(typeof(ServiceCore));
        readonly IScheduler _scheduler;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scheduler"></param>
        public ServiceCore(IScheduler scheduler)
        {
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            _scheduler = scheduler;
        }

        /// <summary>
        /// Starts scheduler.
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            s_log.Info("Service started");

            if (!_scheduler.IsStarted)
            {
                s_log.Info("Starting Scheduler");
                _scheduler.Start().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Stops scheduler.
        /// </summary>
        public void Stop()
        {
            s_log.Info("Stopping Scheduler...");
            _scheduler.Shutdown(true);

            s_log.Info("Service stopped");
        }
    }
}
