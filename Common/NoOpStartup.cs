using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Lucent.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class NoOpStartup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args);
    }
}