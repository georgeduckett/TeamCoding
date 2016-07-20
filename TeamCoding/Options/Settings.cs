using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class Settings
    {
        private OptionPageGrid OptionsPage => (OptionPageGrid)TeamCodingPackage.Current.GetDialogPage(typeof(OptionPageGrid));
        public string Username { get { return OptionsPage.Username; } }
        public string UserImageUrl { get { return OptionsPage.UserImageUrl; } }
    }
}