#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

// ReSharper disable InternalMembersMustHaveComments
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable HeapView.DelegateAllocation
namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Quartz;
    using global::Quartz.Impl;
    using JetBrains.Annotations;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    class ScopeTrackerTests
    {
        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<SampleJob>();
            cb.RegisterType<DisposableDependency>().InstancePerLifetimeScope();

            _container = cb.Build();

            _factory = new StdSchedulerFactory();
            _scheduler = _factory.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();
            _lifetimeScope = _container.Resolve<ILifetimeScope>();
            _jobFactory = new AutofacJobFactory(_lifetimeScope, QuartzAutofacFactoryModule.LifetimeScopeName);
            _scheduler.JobFactory = _jobFactory;
        }

        [TearDown]
        public void TearDown()
        {
            _scheduler.Shutdown(waitForJobsToComplete: false);
            _container.Dispose();
        }

        IContainer _container;
        StdSchedulerFactory _factory;
        AutofacJobFactory _jobFactory;
        ILifetimeScope _lifetimeScope;
        IScheduler _scheduler;

        [UsedImplicitly]
        [PersistJobDataAfterExecution]
        class SampleJob : IJob
        {
            // ReSharper disable once NotAccessedField.Local
            readonly DisposableDependency _dependency;

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public SampleJob([NotNull] DisposableDependency dependency)
            {
                if (dependency == null) throw new ArgumentNullException(nameof(dependency));
                _dependency = dependency;
            }

            public Task Execute(IJobExecutionContext context)
            {
                // ReSharper disable once UnusedVariable
                var data = context.JobDetail.JobDataMap;
                Debug.WriteLine("SampleJob started");
                return Task.FromResult(true);
            }
        }

        [UsedImplicitly]
        class DisposableDependency : IDisposable
        {
            public bool Disposed { get; private set; }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Debug.WriteLine("Disposing dependency 0x{0:x}", GetHashCode());
                Disposed = true;
            }
        }

        [Test]
        [Description("See #17")]
        public void ReturnJob_Should_DisposeJobIfMatchingScopeIsMissing()
        {
            var job = new Mock<IJob>();
            var disposableJob = job.As<IDisposable>();
            _jobFactory.ReturnJob(job.Object);

            disposableJob.Verify(d => d.Dispose(), Times.Once, "Job was not disposed");
        }


        [Test]
        [Description("See #17")]
        public void ReturnJob_Should_HandleMissingMatchingScope()
        {
            var job = new Mock<IJob>();
            Action returnJob = () => _jobFactory.ReturnJob(job.Object);
            returnJob.ShouldNotThrow("Failed to handle missing job.");
        }

        [Test]
        public void ShouldDisposeScopeAfterJobCompletion()
        {
            var key = new JobKey("disposable", "grp2");
            var job1 = JobBuilder.Create<SampleJob>().WithIdentity(key).StoreDurably(true)
                .Build();
            var trigger =
                TriggerBuilder.Create().WithSimpleSchedule(s => s.WithIntervalInSeconds(1).WithRepeatCount(1)).Build();

            var scopesCreated = 0;
            var scopesDisposed = 0;
            DisposableDependency dependency = null;

            _lifetimeScope.ChildLifetimeScopeBeginning += (sender, args) => {
                scopesCreated++;
                dependency = args.LifetimeScope.Resolve<DisposableDependency>();
                args.LifetimeScope.CurrentScopeEnding += (o, eventArgs) => { scopesDisposed++; };
            };

            _scheduler.ScheduleJob(job1, trigger);
            _scheduler.Start();

            Thread.Sleep(3.Seconds());

            _jobFactory.RunningJobs.Should().BeEmpty("Scope was not disposed after job completion");
            dependency.Disposed.Should().BeTrue("Dependency must be disposed");
            scopesDisposed.Should().Be(scopesCreated, "All scopes must be disposed");
        }
    }
}
