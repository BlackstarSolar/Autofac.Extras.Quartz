#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz.Tests
{
    using System;
    using System.Collections.Specialized;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Quartz;
    using global::Quartz.Impl;
    using global::Quartz.Spi;
    using JetBrains.Annotations;
    using NUnit.Framework;

    [TestFixture]
    class QuartzAutofacFactoryModuleTests
    {
        [SetUp]
        public void SetUp()
        {
            var cb = new ContainerBuilder();
            _quartzAutofacFactoryModule = new QuartzAutofacFactoryModule();
            cb.RegisterModule(_quartzAutofacFactoryModule);

            _container = cb.Build();
        }

        [TearDown]
        public void TearDown()
        {
            _container?.Dispose();
        }

        IContainer _container;
        QuartzAutofacFactoryModule _quartzAutofacFactoryModule;

        [UsedImplicitly]
        class TestJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                return Task.FromResult(true);
            }
        }

        [Test]
        public void CanUseGenericAutofacModuleRegistrationSyntax()
        {
            var cb = new ContainerBuilder();
            cb.RegisterModule<QuartzAutofacFactoryModule>();
            cb.Build();
        }

        [Test]
        public void ShouldExecuteConfigureSchedulerFactoryFunctionIfSet()
        {
            var configuration = new NameValueCollection();
            var customSchedulerName = Guid.NewGuid().ToString();
            configuration[StdSchedulerFactory.PropertySchedulerInstanceName] = customSchedulerName;

            _quartzAutofacFactoryModule.ConfigurationProvider = context => configuration;

            var scheduler = _container.Resolve<IScheduler>();
            scheduler.SchedulerName.Should().BeEquivalentTo(customSchedulerName);
        }

        [Test]
        public void ShouldRegisterAutofacJobFactory()
        {
            _container.Resolve<AutofacJobFactory>().Should().NotBeNull();
            _container.Resolve<IJobFactory>().Should().BeOfType<AutofacJobFactory>();
            _container.Resolve<IJobFactory>().Should().BeSameAs(_container.Resolve<AutofacJobFactory>(),
                "should be singleton");
        }

        [Test]
        public void ShouldRegisterAutofacSchedulerFactory()
        {
            var factory = _container.Resolve<ISchedulerFactory>();
            factory.Should().BeOfType<AutofacSchedulerFactory>();
        }

        [Test]
        public void ShouldRegisterFactoryAsSingleton()
        {
            var factory = _container.Resolve<ISchedulerFactory>();
            _container.Resolve<ISchedulerFactory>().Should().BeSameAs(factory);
        }

        [Test]
        public void ShouldRegisterSchedulerAsSingleton()
        {
            var scheduler = _container.Resolve<IScheduler>();
            _container.Resolve<IScheduler>().Should().BeSameAs(scheduler);
        }
    }
}
