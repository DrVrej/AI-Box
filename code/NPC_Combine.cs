using Sandbox;
using System.Threading.Tasks;

namespace AIBox {
	[Library("aibox_npc_combine", Title = "Combine Soldier", Description = "Combine Soldier NPC.", Icon = "person", Spawnable = true)]
	public partial class NPC_Combine : NPC {

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
			UseAnimGraph = true;
		}

		public override void Tick() {
			base.Tick();
			SetAnimInt("holdtype", 2);

			//SetAnimInt("e_weapon", 1);
			SetAnimBool("b_combat", true);
			SetAnimBool("b_aim", true);
			SetAnimBool("bLookAt", true);
			//SetAnimLookAt("bLookAt", EyePos + LookDir);
			var animHelper = new CitizenAnimationHelper(this);
			animHelper.WithLookAt(EyePos + LookDir);
			animHelper.WithVelocity(Velocity);
			animHelper.WithWishVelocity(InputVelocity);
		}
	}
}