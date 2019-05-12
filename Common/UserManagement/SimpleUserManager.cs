using System.Threading.Tasks;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.UserManagement
{
    /// <summary>
    /// Simple implementation that does nothing for now
    /// </summary>
    public class SimpleUserManager : IUserManager
    {
        /// <inheritdoc/>
        public Task<UserFeatures> GetFeaturesAsync(BidRequest bidRequest) => Task.FromResult(UserFeatures.NONE);
    }
}