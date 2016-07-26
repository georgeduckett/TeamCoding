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

        private Task ConnectTask;
        private ConnectionMultiplexer RedisClient;
        private ISubscriber RedisSubscriber;
        private Dictionary<string, List<Action<RedisChannel, RedisValue>>> SubscribedActions = new Dictionary<string, List<Action<RedisChannel, RedisValue>>>();
        public RedisWrapper()
        {
            Current = this;
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanged += SharedSettings_RedisServerChanged;
            ConnectTask = ConnectRedis();
        }

        private void SharedSettings_RedisServerChanged(object sender, EventArgs e)
        {
            ConnectTask.Wait();
            ResetRedis();
            ConnectTask = ConnectRedis();
        }

        private async Task ConnectRedis()
        {
            var redisServer = TeamCodingPackage.Current.Settings.SharedSettings.RedisServer;
            if (!string.IsNullOrWhiteSpace(redisServer))
            {
                RedisClient = await ConnectionMultiplexer.ConnectAsync(redisServer);
                RedisSubscriber = RedisClient.GetSubscriber();

                IEnumerable<Task> tasks;
                lock (SubscribedActions)
                {
                    tasks = SubscribedActions.Keys.SelectMany(key => SubscribedActions[key].Select((a) => RedisSubscriber.SubscribeAsync(key, a)));
                }

                await Task.WhenAll(tasks);
            }
        }

        internal async Task Publish(string channel, byte[] data)
        {
            await ConnectTask;
            await RedisSubscriber?.PublishAsync(channel, data);
        }
        internal async Task Subscribe(string channel, Action<RedisChannel, RedisValue> action)
        {
            lock (SubscribedActions)
            {
                if (!SubscribedActions.ContainsKey(channel))
                {
                    SubscribedActions.Add(channel, new List<Action<RedisChannel, RedisValue>>());
                }
                SubscribedActions[channel].Add(action);
            }

            await ConnectTask;
            await RedisSubscriber?.SubscribeAsync(channel, action);
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
