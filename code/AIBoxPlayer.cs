using Sandbox;
using System.Collections.Generic;

namespace AIBox {
	partial class AIBoxPlayer : Player {
		public Dictionary<string, string> soundTbl = new Dictionary<string, string>(){
			{"spawn", "sounds/diagnostics/beep.vsnd"},
		};

		public override void Respawn() {
			SetModel("models/citizen/citizen.vmdl");
			Controller = new WalkController(); // Use WalkController for movement (you can make your own PlayerController for 100% control)
			Animator = new StandardPlayerAnimator(); // Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
			Camera = new FirstPersonCamera();
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			base.Respawn();
			Log.Info("Player has spawned!");
			PlaySound("vj.playerspawn");
		}

		// Called every tick, clientside and serverside.
		public override void Simulate(Client cl) {
			//Log.Info(base.Health);
			base.Simulate(cl);
			SimulateActiveChild(cl, ActiveChild); // If you have active children (like a weapon etc) you should call this to simulate those too.

			// If we're running serverside and Attack1 was just pressed, spawn something
			if (IsServer && Input.Pressed(InputButton.Attack1)) {
				var npc = new AIBoxNPC {
					Position = EyePos + EyeRot.Forward * 200,
					Rotation = Rotation.LookAt(EyeRot.Backward.WithZ(0))
				};
				/*var ragdoll = new ModelEntity();
				ragdoll.SetModel("models/citizen/citizen.vmdl");
				ragdoll.Position = EyePos + EyeRot.Forward * 40;
				ragdoll.Rotation = Rotation.LookAt( Vector3.Random.Normal );
				ragdoll.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
				ragdoll.PhysicsGroup.Velocity = EyeRot.Forward * 1000;*/
			}
		}

		public override void OnKilled() {
			PlaySound("vj.playerhit");
			base.OnKilled();
			EnableDrawing = false; // Don't draw the player model when dead
		}
	}
}