#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

// ReSharper disable MissingXmlDoc
namespace SimpleService.Jobs
{
    using System;
    using System.Threading.Tasks;
    using AppServices;
    using Quartz;
    using Quartz.Logging;

    /// <summary>
    /// Sample job.
    /// </summary>
    public class HeartbeatJob : IJob
    {
        static readonly ILog s_log = LogProvider.GetLogger(typeof(HeartbeatJob));
        readonly IHeartbeatService _hearbeat;

        public HeartbeatJob(IHeartbeatService hearbeat)
        {
            if (hearbeat == null) throw new ArgumentNullException(nameof(hearbeat));
            _hearbeat = hearbeat;
        }

        public Task Execute(IJobExecutionContext context)
        {
            s_log.Trace("Execute task");
            return _hearbeat.UpdateServiceState("alive");
        }
    }
}
