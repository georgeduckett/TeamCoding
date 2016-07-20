using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.IdentityManagement
{
    public class VSOptionsIdentityProvider : IIdentityProvider
    {
        public bool ShouldCache => false;
        public UserIdentity GetIdentity()
        {
            if (string.IsNullOrWhiteSpace(TeamCodingPackage.Current.Settings.Username)) return null;

            var ImageUrl = string.IsNullOrWhiteSpace(TeamCodingPackage.Current.Settings.UserImageUrl) ? null : new Uri(TeamCodingPackage.Current.Settings.UserImageUrl);
            if(ImageUrl != null &&(!ImageUrl.IsAbsoluteUri || ImageUrl.IsFile || ImageUrl.IsUnc))
            {
                ImageUrl = null;
            }
            return new UserIdentity()
            {
                Id = TeamCodingPackage.Current.Settings.Username,
                DisplayName = TeamCodingPackage.Current.Settings.Username,
                ImageUrl = ImageUrl?.ToString()
            };
        }
    }
}
