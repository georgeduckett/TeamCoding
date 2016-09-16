using SlackConnector.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.SlackPersister
{
    public class ObjectSlackMessageConverter
    {
        public BotMessage ToBotMessage(RemoteIDEModel model)
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
            if (obj == null)
            {
                yield return null;
            }

            if (obj.GetType().IsValueType || obj.GetType() == typeof(string))
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


                if (memberType == typeof(string))
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
                        foreach (var field in GetSlackFields(enumObj[i], $"{propNameWithPrefix}[{i}]"))
                        {
                            yield return field;
                        }
                    }
                }
                else if (memberType.IsClass)
                {
                    foreach (var field in GetSlackFields(memberValue, propNameWithPrefix.ToString()))
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
        public RemoteIDEModel ToIdeModel(BotMessage receivedMessage)
        {
            var result = new RemoteIDEModel();

            foreach (var slackField in receivedMessage.Attachments.SelectMany(a => a.Fields))
            {
                var propNames = slackField.Title.Split('.');
                var lastPropName = propNames.Last();
                propNames = propNames.Take(propNames.Length - 1).ToArray();

                var propObj = (object)result;

                foreach (var propName in propNames)
                {
                    var trimmedPropName = propName;
                    int? arrayIndex = null;

                    if (trimmedPropName.Contains('['))
                    {
                        arrayIndex = int.Parse(trimmedPropName.Substring(trimmedPropName.IndexOf('[') + 1, trimmedPropName.IndexOf(']') - trimmedPropName.IndexOf('[') - 1));
                        trimmedPropName = trimmedPropName.Substring(0, trimmedPropName.IndexOf('['));
                    }

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
                        // NOTE: This might need changing to be simiar to the Array handling below if we use this for objects with an array of a coplex type (not the final property)
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
                        throw new InvalidDataException($"Invalid property/field name found: {lastPropName} in sequence {slackField.Title}.");
                    }


                    var listObj = (System.Collections.IList)lastObj;
                    var arrayObj = lastObj as Array;

                    if (arrayObj != null)
                    {
                        if (arrayObj.Length < lastPropIndex + 1)
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
                        throw new InvalidDataException($"Property/field had brackets [] but is not an IList or Array: {lastPropName} in sequence {slackField.Title}.");
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
                        throw new InvalidDataException($"Invalid property/field name found: {lastPropName} in sequence {slackField.Title}.");
                    }
                }
            }

            return result;
        }
    }
}
