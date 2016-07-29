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
        private Dictionary<string, List<Action<RedisChannel, RedisValue>>> SubscribedActions = new Dictionary<string, List<Action<RedisChannel, RedisValue>>>();
        public RedisWrapper()
        {
            ConnectTask = ConnectRedis();
            TeamCodingPackage.Current.Settings.SharedSettings.RedisServerChanged += SharedSettings_RedisServerChanged;
        }
        private async Task ChangeRedisServer()
        {
            // We don't worry about the result of the task as any exceptions are already handled
            await ConnectTask;
            ResetRedis();
            await ConnectRedis();
        }
        private void SharedSettings_RedisServerChanged(object sender, EventArgs e)
        {
            ConnectTask = ChangeRedisServer();
        }
        private async Task ConnectRedis()
        {
            var redisServer = TeamCodingPackage.Current.Settings.SharedSettings.RedisServer;
            if (!string.IsNullOrWhiteSpace(redisServer))
            {
                TeamCodingPackage.Current.Logger.WriteInformation($"Connecting to Redis using config string: \"{redisServer}\"");
                RedisClient = await ConnectionMultiplexer.ConnectAsync(redisServer)
                    .HandleException((ex) => TeamCodingPackage.Current.Logger.WriteError($"Failed to connect to redis server using config string: {redisServer}"));
                TeamCodingPackage.Current.Logger.WriteInformation($"Connected to Redis using config string: \"{redisServer}\"");

                if (RedisClient != null)
                {
                    IEnumerable<Task> tasks;
                    lock (SubscribedActions)
                    {
                        var subscriber = RedisClient.GetSubscriber();
                        tasks = SubscribedActions.Keys.SelectMany(key => SubscribedActions[key].Select((a) => subscriber.SubscribeAsync(key, a)?.HandleException()));
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }

        internal async Task Publish(string channel, byte[] data)
        {
            await ConnectTask; // Wait to be connected first

            if (RedisClient != null)
            {
                await RedisClient?.GetSubscriber()?.PublishAsync(channel, data)?.HandleException();
                TeamCodingPackage.Current.Logger.WriteInformation("Sent model");
            }
            else
            {
                TeamCodingPackage.Current.Logger.WriteInformation("Redisclient == null, didn't send model");
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
            if (RedisClient != null)
            {
                await RedisClient?.GetSubscriber()?.SubscribeAsync(channel, action)?.HandleException();
                TeamCodingPackage.Current.Logger.WriteInformation("Subscribed");
            }
            else
            {
                TeamCodingPackage.Current.Logger.WriteInformation("Redisclient == null, didn't subscribe");
            }
        }
        private void ResetRedis()
        {
            RedisClient?.Dispose();
            RedisClient = null;
        }
        public void Dispose()
        {
            ResetRedis();
        }
    }
}
