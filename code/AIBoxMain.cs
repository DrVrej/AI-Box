
using Sandbox;

namespace AIBox {
	public partial class AIBoxMain : Sandbox.Game {
		// Constructor
		public AIBoxMain() {
			if (IsServer) {
				Log.Info("Gamemode Has Created Serverside!");
			}
			else if (IsClient) {
				Log.Info("Gamemode Has Created Clientside!");
			}
		}

		public override void ClientJoined(Client client) {
			base.ClientJoined(client);

			var player = new AIBoxPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}

}