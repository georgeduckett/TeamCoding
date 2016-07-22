using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TeamCoding.Options
{ // TODO: Figure out some way of getting (some) settings from a local file that could be synced from a repository. Maybe take them as defaults (and a way of resetting to default)
    [Guid("BEC8E8F5-B7B8-422E-A586-12E7AA7E8DF8")]
    public class OptionPageGrid : UIElementDialogPage
    {
        private const string DefaultUsername = null;
        private const string DefaultImageUrl = null;
        private const string DefaultFileBasedPersisterPath = null;
        public string Username { get; set; } = DefaultUsername;
        public string UserImageUrl { get; set; } = DefaultImageUrl;
        public string FileBasedPersisterPath { get; set; } = DefaultFileBasedPersisterPath;
        private OptionsPage OptionsPage;
        protected override UIElement Child { get { return OptionsPage ?? (OptionsPage = new OptionsPage(this)); } }
        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            TeamCodingPackage.Current.Settings.Update(this);
        }
        public override void ResetSettings()
        {
            base.ResetSettings();

            Username = DefaultUsername;
            UserImageUrl = DefaultImageUrl;
            FileBasedPersisterPath = DefaultFileBasedPersisterPath;
        }
    }
}