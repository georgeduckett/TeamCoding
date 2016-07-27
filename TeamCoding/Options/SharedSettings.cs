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
        public event EventHandler FileBasedPersisterPathChanging { add { FileBasedPersisterPathProperty.Changing += value; } remove { FileBasedPersisterPathProperty.Changing -= value; } }
        private readonly Property<string> FileBasedPersisterPathProperty;
        public const string DefaultFileBasedPersisterPath = null;
        public string RedisServer { get { return RedisServerProperty.Value; } set { RedisServerProperty.Value = value; } }
        public event EventHandler RedisServerChanged { add { RedisServerProperty.Changed += value; } remove { RedisServerProperty.Changed -= value; } }
        public event EventHandler RedisServerChanging { add { RedisServerProperty.Changing += value; } remove { RedisServerProperty.Changing -= value; } }
        private readonly Property<string> RedisServerProperty;
        public const string DefaultRedisServer = null;
        public SharedSettings()
        {
            FileBasedPersisterPathProperty = new Property<string>(this);
            FileBasedPersisterPathProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(FileBasedPersisterPath)}: {FileBasedPersisterPath}");

            RedisServerProperty = new Property<string>(this);
            RedisServerProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(RedisServer)}: {RedisServer}");
        }
    }
}