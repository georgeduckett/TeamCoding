using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class UserSettings
    {
        public string Username { get { return UsernameProperty.Value; } set { UsernameProperty.Value = value; } }
        public event EventHandler UsernameChanged { add { UsernameProperty.Changed += value; } remove { UsernameProperty.Changed -= value; } }
        private readonly Property<string> UsernameProperty;
        public const string DefaultUsername = null;
        public string UserImageUrl { get { return UserImageUrlProperty.Value; } set { UserImageUrlProperty.Value = value; } }
        public event EventHandler UserImageUrlChanged { add { UserImageUrlProperty.Changed += value; } remove { UserImageUrlProperty.Changed -= value; } }
        private readonly Property<string> UserImageUrlProperty;
        public const string DefaultImageUrl = null;
        public UserSettings()
        {
            UsernameProperty = new Property<string>(this);
            UserImageUrlProperty = new Property<string>(this);
        }
    }
}
