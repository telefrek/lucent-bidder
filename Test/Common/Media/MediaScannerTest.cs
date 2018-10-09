using System.IO;
using Lucent.Common.Entities;
using Lucent.Common.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lucent.Common.Media.Test
{
    [TestClass]
    public class MediaScannerTests : BaseTestClass
    {
        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
        }

        protected override void InitializeDI(IServiceCollection services)
        {
            services.AddMediaScanner(Configuration);
        }

        [TestMethod]
        public void TestVideo()
        {
            var scanner = ServiceProvider.GetRequiredService<IMediaScanner>();
            var content = scanner.Scan(Path.Combine(Directory.GetCurrentDirectory(), "resources/test-movie.mp4"), "video/mp4");
            Assert.IsNotNull(content, "Content should not be null");
            Assert.AreEqual(1080, content.H);
            Assert.AreEqual(1920, content.W);
            Assert.AreEqual(3451, content.BitRate);
            Assert.IsTrue(content.Codec.StartsWith("h264"));
            Assert.AreEqual(ContentType.Video, content.ContentType);
        }

        [TestMethod]
        public void TestImage()
        {
            var scanner = ServiceProvider.GetRequiredService<IMediaScanner>();
            var content = scanner.Scan(Path.Combine(Directory.GetCurrentDirectory(), "resources/lucent-blue.png"), "image/png");
            Assert.IsNotNull(content, "Content should not be null");
            Assert.AreEqual(311, content.H);
            Assert.AreEqual(615, content.W);
            Assert.AreEqual(ContentType.Banner, content.ContentType);
        }
    }
}