using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.RedisPersister
{
    public class RedisWrapper : IDisposable
    {
        private Task ConnectTask;
        private ConnectionMultiplexer RedisClient;
        private ISubscriber RedisSubscriber;
        private Dictionary<string, List<Action<RedisChannel, RedisValue>>> SubscribedActions = new Dictionary<string, List<Action<RedisChannel, RedisValue>>>();
        public RedisWrapper()
        {
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanged += SharedSettings_RedisServerChanged;
            ConnectTask = ConnectRedis();
        }
        private void ChangeRedisServer(Task existingConnectTask)
        {
            // We don't worry about the result of the task as any exceptions are already handled
            ResetRedis();
            ConnectTask = ConnectRedis();
        }
        private void SharedSettings_RedisServerChanged(object sender, EventArgs e)
        {
            ConnectTask.ContinueWith(ChangeRedisServer);
        }
        private async Task ConnectRedis()
        {
            var redisServer = TeamCodingPackage.Current.Settings.SharedSettings.RedisServer;
            if (!string.IsNullOrWhiteSpace(redisServer))
            {
                TeamCodingPackage.Current.Logger.WriteInformation($"Connecting to Redis using config string: \"{redisServer}\"");
                RedisClient = await ConnectionMultiplexer.ConnectAsync(redisServer).HandleException();
                TeamCodingPackage.Current.Logger.WriteInformation($"Connected to Redis using config string: \"{redisServer}\"");
                RedisSubscriber = RedisClient.GetSubscriber();

                IEnumerable<Task> tasks;
                lock (SubscribedActions)
                {
                    tasks = SubscribedActions.Keys.SelectMany(key => SubscribedActions[key].Select((a) => RedisSubscriber?.SubscribeAsync(key, a)?.HandleException()));
                }

                await Task.WhenAll(tasks);
            }
        }

        internal async Task Publish(string channel, byte[] data)
        {
            await ConnectTask;
            if (RedisSubscriber != null)
            {
                await RedisSubscriber?.PublishAsync(channel, data)?.HandleException();
            }
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
            if (RedisSubscriber != null)
            {
                await RedisSubscriber?.SubscribeAsync(channel, action)?.HandleException();
            }
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
