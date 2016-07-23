using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class Settings
    {
        public UserSettings UserSettings = new UserSettings();
        public SharedSettings SharedSettings = new SharedSettings();
        internal void Update(OptionPageGrid optionPageGrid)
        {
            UserSettings.Username = optionPageGrid.Username;
            UserSettings.UserImageUrl = optionPageGrid.UserImageUrl;
            SharedSettings.FileBasedPersisterPath = optionPageGrid.FileBasedPersisterPath;
        }
        public Settings()
        {
            Update((OptionPageGrid)TeamCodingPackage.Current.GetDialogPage(typeof(OptionPageGrid)));
        }
    }
}