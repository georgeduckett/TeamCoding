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
        public event EventHandler UsernameChanged
        {
            add { OptionsPage.UsernameChanged += value; }
            remove { OptionsPage.UsernameChanged -= value; }
        }
        public string UserImageUrl { get { return OptionsPage.UserImageUrl; } }
        public event EventHandler UserImageUrlChanged
        {
            add { OptionsPage.UserImageUrlChanged += value; }
            remove { OptionsPage.UserImageUrlChanged -= value; }
        }
        public string FileBasedPersisterPath { get { return OptionsPage.FileBasedPersisterPath; } }
        public event EventHandler FileBasedPersisterPathChanged
        {
            add { OptionsPage.FileBasedPersisterPathChanged += value; }
            remove { OptionsPage.FileBasedPersisterPathChanged -= value; }
        }
    }
}