using Sandbox.UI;
using Sandbox;

namespace AIBox {

	public class AIBoxHud : HudEntity<RootPanel> {

		public AIBoxHud() {
			if (IsClient) {
				RootPanel.SetTemplate("/aiboxhud.html");
				RootPanel.AddChild<NameTags>();
				RootPanel.AddChild<CrosshairCanvas>();
				RootPanel.AddChild<ChatBox>();
				RootPanel.AddChild<VoiceList>();
				RootPanel.AddChild<KillFeed>();
				RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
			}
		}
	}

}
