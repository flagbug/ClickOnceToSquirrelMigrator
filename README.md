# Overview

This library is a helper for the migration from ClickOnce to [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)

The ClickOnce uninstall code is taken from [Wunder.ClickOnceUninstaller](https://github.com/6wunderkinder/Wunder.ClickOnceUninstaller)

# How to

The migration is super-simple, requiring only one method call in the ClickOnce version of the application and one method call in the Squirrel version of the application.

```cs
var updateManager = new UpdateManager("http://update.myapp.com", "MyApp", FrameworkVersion.Net45);
var migrator = new ClickOnceToSquirrelMigrator(updateManager, "ClickOnceAppName");

// Ship a new ClickOnce update and call this method
// It silently installs the Squirrel version of your application in the background
// and removes the ClickOnce shortcut so the user doesn't end up with two shortcuts.
await migrator.InstallSquirrel();

// In the Squirrel version call this method
// It uninstalls the ClickOnce version of the application
await migrator.UninstallClickOnce();
```