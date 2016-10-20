using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private readonly static SemaphoreSlim GetServerStringErrorTextSemaphore = new SemaphoreSlim(1, 1);
        public static async Task<string> GetServerStringErrorText(string serverString)
        {            
            if (string.IsNullOrWhiteSpace(serverString))
            {
                return "Server cannot be purely whitespace";
            }
            await GetServerStringErrorTextSemaphore.WaitAsync();
            try
            {
                using (var redisClient = await ConnectionMultiplexer.ConnectAsync(serverString))
                {
                    if (!redisClient.IsConnected)
                    {
                        return "Could not connect to redis server";
                    }

                    var subscribeTriggerEvent = new ManualResetEventSlim();
                    const string testChannel = "TeamCoding.RedisWrapper.Test";
                    var testValue = "test" + DateTime.UtcNow.ToString();
                    string receivedValue = null;
                    Action<RedisChannel, RedisValue> testHandler = (c, v) =>
                        {
                            if (v.ToString() != testValue)
                            {
                                receivedValue = v.ToString();
                            }
                            subscribeTriggerEvent.Set();
                        };
                    var subscriber = redisClient.GetSubscriber();
                    await subscriber.SubscribeAsync(testChannel, testHandler);
                    await Task.Delay(1000);
                    await subscriber.PublishAsync(testChannel, testValue);

                    if (subscribeTriggerEvent.Wait(10000))
                    {
                        await subscriber.UnsubscribeAsync(testChannel, testHandler);
                        if (receivedValue != null)
                        {
                            return $"Value recieved did not match value sent.{Environment.NewLine}Sent: {testValue}{Environment.NewLine}Received {receivedValue}";
                        }
                    }
                    else
                    {
                        await subscriber.UnsubscribeAsync(testChannel, testHandler);
                        return "Could not send and receive test message after 10 seconds";
                    }
                }
            }
            finally
            {
                GetServerStringErrorTextSemaphore.Release();
            }

            return null;
        }
        private async Task ConnectRedis()
        {
            var redisServer = TeamCodingPackage.Current.Settings.SharedSettings.RedisServer;
            if (!string.IsNullOrWhiteSpace(redisServer))
            {
                TeamCodingPackage.Current.Logger.WriteInformation($"Connecting to Redis using config string: \"{redisServer}\"");
                RedisClient = await ConnectionMultiplexer.ConnectAsync(redisServer)
                    .HandleException((ex) => TeamCodingPackage.Current.Logger.WriteError($"Failed to connect to redis server using config string: {redisServer}"));

                if (RedisClient != null)
                {
                    TeamCodingPackage.Current.Logger.WriteInformation($"Connected to Redis using config string: \"{redisServer}\"");
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
            else if(!string.IsNullOrEmpty(TeamCodingPackage.Current.Settings.SharedSettings.RedisServer))
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
