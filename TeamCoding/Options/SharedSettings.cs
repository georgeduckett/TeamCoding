using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class SharedSettings
    {
        public string FileBasedPersisterPath { get { return FileBasedPersisterPathProperty.Value; } set { FileBasedPersisterPathProperty.Value = value; } }
        public event EventHandler FileBasedPersisterPathChanged { add { FileBasedPersisterPathProperty.Changed += value; } remove { FileBasedPersisterPathProperty.Changed -= value; } }
        private readonly Property<string> FileBasedPersisterPathProperty;
        public const string DefaultFileBasedPersisterPath = null;
        public SharedSettings()
        {
            FileBasedPersisterPathProperty = new Property<string>(this);
        }
    }
}