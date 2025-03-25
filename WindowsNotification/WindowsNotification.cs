using Microsoft.Toolkit.Uwp.Notifications;

namespace WindowsNotification;

public class WindowsNotification {
	public static void ShowNotification(string title, string message) {
		new ToastContentBuilder()
			.AddText(title)
			.AddText(message)
			.Show();
	}
}
