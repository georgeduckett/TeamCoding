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
            // TODO: Cache properties for updating settings
            foreach(var prop in UserSettings.GetType().GetProperties())
            {
                var optionProp = typeof(OptionPageGrid).GetProperty(prop.Name);

                if(optionProp != null)
                {
                    prop.SetValue(UserSettings, optionProp.GetValue(optionPageGrid));
                }
            }
            foreach (var prop in SharedSettings.GetType().GetProperties())
            {
                var optionProp = typeof(OptionPageGrid).GetProperty(prop.Name);

                if (optionProp != null)
                {
                    prop.SetValue(SharedSettings, optionProp.GetValue(optionPageGrid));
                }
            }
        }
        public Settings()
        {
            Update((OptionPageGrid)TeamCodingPackage.Current.GetDialogPage(typeof(OptionPageGrid)));
        }
    }
}