using System.Threading.Tasks;
using Lucent.Common.OpenRTB;

namespace Lucent.Common.UserManagement
{
    /// <summary>
    /// Manages users and tracking
    /// </summary>
    public interface IUserManager
    {
        /// <summary>
        /// Map the user (if possible) from the bid request
        /// </summary>
        /// <param name="bidRequest">The request</param>
        /// <returns>A set of user features</returns>
        Task<UserFeatures> GetFeaturesAsync(BidRequest bidRequest);
    }
}