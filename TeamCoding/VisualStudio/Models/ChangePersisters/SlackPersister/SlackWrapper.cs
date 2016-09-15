using SlackConnector;
using SlackConnector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SlackPersister
{
    public class SlackWrapper : IDisposable
    { // TODO: Add text/message somewhere (maybe config tooltip) indicating that the channel needs setting up (disable notifications!), how to create/install the slack bot etc.
        private string BotId = null;
        private Task ConnectTask;
        private ISlackConnection SlackClient;
        private List<Action<SlackMessage>> SubscribedActions = new List<Action<SlackMessage>>();
        public SlackWrapper()
        {
            ConnectTask = ConnectSlack();
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanged += SharedSettings_SlackTokenChanged;
        }
        private async Task ChangeRedisServer()
        {
            // We don't worry about the result of the task as any exceptions are already handled
            await ConnectTask;
            ResetSlack();
            await ConnectSlack();
        }
        private void SharedSettings_SlackTokenChanged(object sender, EventArgs e)
        {
            ConnectTask = ChangeRedisServer();
        }
        private async Task ConnectSlack()
        {
            var slackToken = TeamCodingPackage.Current.Settings.SharedSettings.SlackToken;
            if (!string.IsNullOrWhiteSpace(slackToken))
            {
                TeamCodingPackage.Current.Logger.WriteInformation($"Connecting to Slack using token: \"{slackToken}\"");
                SlackClient = await new SlackConnector.SlackConnector().Connect(slackToken)
                    .HandleException((ex) => TeamCodingPackage.Current.Logger.WriteError($"Failed to connect to Slack using token: {slackToken}", ex));
                if (SlackClient?.IsConnected ?? false)
                {
                    TeamCodingPackage.Current.Logger.WriteInformation($"Connected to Slack using token: \"{slackToken}\"");
                }

                if (SlackClient != null)
                {
                    BotId = SlackClient.Self.Id;
                    // Scrub the Self.Id field so we don't ignore ourselves
                    typeof(ContactDetails).GetProperty(nameof(ContactDetails.Id)).SetValue(SlackClient.Self, null);

                    SlackClient.OnMessageReceived += SlackClient_OnMessageReceived;
                    SlackClient.OnDisconnect += SlackClient_OnDisconnect;
                }
            }
        }

        private async void SlackClient_OnDisconnect()
        {
            TeamCodingPackage.Current.Logger.WriteError($"Disconnected from Slack, trying to re-connect");
            await ConnectSlack();
        }

        private Task SlackClient_OnMessageReceived(SlackMessage message)
        {
            if (message.User.Id == BotId)
            {
                if (message.ChatHub.Name == "#teamcodingsync")
                {
                    lock (SubscribedActions)
                    {
                        foreach (var action in SubscribedActions)
                        {
                            action(message);
                        }
                    }
                }
            }
            else
            {
                // TODO: Do something when a user talks to the slack bot!
            }
            return Task.CompletedTask;
        }
        internal async Task Publish(BotMessage message)
        {
            await ConnectTask; // Wait to be connected first

            if (SlackClient != null)
            {
                var hub = SlackClient.ConnectedHubs.Select(kv => kv.Value).SingleOrDefault(h => h.Name == "#teamcodingsync"); // TODO: Allow channel to be customised (and add tooltips for slack settings)
                if (hub != null)
                { // TODO: Check the hub is a channel
                    message.ChatHub = hub;
                    await SlackClient.Say(message).HandleException();
                    TeamCodingPackage.Current.Logger.WriteInformation("Sent model");
                }
                else
                {
                    TeamCodingPackage.Current.Logger.WriteInformation("Hub not found");
                }
            }
            else
            {
                TeamCodingPackage.Current.Logger.WriteInformation("SlackClient == null, didn't send model");
            }
        }
        internal Task Subscribe(Action<SlackMessage> action)
        {
            lock (SubscribedActions)
            {
                SubscribedActions.Add(action);
            }
            return Task.CompletedTask;
        }
        private void ResetSlack()
        {
            if (SlackClient != null)
            {
                SlackClient.OnDisconnect -= SlackClient_OnDisconnect;
                SlackClient.Disconnect();
                SlackClient = null;
            }
        }
        public void Dispose()
        {
            ResetSlack();
        }
    }
}
