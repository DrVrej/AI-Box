using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIBox {
	public enum AI_STATE {
		IDLE,
		ALERT,
		FROZEN,
	}
	public enum AI_BEHAVIOR {
		PASSIVE,
		AGGRESSIVE,
		NEUTRAL,
	}
	public enum AI_DISPOSITION {
		INVALID,
		FRIENDLY,
		NEUTRAL,
		HOSTILE,
	}

	//[Library("aibox_npc_base", Title = "AIBox NPC Base", Description = "AI Base used to create NPCs.", Icon = "person", Spawnable = true)]
	public partial class NPC : AnimEntity {
		public AI_STATE State = AI_STATE.IDLE;
		protected float Speed;
		protected Vector3 InputVelocity;
		protected Vector3 LookDir;
		public readonly NPCNav Nav;

		public float SightDistance = 1000.0f; // If the enemy farther than this it will be ignored.
		public AI_BEHAVIOR Behavior = AI_BEHAVIOR.AGGRESSIVE; // TODO: Not implemented
		public List<string> RelationClasses = new();
		public struct AIRelation {
			public Entity Ent;
			public AI_DISPOSITION Disposition;
			public int Priority; // TODO: Not implemented
			public AI_DISPOSITION ForcedDisposition; // TODO: Not implemented, supposed to overtake Disposition !
													 // EX: Player-Friendly NPC turns on a specific player OR a specific player/NPC is set as friendly to this NPC
			public AIRelation(Entity Ent, AI_DISPOSITION Disposition, int Priority = 0) {
				this.Ent = Ent;
				this.Disposition = Disposition;
				this.Priority = Priority;
				this.ForcedDisposition = AI_DISPOSITION.INVALID;
			}
		}
		public Dictionary<Entity, AIRelation> Relations = new();

		public struct AIEnemy {
			public Entity Ent; // The actual enemy entity
			public float Distance; // TODO: Calculated every tick, distance from me to the enemy
			public AIEnemy(Entity Ent, float Distance = 0.0f) {
				this.Ent = Ent;
				this.Distance = Distance;
			}
		}
		public AIEnemy Enemy = new();

		public float NextRelationsUpdate = 0.0f;
		public float NextRelationsCleanupUpdate = 0.0f;

		public NPC() {
			Nav = new NPCNav(this);
		}

		public async override void Spawn() {
			base.Spawn();
			Tags.Add("npc", "aibox");
			await Task.DelaySeconds(0.1f);
			if (this.IsValid()) {
				PlaySound("vj.playerspawn");
			}
		}

		[Event.Tick.Server]
		public virtual void Tick() {
			InputVelocity = 0;

			if (Nav != null) {
				// Wander around
				if (State == AI_STATE.IDLE) {
					Nav.CurNavState = NAV_STATE.WANDER;
				} else if (State == AI_STATE.ALERT) {
					Nav.CurNavState = NAV_STATE.CHASE;
				}

				Nav.Tick(Position);

				if (!Nav.Goal.IsGoalFinished()) {
					InputVelocity = Nav.Goal.Direction.Normal;
					Velocity = Velocity.AddClamped(InputVelocity * Time.Delta * 500, Speed);
				}
			}

			Move(Time.Delta);

			var walkVelocity = Velocity.WithZ(0);
			if (walkVelocity.Length > 0.5f) {
				var turnSpeed = walkVelocity.Length.LerpInverse(0, 100, true);
				var targetRotation = Rotation.LookAt(walkVelocity.Normal, Vector3.Up);
				Rotation = Rotation.Lerp(Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f);
			}
			LookDir = Vector3.Lerp(LookDir, InputVelocity.WithZ(0) * 1000, Time.Delta * 100.0f);

			// Update Relations
			if (NextRelationsUpdate <= Time.Now) {
				UpdateRelations();
				NextRelationsUpdate = Time.Now + 1.0f;
			}

			// Handle Relation for Enemy
			if (Enemy.Ent.IsValid()) {
				Enemy.Distance = Position.Distance(Enemy.Ent.Position);
				State = AI_STATE.ALERT;
			} else {
				State = AI_STATE.IDLE;
			}
			//Log.Info("Current Enemy: " + Enemy.Ent + " | Distance: " + Enemy.Distance + " | AI State: " + State);

			// Occasionally clean up the Relations list by removing deleted entities
			if (NextRelationsCleanupUpdate <= Time.Now) {
				CleanupRelations();
				NextRelationsCleanupUpdate = Time.Now + 5.0f;
			}

			// Debug the NPC's EyePos and see its forward direction
			//var draw = Sandbox.Debug.Draw.ForSeconds(0.5f);
			//draw.WithColor(Color.Red).Circle(EyePos, Vector3.Up, 5.0f);
			//draw.WithColor(Color.Magenta).Arrow(EyePos, EyePos + LocalRotation.Forward * 200.0f, Vector3.Up, 5.0f);
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

		/// <summary>
		/// Updates the Relations list and updates the enemy entity.
		/// </summary>
		public void UpdateRelations() {
			Entity closest = null;
			foreach (var ent in Entity.All.ToArray()) {
				// Check only NPCs or Players!
				if (ent != this && (ent is NPC or SandboxPlayer) && (ent.LifeState == LifeState.Alive)) {
					bool isFri = false;
					NPC npc = ent as NPC;
					SandboxPlayer ply = ent as SandboxPlayer;
					var entClasses = ent.GetRelationClasses();
					//Log.Info(npc + " " + npc.IsValid() + " " + ply + " " + ply.IsValid());
					bool classMatch = RelationClasses.Select(x => x).Intersect(entClasses).Any();
					if (classMatch) {
						isFri = true;
						if (Relations.ContainsKey(ent)) {
							AIRelation rel = Relations[ent];
							rel.Disposition = AI_DISPOSITION.FRIENDLY;
							Relations[ent] = rel;
						} else {
							Relations[ent] = new AIRelation(ent, AI_DISPOSITION.FRIENDLY);
						}
					}
					// This ent is NOT a friend!
					if (!isFri) {
						// Set my disposition to it as enemy!
						if (Relations.ContainsKey(ent)) {
							AIRelation rel = Relations[ent];
							rel.Disposition = AI_DISPOSITION.HOSTILE;
							Relations[ent] = rel;
						} else {
							Relations[ent] = new AIRelation(ent, AI_DISPOSITION.HOSTILE);
						}
						var distToEnt = Position.Distance(ent.Position);
						//Log.Info(distToEnt + " / " + SightDistance + "   | " + ent.Position + " | " + Position);
						if (distToEnt <= SightDistance) {
							if (closest == null || Position.Distance(closest.Position) > distToEnt) {
								closest = ent;
							}
						}
					}
				}
			}
			if (closest.IsValid()) {
				Enemy = new AIEnemy(closest);
			} else {
				// TODO: Reset enemy?
				Enemy = new AIEnemy(null);
			}
		}

		/// <summary>
		/// Updates the Relations list by removing deleted entities.
		/// </summary>
		public void CleanupRelations() {
			foreach (var ent in Relations.Keys.ToArray()) {
				if (!ent.IsValid()) {
					Relations.Remove(ent);
				}
			}
		}
	}
}