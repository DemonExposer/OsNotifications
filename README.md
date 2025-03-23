# Native OS notifications

Calling `Notifications.ShowNotification` will display a notification using your OS's notification manager.

On MacOS, you need to specify the `BundleIdentifier` and this needs to correspond to a defined identifier, otherwise no notification will be shown.
For cross-platform compatibility, it's better to always do this. So, creating a notification will look like this:
```cs
Notifications.BundleIdentifier = "com.apple.finder";
Notifications.ShowNotification("notification-title");
```

# Install
In your .NET project, execute the following command:
```
dotnet add package OsNotifications 
```