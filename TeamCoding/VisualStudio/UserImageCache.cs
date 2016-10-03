﻿using System;
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
        public UserAvatar CreateUserIdentityControl(IUserIdentity userIdentity, bool forDocumentTab = false)
        { // TODO: if we're calling this for a document tab break the borderbrush and tooltip text bindings and set the appropriately here
            var context = CreateUserAvatarModel(userIdentity);
            return new UserAvatar() { DataContext = context };
        }
        public UserAvatarModel CreateUserAvatarModel(IUserIdentity userIdentity)
        {
            var context = new UserAvatarModel();
            context.ToolTip = (userIdentity.DisplayName ?? userIdentity.Id);
            context.BorderBrush = context.BackgroundBrush = UserColours.GetUserBrush(userIdentity);

            // TODO: Cache the avatar models
            // TODO: When display settings change update the avatar models
            SetContextAccordingToDisplaySettings(context, userIdentity);

            return context;
        }
        private void SetContextAccordingToDisplaySettings(UserAvatarModel context, IUserIdentity userIdentity)
        {
            SetText(context, userIdentity);
            SetImageSource(context, userIdentity);
        }
        private void SetText(UserAvatarModel context, IUserIdentity userIdentity)
        {
            if (TeamCodingPackage.Current.Settings.UserSettings.UserCodeDisplay != UserSettings.UserDisplaySetting.Colour)
            {
                var firstLetter = (userIdentity.Id)[0];
                context.Letter = firstLetter;

                context.LetterBrush = VisuallyDistinctColours.GetTextBrushFromBackgroundColour(UserColours.GetUserColour(userIdentity));
            }
        }
        private void SetImageSource(UserAvatarModel context, IUserIdentity userIdentity)
        {
            if (TeamCodingPackage.Current.Settings.UserSettings.UserCodeDisplay == UserSettings.UserDisplaySetting.Avatar)
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
