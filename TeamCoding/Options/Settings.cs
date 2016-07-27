using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class Settings
    {
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

            Update((OptionPageGrid)TeamCodingPackage.Current.GetDialogPage(typeof(OptionPageGrid)));
        }
        internal void Update(OptionPageGrid optionPageGrid)
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
    }
}