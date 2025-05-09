﻿using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OsNotifications;

public partial class Notifications {
	public static string BundleIdentifier = "";
	public static Uri? WindowsAudioSource {
		get => _windowsAudioSource;
		set {
			_windowsAudioSource = value;
			_playDefaultWindowsSound = false;
		}
	}

	private static Uri? _windowsAudioSource = null;
	private static bool _isApplicationTypeSpecified;
	private static bool _playDefaultWindowsSound = true;

	public static void ResetWindowsAudioSource() => _playDefaultWindowsSound = true;

	[LibraryImport("macNotification.dylib")]
	private static partial void setGuiApplication(sbyte isGuiValue);

	public static void SetGuiApplication(bool isGuiValue) {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return;
		
		setGuiApplication(isGuiValue ? (sbyte) 1 : (sbyte) 0);
		_isApplicationTypeSpecified = true;
	}

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

	private static void ShowNotificationLinux(string title, string message) {
		try {
			Process.Start("notify-send", $"\"{title}\" \"{message}\"").WaitForExit();
		} catch (Win32Exception) {
			throw new PlatformNotSupportedException("Notifications are not supported on this Linux distro");
		}
	}

	private static void ShowNotificationMac(string title, string message, string informativeText) {
		if (!_isApplicationTypeSpecified)
			throw new InvalidOperationException("SetGuiApplication must be called before calling ShowNotification. If SetGuiApplication is called with false in a GUI application, this method WILL HANG!");
		
		showNotification(BundleIdentifier, title, message, informativeText);
	}

	private static void ShowNotificationWindows(string title, string message) {
		const string winNotifDll = "WindowsNotification.dll";
		string dllPath = Path.Combine(AppContext.BaseDirectory, winNotifDll);

		string nativePath = Path.Combine(AppContext.BaseDirectory, "runtimes", "win-x64", "native");
		Environment.SetEnvironmentVariable("PATH", nativePath + ";" + Environment.GetEnvironmentVariable("PATH"));

		if (!File.Exists(dllPath))
			dllPath = Path.Combine(nativePath, winNotifDll);

		// In case PublishSingleFile is set to true, load the library from the executable itself (this is the case if dllPath does not exist).
		Assembly assembly = File.Exists(dllPath) ? Assembly.LoadFrom(dllPath) : Assembly.Load(winNotifDll[..^4]);
		Type? windowsNotificationClass = assembly.GetType("WindowsNotification.WindowsNotification");
		MethodInfo? showNotificationMethod = windowsNotificationClass?.GetMethod("ShowNotification");

		object? instance = Activator.CreateInstance(windowsNotificationClass!);
		showNotificationMethod?.Invoke(instance, [title, message, !_playDefaultWindowsSound, _windowsAudioSource]);
	}
}
