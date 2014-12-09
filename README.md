# Overview

This library is a helper for the migration from ClickOnce to [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)

# The Plan

The migration should be super-simple, requiring only one method call in the ClickOnce version of the application and one method call in the Squirrel version of the application.

The codez should look something like this:
```cs
var updateManager = new UpdateManager("http://update.myapp.com", "MyApp", FrameworkVersion.Net45);
var migrator = new ClickOnceToSquirrelMigrator(updateManager, "ClickOnceAppName");

// Ship a new ClickOnce update and call this method
// It will install the Squirrel version of your application silently in the background
// and remove the ClickOnce shortcut
migrator.FirstStep();

// In the Squirrel version call this method
// It will uninstall the ClickOnce version of the application
migrator.SecondStep();
```