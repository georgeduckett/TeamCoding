using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{ // TODO: Figure out some way of getting (some) settings from a local file that could be synced from a repository. Maybe take them as defaults (and a way of resetting to default)
    public class OptionPageGrid : DialogPage
    { // TODO: Make the option page grid a custom control/page so it's easier to understand
        private const string DefaultUsername = null;
        private const string DefaultImageUrl = null;
        private const string DefaultFileBasedPersisterPath = null;

        [Category("User Identity")]
        [DisplayName("Username")]
        [Description("Set a custom username, leave blank for the default.")]
        public string Username { get; set; } = DefaultUsername;
        public event EventHandler UsernameChanged;
        public void OnUsernameChanged() => UsernameChanged?.Invoke(this, new EventArgs());

        [Category("User Identity")]
        [DisplayName("Image URL")]
        [Description("Set a custom image url. Invalid or blank urls are ignored.")]
        public string UserImageUrl { get; set; } = DefaultImageUrl;
        public event EventHandler UserImageUrlChanged;
        public void OnUserImageUrlChanged() => UserImageUrlChanged?.Invoke(this, new EventArgs());

        [Category("Network")]
        [DisplayName("IDE Sharing File Path")]
        [Description("Set a file path to enable sharing via reading/writing to a shared folder. Requires restart")]
        public string FileBasedPersisterPath { get; set; } = DefaultFileBasedPersisterPath; // TODO: perform basic sanity checking
        public event EventHandler FileBasedPersisterPathChanged;
        public void OnFileBasedPersisterPathChanged() => FileBasedPersisterPathChanged?.Invoke(this, new EventArgs());
        protected override void SaveSetting(PropertyDescriptor property)
        {
            base.SaveSetting(property);

            // Invoke the relement method that triggers the event
            // TODO: Cache the method per property name in OptionPageGrid
            var changedEvent = GetType().GetMethod($"On{property.Name}Changed");
            changedEvent.Invoke(this, null);
        }

        public override void ResetSettings()
        {
            base.ResetSettings();
            Username = DefaultUsername;
            UserImageUrl = DefaultImageUrl;
        }
    }
}