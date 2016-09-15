using SlackConnector.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SlackPersister
{
    public class SlackRemoteModelPersister : RemoteModelPersisterBase
    {
        private readonly Task SubscribeTask;
        public SlackRemoteModelPersister()
        {
            SubscribeTask = TeamCodingPackage.Current.Slack.Subscribe(Slack_RemoteModelReceived).HandleException();
        }
        private void Slack_RemoteModelReceived(SlackMessage message)
        {
            // TODO: Handle an invalid message being received
            var receivedMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<BotMessage>(message.RawData);

            var receivedModel = ToIdeModel(receivedMessage);

            receivedModel.IDEUserIdentity.ImageUrl = receivedModel.IDEUserIdentity.ImageUrl.TrimStart('<').TrimEnd('>');
            if (receivedModel.IDEUserIdentity.DisplayName.Contains("|"))
            {
                receivedModel.IDEUserIdentity.DisplayName = receivedModel.IDEUserIdentity.DisplayName.Substring(receivedModel.IDEUserIdentity.DisplayName.IndexOf('|') + 1).TrimEnd('>');
            }
            if (receivedModel.IDEUserIdentity.Id.Contains("|"))
            {
                receivedModel.IDEUserIdentity.Id = receivedModel.IDEUserIdentity.Id.Substring(receivedModel.IDEUserIdentity.Id.IndexOf('|') + 1).TrimEnd('>');
            }

            foreach (var openFile in receivedModel.OpenFiles)
            {
                openFile.RepoUrl = openFile.RepoUrl.TrimStart('<').TrimEnd('>');
            }

            OnRemoteModelReceived(receivedModel);
        }

        private RemoteIDEModel ToIdeModel(BotMessage receivedMessage)
        {
            var result = new RemoteIDEModel();

            foreach(var slackField in receivedMessage.Attachments.SelectMany(a => a.Fields))
            {
                var propNames = slackField.Title.Split('.');
                var lastPropName = propNames.Last();
                propNames = propNames.Take(propNames.Length - 1).ToArray();

                var propObj = (object)result;

                foreach(var propName in propNames)
                {
                    var trimmedPropName = propName;
                    int? arrayIndex = null;

                    if (trimmedPropName.Contains('['))
                    {
                        arrayIndex = int.Parse(trimmedPropName.Substring(trimmedPropName.IndexOf('[') + 1, trimmedPropName.IndexOf(']') - trimmedPropName.IndexOf('[') - 1));
                        trimmedPropName = trimmedPropName.Substring(0, trimmedPropName.IndexOf('['));
                    }

                    // TODO: Handle prop names with indexes
                    var prop = propObj.GetType().GetProperty(trimmedPropName);
                    var field = propObj.GetType().GetField(trimmedPropName);
                    if (prop != null)
                    {
                        if (prop.GetValue(propObj) == null)
                        {
                            prop.SetValue(propObj, Activator.CreateInstance(prop.PropertyType));
                        }
                        propObj = prop.GetValue(propObj);
                    }
                    else if (field != null)
                    { 
                        if (field.GetValue(propObj) == null)
                        {
                            field.SetValue(propObj, Activator.CreateInstance(field.FieldType));
                        }
                        propObj = field.GetValue(propObj);
                    }
                    else
                    {
                        throw new InvalidDataException($"Invalid property/field name found: {trimmedPropName} in sequence {slackField.Title}.");
                    }

                    if (arrayIndex != null)
                    {
                        var listObj = (System.Collections.IList)propObj;

                        while (listObj.Count < arrayIndex + 1)
                        {
                            listObj.Add(Activator.CreateInstance(listObj.GetType().GetGenericArguments()[0]));
                        }

                        propObj = listObj[arrayIndex.Value];
                    }
                }

                int? lastPropIndex = null;
                if (lastPropName.Contains('['))
                {
                    lastPropIndex = int.Parse(lastPropName.Substring(lastPropName.IndexOf('[') + 1, lastPropName.IndexOf(']') - lastPropName.IndexOf('[') - 1));
                    lastPropName = lastPropName.Substring(0, lastPropName.IndexOf('['));

                    var lastProp = propObj.GetType().GetProperty(lastPropName);
                    var lastField = propObj.GetType().GetField(lastPropName);
                    var lastObj = propObj;

                    if (lastProp != null)
                    {
                        lastObj = lastProp.GetValue(propObj);
                        if (lastObj == null)
                        {
                            if (lastProp.PropertyType.IsSubclassOf(typeof(Array)))
                            {
                                lastObj = Activator.CreateInstance(lastProp.PropertyType, 0);
                            }
                            else
                            {
                                lastObj = Activator.CreateInstance(lastProp.PropertyType);
                            }
                        }
                    }
                    else if (lastField != null)
                    {
                        lastObj = lastField.GetValue(propObj);
                        if (lastObj == null)
                        {
                            if (lastProp.PropertyType.IsSubclassOf(typeof(Array)))
                            {
                                lastObj = Activator.CreateInstance(lastField.FieldType, 0);
                            }
                            else
                            {
                                lastObj = Activator.CreateInstance(lastField.FieldType);
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }


                    var listObj = (System.Collections.IList)lastObj;
                    var arrayObj = lastObj as Array;

                    if(arrayObj != null)
                    {
                        if(arrayObj.Length < lastPropIndex + 1)
                        {
                            var newArray = (Array)Activator.CreateInstance(arrayObj.GetType(), lastPropIndex + 1);
                            Array.Copy(arrayObj, newArray, arrayObj.Length);
                            arrayObj = newArray;
                        }

                        arrayObj.SetValue(Convert.ChangeType(slackField.Value, arrayObj.GetType().GetElementType()), lastPropIndex.Value);
                        lastProp.SetValue(propObj, arrayObj);
                    }
                    else if (listObj != null)
                    {
                        while (listObj.Count < lastPropIndex + 1)
                        {
                            listObj.Add(Activator.CreateInstance(listObj.GetType().GetGenericArguments()[0]));
                        }

                        listObj[lastPropIndex.Value] = Convert.ChangeType(slackField.Value, listObj.GetType().GetGenericArguments()[0]);
                        lastProp.SetValue(propObj, listObj);
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    var lastProp = propObj.GetType().GetProperty(lastPropName);
                    var lastField = propObj.GetType().GetField(lastPropName);
                    if (lastProp != null)
                    {
                        lastProp.SetValue(propObj, Convert.ChangeType(slackField.Value, lastProp.PropertyType));
                    }
                    else if (lastField != null)
                    {
                        lastField.SetValue(propObj, Convert.ChangeType(slackField.Value, lastField.FieldType));
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            return result;
        }

        public override void Dispose()
        {
            Task.WaitAll(SubscribeTask);
            base.Dispose();
        }
    }
}
