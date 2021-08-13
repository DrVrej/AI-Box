using Sandbox;
using Sandbox.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.Tools {
	[Library("tool_seek")]
	public class Seek : Base {
		public override void OnClick(TraceResult tr, IList<Entity> selected) {
			if (!Host.IsServer) return;

			foreach (var ent in selected) {
				if (ent is AIBoxNPC npc) {
					npc.Steer = new AIBoxNavSteer();
					npc.Steer.Target = tr.EndPos;
				}
			}
		}
	}
}
