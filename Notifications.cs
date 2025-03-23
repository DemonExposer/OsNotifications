using System.Runtime.InteropServices;

namespace OsNotifications;

public partial class Notifications {
	public static string? BundleIdentifier = null;
	
	[LibraryImport("macNotification.dylib")]
	private static partial void showNotification([MarshalAs(UnmanagedType.LPStr)] string identifier, [MarshalAs(UnmanagedType.LPStr)] string title, [MarshalAs(UnmanagedType.LPStr)] string subtitle, [MarshalAs(UnmanagedType.LPStr)] string informativeText);
	
	public static void ShowNotification(string title, string message = "", string informativeText = "") {
		if (BundleIdentifier == null)
			throw new InvalidOperationException("BundleIdentifier must be set on MacOS before calling ShowNotification. If it is set to a non-existing identifier, no notification will be shown!");
		
		showNotification(BundleIdentifier, title, message, informativeText);
	}
}