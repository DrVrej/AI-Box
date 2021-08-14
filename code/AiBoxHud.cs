using Sandbox.UI;
using Sandbox;

namespace AIBox {
	
	public class AIBoxHud : HudEntity<RootPanel> {
		
		public AIBoxHud() {
			if (IsClient) {
				RootPanel.SetTemplate("/aiboxhud.html");
			}
		}
	}

}
