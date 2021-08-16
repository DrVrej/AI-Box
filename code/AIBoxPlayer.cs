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
			Inventory = new Inventory(this);
			if (Camera == null) { Camera = new FirstPersonCamera(); }
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			var clothing1 = new ModelEntity("models/citizen_clothes/trousers/trousers.lab.vmdl", this);
			clothing1.EnableHideInFirstPerson = true;
			var clothing2 = new ModelEntity("models/citizen_clothes/jacket/labcoat.vmdl", this);
			clothing2.EnableHideInFirstPerson = true;
			var clothing3 = new ModelEntity("models/citizen_clothes/shoes/smartshoes/smartshoes.vmdl", this);
			clothing3.EnableHideInFirstPerson = true;
			var clothing4 = new ModelEntity("models/citizen_clothes/gloves/gloves_workgloves.vmdl", this);
			clothing4.EnableHideInFirstPerson = true;
			base.Respawn();

			Inventory.Add(new PhysGun(), true);
			//Scale = Rand.Float(0.6f, 1.8f);
			Log.Info("Player has spawned!");
			PlaySound("vj.playerspawn");
		}

		// Called every tick, clientside and serverside.
		public override void Simulate(Client cl) {
			//Log.Info(base.Health);
			base.Simulate(cl);
			SimulateActiveChild(cl, ActiveChild); // If you have active children (like a weapon etc) you should call this to simulate those too.

			// Handle Attack1
			if (IsServer && Input.Pressed(InputButton.Attack1)) {
				if (Health <= 0) {
					Respawn();
				}
			}

			// Handle Attack2
			if (IsServer && Input.Pressed(InputButton.Attack2)) {
				if (LifeState == LifeState.Alive) {
					var tr = Trace.Ray(EyePos, EyePos + EyeRot.Forward * 1000)
						.Ignore(this)
						.Size(20)
						.Run();
					new AIBoxNPCDefault() {
						Position = tr.EndPos,
						Rotation = Rotation.LookAt(EyeRot.Backward.WithZ(0)),
						Owner = this,
					};
				}
			}

			// Thirdperson
			if (Input.Pressed(InputButton.View)) {
				if (Camera is FirstPersonCamera) {
					Camera = new ThirdPersonCamera();
				} else {
					Camera = new FirstPersonCamera();
				}
			}
		}

		public override void OnKilled() {
			PlaySound("vj.playerhit");

			base.OnKilled();

			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;  // Don't draw the player model when dead

			Inventory.DropActive();
			Inventory.DeleteContents();
		}
	}
}