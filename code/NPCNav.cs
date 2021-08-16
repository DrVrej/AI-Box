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

		public Vector3 Goal { get; set; }
		public Vector3 CurGoalPos;
		public List<Vector3> GoalPoints = new List<Vector3>(); // List of GoalPoints the NPC is navigating to right now
		public Vector3 GoalDir;
		public bool IsGoalFinished => GoalPoints.Count <= 1;

		public float MinRadius { get; set; } = 200;
		public float MaxRadius { get; set; } = 500;

		public float NextWanderTime { get; set; } = 0.0f;

		public NPCNav() { }

		public virtual void Tick(Vector3 currentPosition) {
			/*Log.Info("Current Goal: " + GoalPoints.Count + " || " + CurGoalPos + " || Finished? " + IsGoalFinished);
			Log.Info(Goal);
			for (int i = 0; i < GoalPoints.Count; i++) {
				Log.Info($"      GoalPoints[{i}]: {GoalPoints[i].ToString()}");
			}*/

			if (Goal != null) {
				UpdateGoal(currentPosition, Goal);
			}

			if (IsGoalFinished) {
				GoalDir = Vector3.Zero;

				// For wandering
				if (CurNavState == NAV_STATE.WANDER) {
					if (NextWanderTime <= Time.Now) {
						FindNewTarget_Wander(currentPosition);
						NextWanderTime = Time.Now + Rand.Float(3.0f, 10.0f);
					}
				} else if (CurNavState == NAV_STATE.CHASE) {
					FindNewTarget_Chase(currentPosition);
				} else { // If not wandering, then just return!
					return;
				}
			}

			GoalDir = GetDirection(currentPosition);

			var avoid = GetAvoidance(currentPosition, 300);
			if (!avoid.IsNearlyZero()) {
				GoalDir = (GoalDir + avoid).Normal;
			}

			DebugDraw(0.1f, 0.1f);
		}

		public virtual bool FindNewTarget_Wander(Vector3 center) {
			var t = NavMesh.GetPointWithinRadius(center, MinRadius, MaxRadius);
			if (t.HasValue) {
				Goal = t.Value;
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
				//Goal = null;
				return false;
			} else {
				Goal = closest.Position;
				return true;
			}
		}

		Vector3 GetAvoidance(Vector3 position, float radius) {
			var center = position + GoalDir * radius * 0.5f;

			var objectRadius = 200.0f;
			Vector3 avoidance = default;

			var avoidProps = GoalPoints.Count > 2;
			foreach (var ent in Physics.GetEntitiesInSphere(center, radius)) {
				if (ent.IsWorld) continue;
				if ((ent is NPC or Player) || (avoidProps && ent is Prop)) {
					var delta = (position - ent.Position).WithZ(0);
					var closeness = delta.Length;
					if (closeness < 0.001f) continue;
					var thrust = ((objectRadius - closeness) / objectRadius).Clamp(0, 1);
					if (thrust <= 0) continue;
					//avoidance += delta.Cross( GoalDir ).Normal * thrust * 2.5f;
					avoidance += delta.Normal * thrust * thrust;
					//Log.Info(ent + "   " + avoidance.Length);
				}
			}

			return avoidance;
		}

		public void UpdateGoal(Vector3 from, Vector3 to) {
			bool needsBuild = false;

			if (!CurGoalPos.IsNearlyEqual(to, 5)) {
				CurGoalPos = to;
				needsBuild = true;
			}

			if (needsBuild) {
				var from_fixed = NavMesh.GetClosestPoint(from);
				var tofixed = NavMesh.GetClosestPoint(to);

				GoalPoints.Clear();
				NavMesh.GetClosestPoint(from);
				NavMesh.BuildPath(from_fixed.Value, tofixed.Value, GoalPoints);
				//GoalPoints.Add( NavMesh.GetClosestPoint( to ) );
			}

			if (GoalPoints.Count <= 1) {
				return;
			}

			var deltaToCurrent = from - GoalPoints[0];
			var deltaToNext = from - GoalPoints[1];
			var delta = GoalPoints[1] - GoalPoints[0];
			var deltaNormal = delta.Normal;

			if (deltaToNext.WithZ(0).Length < 20) {
				GoalPoints.RemoveAt(0);
				return;
			}

			// If we're in front of this line then
			// remove it and move on to next one
			if (deltaToNext.Normal.Dot(deltaNormal) >= 1.0f) {
				GoalPoints.RemoveAt(0);
			}
		}

		public float Distance(int point, Vector3 from) {
			if (GoalPoints.Count <= point) return float.MaxValue;

			return GoalPoints[point].WithZ(from.z).Distance(from);
		}

		public Vector3 GetDirection(Vector3 position) {
			if (GoalPoints.Count == 1) {
				return (GoalPoints[0] - position).WithZ(0).Normal;
			} else if (GoalPoints.Count == 0) {
				return position;
			}
			return (GoalPoints[1] - position).WithZ(0).Normal;
		}

		public void DebugDraw(float time, float opacity = 1.0f) {
			var draw = Sandbox.Debug.Draw.ForSeconds(time);
			var lift = Vector3.Up * 2;

			draw.WithColor(Color.White.WithAlpha(opacity)).Circle(lift + CurGoalPos, Vector3.Up, 20.0f);

			int i = 0;
			var lastPoint = Vector3.Zero;
			foreach (var point in GoalPoints) {
				if (i > 0) {
					draw.WithColor(i == 1 ? Color.Green.WithAlpha(opacity) : Color.Cyan.WithAlpha(opacity)).Arrow(lastPoint + lift, point + lift, Vector3.Up, 5.0f);
				}

				lastPoint = point;
				i++;
			}
		}
	}
}