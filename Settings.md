# Settings

There are shared settings (AKA Sync Settings), used to share IDE actions and User settings, used to control how you appear to other users. User Settings are optional, Shared Settings are not; See below for details.

## Shared Settings

In order to see other user's actions within the IDE you need to set up at least one method of syncronisation. You can use multiple methods in case one is intermittently unavailable although this is a rare use-case.

- **UNC File Share**. Define a path to a shared folder. Files will be placed in this folder to send changes and new/changed files will be picked up to receive changes. Read/write access required.
- **Redis**. Define the redis configuration string (refer to Redis docs for details. In the simple case this can just be the hostname of the redis server). The publish/subscribe method will be used to send and receive changes.
- **Slack**. Define the slack bot token and channel. Messages will be posted to the channel specified by the slack bot idenfitied by the access token. There will be a lot of messages, so it's recommended for you to unsubscribe from the channel.

**User Settings**

Using the user settings you can customise how you appear to others.

- **Username**. The username people see when hovering over your avatar that appears on a tab. The first letter is used as the avatar if no image url is specified.
- **Image URL**. The URL of an image to use as your avatar that appears on a tab or within a document.