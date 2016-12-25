#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

namespace SimpleService.AppServices
{
    using System.Threading.Tasks;
    using Common.Logging;
    using JetBrains.Annotations;

    [UsedImplicitly]
    class HeartbeatService : IHeartbeatService
    {
        static readonly ILog s_log = LogManager.GetLogger<HeartbeatService>();

        public Task UpdateServiceState(string state)
        {
            s_log.InfoFormat("Service state: {0}.", state);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    ///     Injected service.
    /// </summary>
    public interface IHeartbeatService
    {
        /// <summary>
        /// Set service status.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Task.</returns>
        Task UpdateServiceState(string state);
    }
}
