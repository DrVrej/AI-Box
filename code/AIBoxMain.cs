
using Sandbox;
using System.Linq;

namespace AIBox {
	public partial class AIBoxMain : Game {
		// Constructor
		public AIBoxMain() {
			if (IsServer) {
				Log.Info("Gamemode Has Created Serverside!");
				new AIBoxHud();
			} else if (IsClient) {
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
			short i = 0;
			foreach (var npc in Entity.All.OfType<AIBoxNPC>()) {
				if ((npc.Owner != null) && (ConsoleSystem.Caller == npc.Owner.GetClientOwner())) {
					i++;
					npc.Delete();
				}
			}
			Log.Info("Removed " + i + " NPC(s)!");
		}

		// Remove all AIBox NPCs
		[ServerCmd("aibox_npc_clear_all")]
		public static void AIBox_NPC_Clear_All() {
			short i = 0;
			foreach (var npc in Entity.All.OfType<AIBoxNPC>()) {
				i++;
				npc.Delete();
			}
			Log.Info("Removed " + i + " NPC(s)!");
		}
	}
}