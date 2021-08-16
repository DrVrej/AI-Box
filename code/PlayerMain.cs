using Sandbox;
using System.Collections.Generic;

namespace AIBox {
	partial class PlayerMain : Player {
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
			new ModelEntity("models/citizen_clothes/trousers/trousers.lab.vmdl", this) {
				EnableHideInFirstPerson = true,
			};
			new ModelEntity("models/citizen_clothes/jacket/labcoat.vmdl", this) {
				EnableHideInFirstPerson = true,
			};
			new ModelEntity("models/citizen_clothes/shoes/smartshoes/smartshoes.vmdl", this) {
				EnableHideInFirstPerson = true,
			};
			new ModelEntity("models/citizen_clothes/gloves/gloves_workgloves.vmdl", this) {
				EnableHideInFirstPerson = true,
			};
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

			if (IsServer) {
				// Handle Attack1
				if (Input.Pressed(InputButton.Attack1)) {
					if (Health <= 0) {
						Respawn();
					}
				}

				// Handle Attack2
				if (Input.Pressed(InputButton.Slot1)) {
					if (LifeState == LifeState.Alive) {
						var tr = Trace.Ray(EyePos, EyePos + EyeRot.Forward * 1000)
							.Ignore(this)
							.Size(10)
							.Run();
						new NPC_Citizen() {
							Position = tr.EndPos,
							Rotation = Rotation.LookAt(EyeRot.Backward.WithZ(0)),
							Owner = this,
						};
					}
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