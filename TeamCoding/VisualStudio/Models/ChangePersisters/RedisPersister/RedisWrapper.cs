using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister
{
    public class RedisWrapper : IDisposable
    {
        public static RedisWrapper Current;

        private ConnectionMultiplexer RedisClient; // TODO: allow for failing to connect to redis (and connect asyncronously)
        private ISubscriber RedisSubscriber;
        public RedisWrapper()
        {
            Current = this;
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanged += SharedSettings_RedisServerChanged;
            ConnectRedis();
        }

        private void SharedSettings_RedisServerChanged(object sender, EventArgs e)
        {
            Dispose();
            ConnectRedis();
        }

        private void ConnectRedis()
        {
            if (!string.IsNullOrWhiteSpace(TeamCodingPackage.Current.Settings.SharedSettings.RedisServer))
            {
                RedisClient = ConnectionMultiplexer.Connect(TeamCodingPackage.Current.Settings.SharedSettings.RedisServer);
                RedisSubscriber = RedisClient.GetSubscriber();
            }
        }

        internal void Publish(string channel, byte[] data)
        {
            RedisSubscriber?.Publish(channel, data);
        }
        internal void Subscribe(string channel, Action<RedisChannel, RedisValue> action)
        {
            RedisSubscriber?.Subscribe(channel, action);
        }
        private void ResetRedis()
        {
            RedisClient?.Dispose();
            RedisClient = null;
            RedisSubscriber = null;
        }
        public void Dispose()
        {
            ResetRedis();
        }
    }
}
