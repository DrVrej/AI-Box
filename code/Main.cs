
using Sandbox;

namespace VJ {
	public partial class VJMod : Sandbox.Game {
		// Constructor
		public VJMod() {
			if (IsServer) {
				Log.Info("Gamemode Has Created Serverside!");
			}
			else if (IsClient) {
				Log.Info("Gamemode Has Created Clientside!");
			}
		}

		public override void ClientJoined(Client client) {
			base.ClientJoined(client);

			var player = new VJPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}

}