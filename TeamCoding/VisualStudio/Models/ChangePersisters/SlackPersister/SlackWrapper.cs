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
    {
        private string BotId = null;
        private Task ConnectTask;
        private ISlackConnection SlackClient;
        private List<Action<SlackMessage>> SubscribedActions = new List<Action<SlackMessage>>();
        public SlackWrapper()
        {
            ConnectTask = ConnectSlack();
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanged += SharedSettings_SlackSettingsChanged;
            TeamCodingPackage.Current.Settings.SharedSettings.SlackChannelChanged += SharedSettings_SlackSettingsChanged;
        }
        private async Task ChangeSlackServer()
        {
            // We don't worry about the result of the task as any exceptions are already handled
            await ConnectTask;
            ResetSlack();
            await ConnectSlack();
        }
        private void SharedSettings_SlackSettingsChanged(object sender, EventArgs e)
        {
            ConnectTask = ChangeSlackServer();
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
                if (message.ChatHub.Name == TeamCodingPackage.Current.Settings.SharedSettings.SlackChannel)
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
                // Don't reply back to messages from others, otherwise the user will get a message per user using the extension
            }
            return Task.CompletedTask;
        }
        internal async Task Publish(BotMessage message)
        {
            await ConnectTask; // Wait to be connected first

            if (SlackClient != null)
            {
                var hub = SlackClient.ConnectedHubs.Select(kv => kv.Value).SingleOrDefault(h => h.Name == TeamCodingPackage.Current.Settings.SharedSettings.SlackChannel);
                if (hub != null)
                {
                    message.ChatHub = hub;
                    await SlackClient.Say(message).HandleException();
                    TeamCodingPackage.Current.Logger.WriteInformation("Sent model via Slack");
                }
                else if (string.IsNullOrEmpty(TeamCodingPackage.Current.Settings.SharedSettings.SlackChannel))
                {
                    TeamCodingPackage.Current.Logger.WriteInformation($"Slack channel not specified.");
                }
                else if (!TeamCodingPackage.Current.Settings.SharedSettings.SlackChannel.StartsWith("#"))
                {
                    TeamCodingPackage.Current.Logger.WriteInformation($"Slack channel must start with a #. It is set as {TeamCodingPackage.Current.Settings.SharedSettings.SlackChannel}");
                }
                else
                {
                    TeamCodingPackage.Current.Logger.WriteInformation($"Slack channel {TeamCodingPackage.Current.Settings.SharedSettings.SlackChannel} not found (it doesn't exist, or the bot the API token is for doesn't have access).");
                }
            }
            else if(!string.IsNullOrEmpty(TeamCodingPackage.Current.Settings.SharedSettings.SlackToken))
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
