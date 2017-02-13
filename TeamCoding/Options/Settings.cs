using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class Settings
    {
        public const string TeamCodingConfigFileName = "teamcoding.json";
        public readonly UserSettings UserSettings = new UserSettings();
        public readonly SharedSettings SharedSettings = new SharedSettings();
        public readonly Dictionary<object, PropertyInfo[]> SettingsProperties;
        public readonly static PropertyInfo[] OptionPageGridProperties = typeof(OptionPageGrid).GetProperties();
        public Settings()
        {
            SettingsProperties = new Dictionary<object, PropertyInfo[]>()
            {
                [UserSettings] = typeof(UserSettings).GetProperties(),
                [SharedSettings] = typeof(SharedSettings).GetProperties()
            };

            UpdateFromGrid((OptionPageGrid)TeamCodingPackage.Current.GetDialogPage(typeof(OptionPageGrid)));
        }
        internal bool LoadFromJsonFile(string solutionFile = null)
        {
            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(EnvDTE.DTE));

            solutionFile = solutionFile ?? dte.Solution?.FileName;

            if (string.IsNullOrWhiteSpace(solutionFile))
            {
                return false;
            }
            var SolutionPath = Path.GetDirectoryName(solutionFile);

            TeamCodingPackage.Current.Logger.WriteInformation($"Looking for {TeamCodingConfigFileName} in {SolutionPath}");

            var teamConfigFilePath = Directory.GetFiles(SolutionPath, TeamCodingConfigFileName, SearchOption.AllDirectories);

            if (teamConfigFilePath.Length == 0)
            {
                TeamCodingPackage.Current.Logger.WriteInformation($"No {TeamCodingConfigFileName} file found");
                return false;
            }
            else if (teamConfigFilePath.Length != 1)
            {
                TeamCodingPackage.Current.Logger.WriteError($"Multiple {TeamCodingConfigFileName} files found");
                return false;
            }

            TeamCodingPackage.Current.Logger.WriteInformation($"{TeamCodingConfigFileName} file found");
            var teamConfig = JObject.Parse(File.ReadAllText(teamConfigFilePath[0]));
            TeamCodingPackage.Current.Logger.WriteInformation($"{TeamCodingConfigFileName} file parsed as json successfully");

            foreach (var prop in SettingsProperties[SharedSettings])
            {
                var configPropValue = teamConfig[prop.Name];
                if(configPropValue == null)
                {
                    continue;
                }

                try
                {
                    TeamCodingPackage.Current.Logger.WriteInformation($"Setting {prop.Name} to {configPropValue.ToObject(prop.PropertyType)}");
                    prop.SetValue(SharedSettings, configPropValue.ToObject(prop.PropertyType));
                }
                catch(Exception ex)
                {
                    TeamCodingPackage.Current.Logger.WriteError($"{prop.Name} cannot be set to {configPropValue.ToObject(prop.PropertyType)}");
                    TeamCodingPackage.Current.Logger.WriteError(ex);
                }
            }

            return true;
        }
        internal void UpdateFromGrid(OptionPageGrid optionPageGrid)
        {
            foreach (var key in SettingsProperties.Keys)
            {
                foreach (var prop in SettingsProperties[key])
                {
                    var optionProp = OptionPageGridProperties.SingleOrDefault(p => p.Name == prop.Name);

                    if (optionProp != null)
                    {
                        prop.SetValue(key, optionProp.GetValue(optionPageGrid));
                    }
                }
            }
        }
        internal void UpdateToGrid(OptionPageGrid optionPageGrid)
        {
            foreach (var key in SettingsProperties.Keys)
            {
                foreach (var prop in SettingsProperties[key])
                {
                    var optionProp = OptionPageGridProperties.SingleOrDefault(p => p.Name == prop.Name);

                    if (optionProp != null)
                    {
                        optionProp.SetValue(optionPageGrid, prop.GetValue(key));
                    }
                }
            }
        }
    }
}