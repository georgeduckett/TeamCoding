using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class OptionPageGrid : DialogPage
    { // TODO: Make the option page grid a custom control/page so it's easier to understand
        private const string DefaultUsername = null;
        private const string DefaultImageUrl = null;
        private const string DefaultFileBasedPersisterPath = null;
        [Category("User Identity")]
        [DisplayName("Username")]
        [Description("Set a custom username, leave blank for the default.")]
        public string Username { get; set; } = DefaultUsername;
        [Category("User Identity")]
        [DisplayName("Image URL")]
        [Description("Set a custom image url. Invalid or blank urls are ignored.")]
        public string UserImageUrl { get; set; } = DefaultImageUrl;
        [Category("Network")]
        [DisplayName("IDE Sharing File Path")]
        [Description("Set a file path to enable sharing via reading/writing to a shared folder. Requires restart")]
        public string FileBasedPersisterPath { get; set; } = DefaultFileBasedPersisterPath; // TODO: perform basic sanity checking

        protected override void SaveSetting(PropertyDescriptor property)
        {
            base.SaveSetting(property);

            switch (property.Name)
            {
                case nameof(Username):
                case nameof(UserImageUrl):
                    TeamCodingPackage.Current.LocalIdeModel.OnUserIdentityChanged();
                    break;
            }
        }

        public override void ResetSettings()
        {
            base.ResetSettings();
            Username = DefaultUsername;
            UserImageUrl = DefaultImageUrl;
        }
    }
}