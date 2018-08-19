using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lucent.Common
{
    public interface ILucentExtension
    {
        void Load(IServiceProvider provider, IConfiguration configuration);
    }
}