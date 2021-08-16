using Sandbox;
using System.Threading.Tasks;

namespace AIBox {
	//[Library("aibox_npc_base", Title = "AIBox NPC Base", Description = "AI Base used to create NPCs.", Icon = "person", Spawnable = true)]
	public partial class NPC : AnimEntity {
		//[ConVar.Replicated]
		//public static bool nav_drawpath { get; set; }

		public enum STATE : int {
			IDLE = 0,
			ALERT = 1,
			FROZEN = 2,
		}
		public STATE State = STATE.IDLE;

		protected float Speed;
		Vector3 InputVelocity;
		Vector3 LookDir;
		public NPCNav Nav;

		private async void InitialSetup() {
			await Task.Delay(100);
			if (this.IsValid()) {
				Log.Info("Owner:" + Owner);
				PlaySound("vj.playerspawn");
			}
		}

		public override void Spawn() {
			base.Spawn();
			Nav = new NPCNav();
			Tags.Add("NPC", "AIBox");

			//SetModel("models/characters/combine_soldier/combine_soldier_new_content.vmdl_c");
			//SetModel("models/citizen/citizen.vmdl");

			InitialSetup();
		}

		[Event.Tick.Server]
		public void Tick() {
			//using var _a = Sandbox.Debug.Profile.Scope("NPC::Tick");

			InputVelocity = 0;

			if (Nav != null) {
				// Wander around
				if (State == STATE.IDLE) {
					Nav.CurNavState = NAV_STATE.WANDER;
				}

				Nav.Tick(Position);

				if (!Nav.IsGoalFinished) {
					InputVelocity = Nav.GoalDir.Normal;
					Velocity = Velocity.AddClamped(InputVelocity * Time.Delta * 500, Speed);
				}

				/*if (nav_drawpath) {
					Nav.DebugDrawPath();
				}*/
			}

			Move(Time.Delta);

			var walkVelocity = Velocity.WithZ(0);
			if (walkVelocity.Length > 0.5f) {
				var turnSpeed = walkVelocity.Length.LerpInverse(0, 100, true);
				var targetRotation = Rotation.LookAt(walkVelocity.Normal, Vector3.Up);
				Rotation = Rotation.Lerp(Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f);
			}

			var animHelper = new CitizenAnimationHelper(this);

			LookDir = Vector3.Lerp(LookDir, InputVelocity.WithZ(0) * 1000, Time.Delta * 100.0f);
			animHelper.WithLookAt(EyePos + LookDir);
			animHelper.WithVelocity(Velocity);
			animHelper.WithWishVelocity(InputVelocity);
		}

		protected virtual void Move(float timeDelta) {
			var bbox = BBox.FromHeightAndRadius(64, 4);
			//DebugOverlay.Box( Position, bbox.Mins, bbox.Maxs, Color.Green );

			MoveHelper move = new(Position, Velocity);
			move.MaxStandableAngle = 50;
			move.Trace = move.Trace.Ignore(this).Size(bbox);

			if (!Velocity.IsNearlyZero(0.001f)) {
				Sandbox.Debug.Draw.Once
									.WithColor(Color.Red)
									.IgnoreDepth()
									.Arrow(Position, Position + Velocity * 2, Vector3.Up, 2.0f);

				//using (Sandbox.Debug.Profile.Scope("TryUnstuck"))
				move.TryUnstuck();

				//using (Sandbox.Debug.Profile.Scope("TryMoveWithStep"))
				move.TryMoveWithStep(timeDelta, 30);
			}

			//using (Sandbox.Debug.Profile.Scope("Ground Checks")) {
			var tr = move.TraceDirection(Vector3.Down * 10.0f);

			if (move.IsFloor(tr)) {
				GroundEntity = tr.Entity;

				if (!tr.StartedSolid) {
					move.Position = tr.EndPos;
				}

				if (InputVelocity.Length > 0) {
					var movement = move.Velocity.Dot(InputVelocity.Normal);
					move.Velocity -= movement * InputVelocity.Normal;
					move.ApplyFriction(tr.Surface.Friction * 10.0f, timeDelta);
					move.Velocity += movement * InputVelocity.Normal;

				} else {
					move.ApplyFriction(tr.Surface.Friction * 10.0f, timeDelta);
				}
			} else {
				GroundEntity = null;
				move.Velocity += Vector3.Down * 900 * timeDelta;
				Sandbox.Debug.Draw.Once.WithColor(Color.Red).Circle(Position, Vector3.Up, 10.0f);
			}
			//}

			Position = move.Position;
			Velocity = move.Velocity;
		}
	}
}