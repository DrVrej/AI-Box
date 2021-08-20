using Sandbox;
using System.Collections.Generic;

namespace AIBox {
	public static class Globals {
		public static List<string> GetRelationClasses(this Entity ent) {
			if (ent is AIBox.PlayerMain p) {
				return p.RelationClasses;
			} else if (ent is AIBox.NPC n) {
				return n.RelationClasses;
			} else {
				return null;
			}
		}
	}
}