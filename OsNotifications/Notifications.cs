using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OsNotifications;

public partial class Notifications {
	public static string? BundleIdentifier = null;
	
	[LibraryImport("macNotification.dylib")]
	private static partial void showNotification([MarshalAs(UnmanagedType.LPStr)] string identifier, [MarshalAs(UnmanagedType.LPStr)] string title, [MarshalAs(UnmanagedType.LPStr)] string subtitle, [MarshalAs(UnmanagedType.LPStr)] string informativeText);
	
	public static void ShowNotification(string title, string message = "", string informativeText = "") {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			ShowNotificationLinux(title, message);
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			ShowNotificationMac(title, message, informativeText);
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			ShowNotificationWindows(title, message);
		else
			throw new PlatformNotSupportedException("Notifications are only supported on Linux, MacOS and Windows");
	}

	private static void ShowNotificationLinux(string title, string message) => Process.Start("notify-send", $"\"{title}\" \"{message}\"").WaitForExit();

	private static void ShowNotificationMac(string title, string message, string informativeText) {
		if (BundleIdentifier == null)
			throw new InvalidOperationException("BundleIdentifier must be set on MacOS before calling ShowNotification. If it is set to a non-existing identifier, no notification will be shown!");
		
		showNotification(BundleIdentifier, title, message, informativeText);
	}

	private static void ShowNotificationWindows(string title, string message) {
		string dllPath = Path.Combine(AppContext.BaseDirectory, "WindowsNotification.dll");
		Assembly assembly = Assembly.LoadFrom(dllPath);
		Type? windowsNotificationClass = assembly.GetType("WindowsNotification.WindowsNotification");
		MethodInfo? showNotificationMethod = windowsNotificationClass?.GetMethod("ShowNotification");

		object? instance = Activator.CreateInstance(windowsNotificationClass!);
		showNotificationMethod?.Invoke(instance, [title, message]);
	}
}