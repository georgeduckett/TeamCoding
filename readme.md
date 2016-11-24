Team Coding
--

**About:** The Team Coding Visual Studio extension helps discover team members who are working in the same area of a source controlled solution as you. It displays icons on tabs, text in CodeLens is as well as showing a coloured caret where they are working for C# and visual basic code.

**Quickstart:** There are 2 ways Team Coding can be minimally configured.
In the options menu (under Team Coding) where you can also set user settings and/or
by opening a repository with a `teamcoding.json` file in it (shared settings will be taken from there).
An example of a valid `teamcoding.json` file can be found in the options menu.
The borders around the user image at the top of open tabs are coloured white for the user's selected tab and grey for an edited document tab.
You can test the extension on your own, or test that you've got a sharing method correctly set up by ticking the "Show yourself" checkbox.

**Settings:** The settings are located at Tools->Options->Team Coding.
There are configuration options for your user identity (how you appear to others) and how others appear to you, as well as configuration options for how to share open document information with others.
Multiple methods can be used at once, you can also add a `teamcoding.json` file anywhere within the solution folder for it to override those settings.
If this file is in place then for that solution settings don't need to be set in each developer's IDE.
You can enable/disable the CodeLens addon in Tools->Options->Text Editor->All Language->Code Lens->Team Coding.
For more details regarding the different user and shared options, see [Settings.md](https://github.com/georgeduckett/TeamCoding/blob/master/Settings.md)

**User Identity:** It tries to get your email address as a user identity from various sources (saved Windows Credentials for GitHub, your logged in identity from Visual Studio, your machine name).
This will be made public to your team. It uses Gravatar to get a user image from the email address.

**Overview window** There is a tool window that allows you to see every user and what documents they've got open in a treeview. To access it use the View->Other Windows->TeamCoding Overview command.

**Roadmap:** Still very much alpha, please report bugs as [GitHub issues](https://github.com/georgeduckett/TeamCoding/issues).
Currently sharing options are a shared folder, Redis, Slack, an SQL Server table or via a server application as a windows service or console application. If you want others (along with any other feature requests) please raise them as issues. Supports Git repos and Team Foundation Services / Visual Studio Online for determining what to share as well as a fall-back to sharing purely based on matching solution filenames. In the pipeline a shared-coding experience (multiple users editing the same document(s)).

**Reporting Bugs**: When reporting bugs ([here](https://github.com/georgeduckett/TeamCoding/issues)) please include information from the `Team Coding` tab in the output window (if relevent). If there were any exceptions they should be visible there, which can help track down the cause.

**Links:** [GitHub Repo](https://github.com/georgeduckett/TeamCoding/)

[![Build status](https://ci.appveyor.com/api/projects/status/vqgmu9893sxn3p7m?svg=true)](https://ci.appveyor.com/project/georgeduckett/teamcoding)

---

![Demo Gif](http://i.giphy.com/3oz8xNb3MTsqzn67za.gif)