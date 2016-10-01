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
        public UserAvatarModel CreateUserAvatarModel(IUserIdentity userIdentity, UserSettings.UserDisplaySetting displaySetting)
        {
            var context = new UserAvatarModel();
            context.BackgroundBrush = UserColours.GetUserBrush(userIdentity);
            SetText(context, userIdentity, displaySetting);

            if (userIdentity.ImageUrl != null && displaySetting == UserSettings.UserDisplaySetting.Avatar)
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

            return context;
        }
        public UserAvatar CreateUserIdentityControl(IUserIdentity userIdentity, UserSettings.UserDisplaySetting displaySetting)
        {
            var context = CreateUserAvatarModel(userIdentity, displaySetting);
            return new UserAvatar() { DataContext = context };
        }
        internal void SetUserControlProperties(UserAvatar parentControl, IRemotelyAccessedDocumentData matchedRemoteDoc, UserSettings.UserDisplaySetting displaySetting)
        {
            var context = (UserAvatarModel)parentControl.DataContext;
            SetText(context, matchedRemoteDoc.IdeUserIdentity, displaySetting);
            context.ToolTip = (matchedRemoteDoc.IdeUserIdentity.DisplayName ?? matchedRemoteDoc.IdeUserIdentity.Id) + (matchedRemoteDoc.BeingEdited ? " [edited]" : string.Empty);

            if (matchedRemoteDoc.HasFocus)
            {
                context.BorderVisibility = Visibility.Visible;
                context.BorderBrush = DocSelectedBorderBrush;
            }
            else if (matchedRemoteDoc.BeingEdited)
            {
                context.BorderVisibility = Visibility.Visible;
                context.BorderBrush = DocEditedBorderBrush;
            }
            else
            {
                context.BorderVisibility = Visibility.Hidden;
            }

            if (displaySetting == UserSettings.UserDisplaySetting.Avatar)
            {
                SetImageSource(context, matchedRemoteDoc);
            }
        }
        public static void SetText(UserAvatarModel context, IUserIdentity userIdentity, UserSettings.UserDisplaySetting displaySetting)
        {
            if (displaySetting != UserSettings.UserDisplaySetting.Colour)
            {
                var firstLetter = (userIdentity.Id)[0];
                context.Letter = firstLetter;

                context.LetterBrush = VisuallyDistinctColours.GetTextBrushFromBackgroundColour(UserColours.GetUserColour(userIdentity));
            }
        }
        private void SetImageSource(UserAvatarModel context, IRemotelyAccessedDocumentData matchedRemoteDoc)
        {
            ImageSource imageSource = null;
            if (matchedRemoteDoc.IdeUserIdentity.ImageBytes != null)
            {
                using (var MS = new MemoryStream(matchedRemoteDoc.IdeUserIdentity.ImageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = MS;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imageSource = bitmap;
                }
            }
            else if (matchedRemoteDoc.IdeUserIdentity.ImageUrl != null)
            {
                if (UrlImages.ContainsKey(matchedRemoteDoc.IdeUserIdentity.ImageUrl))
                {
                    imageSource = UrlImages[matchedRemoteDoc.IdeUserIdentity.ImageUrl];
                }
            }

            context.AvatarImageSource = imageSource;
        }
    }
}
