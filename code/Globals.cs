using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace AIBox {
	public static class Globals {
		public static List<string> GetRelationClasses(this Entity ent) {
			if (ent is SandboxPlayer p) {
				return p.RelationClasses;
			} else if (ent is AIBox.NPC n) {
				return n.RelationClasses;
			} else {
				return null;
			}
		}

		// Remove AIBox NPCs created by me
		[ServerCmd("aibox_npc_clear")]
		public static void AIBox_NPC_Clear() {
			short i = 0;
			foreach (var npc in Entity.All.OfType<NPC>()) {
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
			foreach (var npc in Entity.All.OfType<NPC>()) {
				i++;
				npc.Delete();
			}
			Log.Info("Removed " + i + " NPC(s)!");
		}
	}
}