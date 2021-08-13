using Sandbox;
using Sandbox.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.Tools {
	[Library("tool_wander")]
	public class Wander : Base {
		public override void OnClick(TraceResult tr, IList<Entity> selected) {
			if (!Host.IsServer) return;

			foreach (var ent in selected) {
				if (ent is AIBoxNPC npc) {
					var wander = new AIBoxNavSteer();
					wander.MinRadius = 500;
					wander.MaxRadius = 2000;
					npc.Steer = wander;

					if (!wander.FindNewTarget(npc.Position)) {
						DebugOverlay.Text(npc.EyePos, "COULDN'T FIND A WANDERING POSITION!", 5.0f);
					}
				}
			}
		}
	}
}
