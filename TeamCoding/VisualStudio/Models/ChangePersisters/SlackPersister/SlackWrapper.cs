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
        private Task ConnectTask;
        private ISlackConnection SlackClient;
        private List<Action<string>> SubscribedActions = new List<Action<string>>();
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
            lock (SubscribedActions)
            {
                foreach(var action in SubscribedActions)
                {
                    action(message.RawData);
                }
            }
            return Task.CompletedTask;
        }

        internal async Task Publish(string data)
        {
            await ConnectTask; // Wait to be connected first

            if (SlackClient != null)
            {
                // TODO: Maybe use slack attachments
                await SlackClient.Say(new BotMessage() { Text = data }).HandleException();
                TeamCodingPackage.Current.Logger.WriteInformation("Sent model");
            }
            else
            {
                TeamCodingPackage.Current.Logger.WriteInformation("SlackClient == null, didn't send model");
            }
        }
        internal Task Subscribe(Action<string> action)
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
