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
{ // Test out some way of getting (some) settings from a local file that could be synced from a repository.
    [Guid(Guids.OptionPageGridGuidString)]
    public class OptionPageGrid : UIElementDialogPage
    {
        public const string OptionsName = "Team Coding";
        public string Username { get; set; } = UserSettings.DefaultUsername;
        public string UserImageUrl { get; set; } = UserSettings.DefaultImageUrl;
        public string FileBasedPersisterPath { get; set; } = SharedSettings.DefaultFileBasedPersisterPath;
        public string RedisServer { get; set; } = SharedSettings.DefaultRedisServer;
        public string SlackToken { get; set; } = SharedSettings.DefaultSlackToken;
        public string SlackChannel { get; set; } = SharedSettings.DefaultSlackChannel;
        public string SqlServerConnectionString { get; set; } = SharedSettings.DefaultSqlServerConnectionString;
        private OptionsPage OptionsPage;
        protected override UIElement Child { get { return OptionsPage ?? (OptionsPage = new OptionsPage(this)); } }
        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            TeamCodingPackage.Current.Settings.Update(this);
        }
    }
}