using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeamCoding.Documents;
using TeamCoding.IdentityManagement;
using TeamCoding.Extensions;
using Microsoft.VisualStudio.Shell;
using TeamCoding.Options;
using TeamCoding.VisualStudio.Controls;

namespace TeamCoding.VisualStudio
{
    /// <summary>
    /// Maintains a cache of images of users
    /// </summary>
    public class UserImageCache
    {
        private static readonly Brush DocSelectedBorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 1.0f, ScG = 1.0f, ScB = 1.0f });
        private static readonly Brush DocEditedBorderBrush = new SolidColorBrush(new Color() { ScA = 0.65f, ScR = 0.5f, ScG = 0.5f, ScB = 0.5f });
        private readonly Dictionary<string, ImageSource> UrlImages = new Dictionary<string, ImageSource>();
        private readonly Dictionary<string, UserAvatarModel> UserModels = new Dictionary<string, UserAvatarModel>();
        public UserImageCache()
        {
            TeamCodingPackage.Current.Settings.UserSettings.UserTabDisplayChanged += UserSettings_UserTabDisplayChanged;
        }
        private void UserSettings_UserTabDisplayChanged(object sender, EventArgs e)
        {
            var users = TeamCodingPackage.Current
                                         .RemoteModelChangeManager
                                         .GetOpenFiles()
                                         .GroupBy(of => of.IdeUserIdentity)
                                         .Select(g => g.Key).GroupBy(ui => ui.Id)
                                         .ToDictionary(g=> g.Key, g => g.First());

            foreach (var context in UserModels)
            {
                SetContextAccordingToDisplaySettings(context.Value, users[context.Key]);
            }
        }
        public UserAvatar CreateUserIdentityControl(IUserIdentity userIdentity)
        {
            var context = CreateUserAvatarModel(userIdentity);
            return new UserAvatar() { DataContext = context };
        }
        public UserAvatarModel CreateUserAvatarModel(IUserIdentity userIdentity)
        {
            UserAvatarModel context;
            if(UserModels.TryGetValue(userIdentity.Id, out context))
            {
                return context;
            }

            context = new UserAvatarModel();
            context.ToolTip = (userIdentity.DisplayName ?? userIdentity.Id);
            context.Tag = userIdentity.Id;
            context.BorderBrush = context.BackgroundBrush = UserColours.GetUserBrush(userIdentity);
            
            SetContextAccordingToDisplaySettings(context, userIdentity);

            UserModels.Add(userIdentity.Id, context);

            return context;
        }
        private void SetContextAccordingToDisplaySettings(UserAvatarModel context, IUserIdentity userIdentity)
        {
            SetText(context, userIdentity);
            SetImageSource(context, userIdentity);
        }
        private void SetText(UserAvatarModel context, IUserIdentity userIdentity)
        {
            if (TeamCodingPackage.Current.Settings.UserSettings.UserTabDisplay == UserSettings.UserDisplaySetting.Colour)
            {
                context.Letter = null;
            }
            else
            {
                var firstLetter = (userIdentity.Id)[0];
                context.Letter = firstLetter;

                context.LetterBrush = VisuallyDistinctColours.GetTextBrushFromBackgroundColour(UserColours.GetUserColour(userIdentity));
            }
        }
        private void SetImageSource(UserAvatarModel context, IUserIdentity userIdentity)
        {
            if (TeamCodingPackage.Current.Settings.UserSettings.UserTabDisplay == UserSettings.UserDisplaySetting.Avatar)
            {
                if (userIdentity.ImageBytes != null)
                {
                    using (var MS = new MemoryStream(userIdentity.ImageBytes))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = MS;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        context.AvatarImageSource = UrlImages[userIdentity.ImageUrl] = bitmap;
                    }
                }
                else if (userIdentity.ImageUrl != null)
                {
                    if (UrlImages.ContainsKey(userIdentity.ImageUrl))
                    {
                        context.AvatarImageSource = UrlImages[userIdentity.ImageUrl];
                    }
                    else
                    {
                        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                        {
                            try
                            {
                                var request = await TeamCodingPackage.Current.HttpClient.GetAsync(userIdentity.ImageUrl);
                                if (!request.IsSuccessStatusCode) return;
                                var imageStream = await request.Content.ReadAsStreamAsync();
                                context.AvatarImageSource = UrlImages[userIdentity.ImageUrl] = BitmapFrame.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            }
                            catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
                            {
                                TeamCodingPackage.Current.Logger.WriteError(ex);
                            }
                        });
                    }
                }
            }
            else
            {
                context.AvatarImageSource = null;
            }
        }
        /// <summary>
        /// Sets properties specific to a user control in a document tab
        /// </summary>
        /// <param name="context"></param>
        /// <param name="matchedRemoteDoc"></param>
        /// <param name="displaySetting"></param>
        internal void SetDocumentTabUserControlProperties(UserAvatar control, IRemotelyAccessedDocumentData matchedRemoteDoc)
        {
            control.ToolTip = (matchedRemoteDoc.IdeUserIdentity.DisplayName ?? matchedRemoteDoc.IdeUserIdentity.Id) + (matchedRemoteDoc.BeingEdited ? " [edited]" : string.Empty);

            if (matchedRemoteDoc.HasFocus)
            {
                control.UserBorderVisibility = Visibility.Visible;
                control.UserBorderBrush = DocSelectedBorderBrush;
            }
            else if (matchedRemoteDoc.BeingEdited)
            {
                control.UserBorderVisibility = Visibility.Visible;
                control.UserBorderBrush = DocEditedBorderBrush;
            }
            else
            {
                control.UserBorderVisibility = Visibility.Hidden;
            }
        }
    }
}
