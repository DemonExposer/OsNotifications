using Microsoft.Toolkit.Uwp.Notifications;

namespace WindowsNotification;

public class WindowsNotification {
	public static void ShowNotification(string title, string message, bool playCustomSound, Uri? audioSource) {
		ToastContentBuilder toastContentBuilder = new ToastContentBuilder()
			.AddText(title)
			.AddText(message)
			.AddAudio(audioSource);

		if (playCustomSound) {
			if (audioSource == null)
				toastContentBuilder.AddAudio(new ToastAudio { Silent = true });
			else
				toastContentBuilder.AddAudio(audioSource);
		}

		toastContentBuilder.Show();
	}
}
