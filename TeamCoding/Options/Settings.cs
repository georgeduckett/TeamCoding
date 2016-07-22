using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class Settings
    {
        private class Property<T> where T : IEquatable<T>
        {
            private readonly Settings Owner;
            public Property(Settings owner) { Owner = owner; }
            private T _Value;
            public T Value
            {
                get { return _Value; }
                set
                {
                    if(!EqualityComparer<T>.Default.Equals(_Value, value))
                    {
                        _Value = value;
                        Changed?.Invoke(Owner, EventArgs.Empty);
                    }
                }
            }
            public event EventHandler Changed;
        }

        public string Username { get { return UsernameProperty.Value; } set { UsernameProperty.Value = value; } }
        public event EventHandler UsernameChanged { add { UsernameProperty.Changed += value; } remove { UsernameProperty.Changed -= value; } }
        private readonly Property<string> UsernameProperty;
        public const string DefaultUsername = null;
        public string UserImageUrl { get { return UserImageUrlProperty.Value; } set { UserImageUrlProperty.Value = value; } }
        public event EventHandler UserImageUrlChanged { add { UserImageUrlProperty.Changed += value; } remove { UserImageUrlProperty.Changed -= value; } }
        private readonly Property<string> UserImageUrlProperty;
        public const string DefaultImageUrl = null;
        public string FileBasedPersisterPath { get { return FileBasedPersisterPathProperty.Value; } set { FileBasedPersisterPathProperty.Value = value; } }
        public event EventHandler FileBasedPersisterPathChanged { add { FileBasedPersisterPathProperty.Changed += value; } remove { FileBasedPersisterPathProperty.Changed -= value; } }
        private readonly Property<string> FileBasedPersisterPathProperty;
        public const string DefaultFileBasedPersisterPath = null;

        internal void Update(OptionPageGrid optionPageGrid)
        {
            Username = optionPageGrid.Username;
            UserImageUrl = optionPageGrid.UserImageUrl;
            FileBasedPersisterPath = optionPageGrid.FileBasedPersisterPath;
        }
        public Settings()
        {
            UsernameProperty = new Property<string>(this);
            UserImageUrlProperty = new Property<string>(this);
            FileBasedPersisterPathProperty = new Property<string>(this);

            Update((OptionPageGrid)TeamCodingPackage.Current.GetDialogPage(typeof(OptionPageGrid)));
        }
    }
}