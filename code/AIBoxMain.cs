
using Sandbox;
using System.Linq;

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

		// Remove AIBox NPCs created by me
		[ServerCmd("aibox_npc_clear")]
		public static void AIBox_NPC_Clear() {
			foreach (var npc in Entity.All.OfType<AIBoxNPC>().ToArray()) {
				if ((npc.Owner != null) && (ConsoleSystem.Caller == npc.Owner.GetClientOwner())) {
					npc.Delete();
				}
			}
		}

		// Remove all AIBox NPCs
		[ServerCmd("aibox_npc_clear_all")]
		public static void AIBox_NPC_Clear_All() {
			foreach (var npc in Entity.All.OfType<AIBoxNPC>().ToArray()) {
				npc.Delete();
			}
		}
	}
}