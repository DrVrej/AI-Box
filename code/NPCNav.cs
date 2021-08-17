using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace AIBox {
	public enum NAV_STATE {
		NONE,
		WANDER,
		CHASE,
	}

	public class NPCNav {
		public NAV_STATE CurNavState = NAV_STATE.NONE; // The current navigation state (Usually set by the NPC)

		public struct NavGoal {
			public Vector3 Position;
			public Vector3 PrevPosition;
			public List<Vector3> Points; // List of Points the NPC is navigating to right now
			public Vector3 Direction; // Current goal's direction (Useful for turning & facing)
			public bool IsGoalFinished() { return Points.Count <= 1; }
		}
		public NavGoal Goal = new NavGoal();

		public struct NavWander {
			public float MinRadius; // Minimum wandering distance
			public float MaxRadius; // Maximum wandering distance
			public float NextTime; // Time until it can wander again
			public NavWander(float min, float max) {
				MinRadius = min;
				MaxRadius = max;
				NextTime = 0.0f;
			}
		}
		public NavWander Wander = new NavWander(200.0f, 500.0f);

		// Constructor
		public NPCNav() {
			Goal.Points = new List<Vector3>();
		}

		public virtual void Tick(Vector3 currentPosition) {
			/*Log.Info("Current Goal: " + Goal.Points.Count + " || " + Goal.Position + " || Finished? " + Goal.IsGoalFinished());
			for (int i = 0; i < Goal.Points.Count; i++) {
				Log.Info($"      Goal.Points[{i}]: {Goal.Points[i].ToString()}");
			}*/

			if (Goal.Position != null) {
				UpdateGoal(currentPosition, Goal.Position);
			}

			if (Goal.IsGoalFinished()) {
				Goal.Direction = Vector3.Zero; // Reset the Direction since the goal is finished

				if (CurNavState == NAV_STATE.WANDER) {
					if (Wander.NextTime <= Time.Now) {
						FindNewTarget_Wander(currentPosition);
						Wander.NextTime = Time.Now + Rand.Float(3.0f, 10.0f);
					}
				} else if (CurNavState == NAV_STATE.CHASE) {
					FindNewTarget_Chase(currentPosition);
				} else { // No state, then just return
					return;
				}
			}

			Goal.Direction = GetDirection(currentPosition);

			var avoid = GetAvoidance(currentPosition, 300);
			if (!avoid.IsNearlyZero()) {
				Goal.Direction = (Goal.Direction + avoid).Normal;
			}

			DebugDraw(0.1f, 0.1f);
		}

		public virtual bool FindNewTarget_Wander(Vector3 center) {
			var t = NavMesh.GetPointWithinRadius(center, Wander.MinRadius, Wander.MaxRadius);
			if (t.HasValue) {
				Goal.Position = t.Value;
			}

			return t.HasValue;
		}

		public virtual bool FindNewTarget_Chase(Vector3 center) {
			Entity closest = null;
			foreach (var ply in Entity.All.OfType<Player>().ToArray()) {
				if (ply.LifeState != LifeState.Alive) continue;
				if (closest == null || center.Distance(closest.Position) > center.Distance(ply.Position)) {
					closest = ply;
				}
			}
			if (closest == null) {
				//Goal.Position = null;
				return false;
			} else {
				Goal.Position = closest.Position;
				return true;
			}
		}

		public void UpdateGoal(Vector3 from, Vector3 to) {
			// Only rebuild the path if the position has changed greatly!
			if (!Goal.PrevPosition.IsNearlyEqual(to, 5)) {
				var fromFixed = NavMesh.GetClosestPoint(from);
				var toFixed = NavMesh.GetClosestPoint(to);
				Goal.Points.Clear(); // Remove all points
				NavMesh.GetClosestPoint(from);
				NavMesh.BuildPath(fromFixed.Value, toFixed.Value, Goal.Points);
				//Goal.Points.Add( NavMesh.GetClosestPoint( to ) );
			}
			if (Goal.Points.Count <= 1) { return; }
			Goal.PrevPosition = to;

			//var deltaToCurrent = from - Goal.Points[0]; // Unused
			var deltaToNext = from - Goal.Points[1];
			var delta = Goal.Points[1] - Goal.Points[0];
			var deltaNormal = delta.Normal;

			// I reached the end of the current Point, so remove it!
			if (deltaToNext.WithZ(0).Length < 20) {
				Goal.Points.RemoveAt(0);
				return;
			}

			// If we're in front of this line then remove it and move on to next one
			if (deltaToNext.Normal.Dot(deltaNormal) >= 1.0f) {
				Goal.Points.RemoveAt(0);
			}
		}

		public Vector3 GetDirection(Vector3 position) {
			if (Goal.Points.Count == 1) {
				return (Goal.Points[0] - position).WithZ(0).Normal;
			} else if (Goal.Points.Count == 0) {
				return position;
			}
			return (Goal.Points[1] - position).WithZ(0).Normal;
		}

		public Vector3 GetAvoidance(Vector3 position, float radius) {
			var center = position + Goal.Direction * radius * 0.5f;
			var objectRadius = 200.0f;
			Vector3 avoidance = default;
			var avoidProps = Goal.Points.Count > 2; // Only avoid props if it's not the last point
			foreach (var ent in Physics.GetEntitiesInSphere(center, radius)) {
				if (ent.IsWorld) continue;
				if ((ent is NPC or Player) || (avoidProps && ent is Prop)) {
					var delta = (position - ent.Position).WithZ(0);
					var closeness = delta.Length;
					if (closeness < 0.001f) continue;
					var thrust = ((objectRadius - closeness) / objectRadius).Clamp(0, 1);
					if (thrust <= 0) continue;
					//avoidance += delta.Cross( Goal.Direction ).Normal * thrust * 2.5f;
					avoidance += delta.Normal * thrust * thrust;
					//Log.Info(ent + "   " + avoidance.Length);
				}
			}

			return avoidance;
		}

		public float Distance(int point, Vector3 from) {
			if (Goal.Points.Count <= point) return float.MaxValue;
			return Goal.Points[point].WithZ(from.z).Distance(from);
		}

		public void DebugDraw(float time, float opacity = 1.0f) {
			var draw = Sandbox.Debug.Draw.ForSeconds(time);
			var lift = Vector3.Up * 2;
			draw.WithColor(Color.White.WithAlpha(opacity)).Circle(lift + Goal.Position, Vector3.Up, 20.0f);

			int i = 0;
			var lastPoint = Vector3.Zero;
			foreach (var point in Goal.Points) {
				if (i > 0) {
					draw.WithColor(i == 1 ? Color.Green.WithAlpha(opacity) : Color.Cyan.WithAlpha(opacity)).Arrow(lastPoint + lift, point + lift, Vector3.Up, 5.0f);
				}
				lastPoint = point;
				i++;
			}
		}
	}
}