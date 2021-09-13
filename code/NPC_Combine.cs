using Sandbox;
using System.Threading.Tasks;

namespace AIBox {
	[Library("aibox_npc_combine", Title = "Combine Soldier", Description = "Combine Soldier NPC.", Icon = "person", Spawnable = true)]
	public partial class NPC_Combine : NPC {

		/* Notes:
			* Animgraph:
				* Only has 1 hold type: Rifle
				* Unused parameters: e_combineclass, e_weapon, e_specific_activities, e_danger_type
		*/
		public override void Spawn() {
			base.Spawn();

			SetModel("models/characters/combine_soldier/combine_soldier_new_content.vmdl_c");
			EyePos = Position + Vector3.Up * 64;
			CollisionGroup = CollisionGroup.Player;
			SetupPhysicsFromCapsule(PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius(72, 10));

			EnableHitboxes = true;

			SetMaterialGroup(Rand.Int(0, 4));

			Health = 100;
			Speed = 100;
			RelationClasses.Add("combine");
			//RelationClasses.Add("player");
			UseAnimGraph = true;
		}

		protected override void OnAnimGraphTag(string tag, AnimGraphTagEvent fireMode) {
			Log.Info(tag);
		}

		public override void Tick() {
			base.Tick();

			//SetAnimLookAt("bLookAt", EyePos + LookDir);

			if (Enemy.Ent.IsValid()) {
				SetAnimBool("b_combat", true);
			} else {
				SetAnimBool("b_combat", false);
			}

			if (Nav != null) {
				if (!Nav.Goal.IsGoalFinished()) {
					//SetAnimBool("b_combat", false);
				}
			}
		}
	}
}