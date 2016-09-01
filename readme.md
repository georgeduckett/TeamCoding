Team Coding
--

**About:** The Team Coding Visual Studio extension helps discover team members who are working in the same area of a Git repository as you. You can set your own username and user image url via settings (Tools->Options->Team Coding), or let it try and figure them out automatically (see below). Once working it will display user icons at the top of your tabs when other users have the same document open, with a white border when the tab is selected and a grey border when it's edited. It will also display text in CodeLens (disable in CodeLens settings) indicating whether someone is currently working on a class/member based on where their caret (text cursor) is as well as showing a coloured caret where they are working for C# and Visual Basic code.

**Quickstart:** There are 2 ways Team Coding can be minimally configured.
In the options menu (under Team Coding) where you can also set user settings and/or
by opening a repository with a `teamcoding.json` file in it (shared settings will be taken from there).
An example of a valid `teamcoding.json` file can be found in the options menu.

**Settings:** Also in the Team Coding settings page is configuration options for how to share open document information with others. Multiple methods can be used at once, you can also add a `teamcoding.json` file anywhere within the solution folder for it to override user settings. If this file is in place then for that solution settings don't need to be set in each developer's IDE.

**User Identity:** It tries to get your email address as a user identity from various sources (saved Windows Credentials for GitHub, your logged in identity from Visual Studio, your machine name). This will be made public to your team. It uses Gravatar to get a user image from the email address.

**Roadmap:** Still very much alpha, please report bugs as [GitHub issues](https://github.com/georgeduckett/TeamCoding/issues). Currently sharing options are a shared folder, and Redis. If you want others (along with any other feature requests) please raise them as issues. Supports Git repos and Team Foundation Services / Visual Studio Online for determining what to share. In the pipeline a shared-coding experience (multiple users editing the same document(s)).

**Reporting Bugs**: When reporting bugs ([here](https://github.com/georgeduckett/TeamCoding/issues)) please include information from the `Team Coding` tab in the output window (if relevent). If there were any exceptions they should be visible there, which can help track down the cause.

**Links:** [GitHub Repo](https://github.com/georgeduckett/TeamCoding/)

[![Build status](https://ci.appveyor.com/api/projects/status/vqgmu9893sxn3p7m?svg=true)](https://ci.appveyor.com/project/georgeduckett/teamcoding)