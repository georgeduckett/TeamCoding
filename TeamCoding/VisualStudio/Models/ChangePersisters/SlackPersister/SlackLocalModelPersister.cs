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
    public class SlackLocalModelPersister : ILocalModelPerisister
    {
        private readonly LocalIDEModel IdeModel;
        public SlackLocalModelPersister(LocalIDEModel model)
        {
            IdeModel = model;
            IdeModel.OpenViewsChanged += IdeModel_OpenViewsChanged;
            IdeModel.TextContentChanged += IdeModel_TextContentChanged;
            IdeModel.TextDocumentSaved += IdeModel_TextDocumentSaved;
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanging += SharedSettings_SlackServerChanging;
            TeamCodingPackage.Current.Settings.SharedSettings.SlackTokenChanged += SharedSettings_SlackServerChanged;
        }

        private void SharedSettings_SlackServerChanged(object sender, EventArgs e)
        {
            SendChanges();
        }

        private void SharedSettings_SlackServerChanging(object sender, EventArgs e)
        {
            SendModel(new RemoteIDEModel(new LocalIDEModel()));
        }

        private void IdeModel_TextDocumentSaved(object sender, Microsoft.VisualStudio.Text.TextDocumentFileActionEventArgs e)
        {
            SendChanges();
        }
        private void IdeModel_TextContentChanged(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            // SendChanges();
        }
        private void IdeModel_OpenViewsChanged(object sender, EventArgs e)
        {
            SendChanges();
        }
        protected virtual void SendChanges()
        {
            SendModel(new RemoteIDEModel(IdeModel));
        }
        private void SendModel(RemoteIDEModel remoteModel)
        {
            TeamCodingPackage.Current.Logger.WriteInformation("Publishing Model");
            TeamCodingPackage.Current.Slack.Publish(ToBotMessage(remoteModel)).HandleException();
        }
        private BotMessage ToBotMessage(RemoteIDEModel model)
        {
            var message = new BotMessage()
            {
                Text = $"{model.IDEUserIdentity.DisplayName} is doing some coding!"
            };

            var attachment = new SlackAttachment()
            {
                ThumbUrl = model.IDEUserIdentity.ImageUrl,
                AuthorName = model.IDEUserIdentity.DisplayName,
                AuthorLink = model.IDEUserIdentity.Id,
                AuthorIcon = model.IDEUserIdentity.ImageUrl,
                ColorHex = new System.Windows.Media.ColorConverter().ConvertToString(model.IDEUserIdentity.GetUserColour()),
                Fallback = model.OpenFiles.Count == 0 ?
                               $"{ model.IDEUserIdentity.DisplayName} has no files open." :
                               $"{model.IDEUserIdentity.DisplayName} has {model.OpenFiles.Count} files open in {model.OpenFiles.FirstOrDefault()?.RepoUrl}"
            };
            foreach (var field in GetSlackFields(model).OrderBy(f => f.Title.Count(c => c == '.')).ThenBy(f => f.Title))
            {
                attachment.Fields.Add(field);
            }

            message.Attachments.Add(attachment);

            return message;
        }
        private IEnumerable<SlackAttachmentField> GetSlackFields(object obj, string propNamePrefix = "")
        {
            if(obj == null)
            {
                yield return null;
            }

            if(obj.GetType().IsValueType || obj.GetType() == typeof(string))
            {
                yield return new SlackAttachmentField()
                {
                    IsShort = true,
                    Title = propNamePrefix,
                    Value = obj.ToString()
                };

                yield break;
            }

            foreach (var member in obj.GetType().GetMembers())
            {
                if (member.GetCustomAttributes(typeof(ProtoBuf.ProtoIgnoreAttribute), true).Any()) continue;

                object memberValue;
                Type memberType;

                if (member.MemberType == MemberTypes.Property)
                {
                    memberValue = ((PropertyInfo)member).GetValue(obj);
                    memberType = ((PropertyInfo)member).PropertyType;
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    memberValue = ((FieldInfo)member).GetValue(obj);
                    memberType = ((FieldInfo)member).FieldType;
                }
                else
                {
                    continue;
                }


                if (memberValue == null) continue;

                var propNameWithPrefix = new StringBuilder(propNamePrefix);

                if (!string.IsNullOrEmpty(propNamePrefix))
                {
                    propNameWithPrefix.Append('.');
                }

                propNameWithPrefix.Append(member.Name);


                if(memberType == typeof(string))
                {
                    yield return new SlackAttachmentField()
                    {
                        IsShort = true,
                        Title = propNameWithPrefix.ToString(),
                        Value = (string)memberValue
                    };
                }
                else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(memberType))
                {
                    var enumObj = (System.Collections.IList)memberValue;

                    for (int i = 0; i < enumObj.Count; i++)
                    {
                        foreach(var field in GetSlackFields(enumObj[i], $"{propNameWithPrefix}[{i}]"))
                        {
                            yield return field;
                        }
                    }
                }
                else if (memberType.IsClass)
                {
                    foreach(var field in GetSlackFields(memberValue, propNameWithPrefix.ToString()))
                    {
                        yield return field;
                    }
                }
                else
                {
                    yield return new SlackAttachmentField()
                    {
                        IsShort = true,
                        Title = propNameWithPrefix.ToString(),
                        Value = memberValue.ToString()
                    };
                }
            }
        }
        public void Dispose()
        {

        }
    }
}
