using System.Linq;
using InElonWeTrust.Core.Database;
using InElonWeTrust.Core.Database.Models;
using Oddity;

namespace InElonWeTrust.Core.Services.UserLaunchSubscriptions
{
    public class UserLaunchSubscriptionsService
    {
        private OddityCore _oddity;

        public UserLaunchSubscriptionsService()
        {
            _oddity = new OddityCore();
        }

        public void AddUserSubscription(ulong userId)
        {

        }

        public void RemoveUserSubscription(ulong userId)
        {

        }
    }
}
