using System.IO;
using Lucent.Common.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Test
{
    public abstract class BaseTestClass
    {
        public TestContext TestContext { get; set; }
        protected const string SERVICE_PROVIDER_KEY = "service.provider";

        protected ServiceProvider ServiceProvider { get => (ServiceProvider)TestContext.Properties[SERVICE_PROVIDER_KEY]; }
        protected IConfiguration Configuration { get; private set; }

        public BaseTestClass()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = configBuilder.Build();
        }

        public virtual void TestInitialize()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ILoggerFactory>(new LoggerFactory().AddConsole());
            services.AddLogging();
            services.AddSerialization(null);

            InitializeDI(services);

            var serviceProvider = services.BuildServiceProvider();
            TestContext.Properties.Add(SERVICE_PROVIDER_KEY, serviceProvider);
        }

        protected abstract void InitializeDI(IServiceCollection services);

        [TestCleanup]
        public void TestCleanup()
        {

        }
    }
}