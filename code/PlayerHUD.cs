using Sandbox.UI;
using Sandbox;

namespace AIBox {

	public class PlayerHUD : HudEntity<RootPanel> {

		public PlayerHUD() {
			if (IsClient) {
				RootPanel.StyleSheet.Load("/PlayerHUD.scss");
				RootPanel.AddChild<NameTags>();
				RootPanel.AddChild<CrosshairCanvas>();
				RootPanel.AddChild<ChatBox>();
				RootPanel.AddChild<VoiceList>();
				RootPanel.AddChild<KillFeed>();
				RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				RootPanel.AddChild<InventoryBar>();
				RootPanel.AddChild<Health>();
			}
		}
	}

}
