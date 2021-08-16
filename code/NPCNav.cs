using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace AIBox {
	public class NPCNav {
		public struct AIBoxNavSteerOutput {
			public bool Finished;
			public Vector3 Direction;
		}

		public Vector3 Target { get; set; }
		public AIBoxNavSteerOutput Output;
		public bool Wander = false;
		public float MinRadius { get; set; } = 200;
		public float MaxRadius { get; set; } = 500;

		public Vector3 TargetPosition;
		public List<Vector3> Points = new List<Vector3>(); // List of Points the NPC is navigating to right now
		public bool IsEmpty => Points.Count <= 1;

		public NPCNav() { }

		public virtual void Tick(Vector3 currentPosition) {
			if (Target != null) {
				Update(currentPosition, Target);
			}

			Output.Finished = IsEmpty;

			if (Output.Finished) {
				Output.Direction = Vector3.Zero;

				// For wandering
				if (Wander == true) {
					if (IsEmpty) {
						FindNewTarget_Wander(currentPosition);
					}
				} else { // If not wandering, then just return!
					return;
				}
			}

			Output.Direction = GetDirection(currentPosition);

			var avoid = GetAvoidance(currentPosition, 500);
			if (!avoid.IsNearlyZero()) {
				Output.Direction = (Output.Direction + avoid).Normal;
			}
		}

		public virtual bool FindNewTarget_Wander(Vector3 center) {
			var t = NavMesh.GetPointWithinRadius(center, MinRadius, MaxRadius);
			if (t.HasValue) {
				Target = t.Value;
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
				//Target = null;
				return false;
			} else {
				Target = closest.Position;
				return true;
			}
		}

		Vector3 GetAvoidance(Vector3 position, float radius) {
			var center = position + Output.Direction * radius * 0.5f;

			var objectRadius = 200.0f;
			Vector3 avoidance = default;

			foreach (var ent in Physics.GetEntitiesInSphere(center, radius)) {
				if (ent is not NPC) continue;
				if (ent.IsWorld) continue;

				var delta = (position - ent.Position).WithZ(0);
				var closeness = delta.Length;
				if (closeness < 0.001f) continue;
				var thrust = ((objectRadius - closeness) / objectRadius).Clamp(0, 1);
				if (thrust <= 0) continue;

				//avoidance += delta.Cross( Output.Direction ).Normal * thrust * 2.5f;
				avoidance += delta.Normal * thrust * thrust;
			}

			return avoidance;
		}

		public void Update(Vector3 from, Vector3 to) {
			bool needsBuild = false;

			if (!TargetPosition.IsNearlyEqual(to, 5)) {
				TargetPosition = to;
				needsBuild = true;
			}

			if (needsBuild) {
				var from_fixed = NavMesh.GetClosestPoint(from);
				var tofixed = NavMesh.GetClosestPoint(to);

				Points.Clear();
				NavMesh.GetClosestPoint(from);
				NavMesh.BuildPath(from_fixed.Value, tofixed.Value, Points);
				//Points.Add( NavMesh.GetClosestPoint( to ) );
			}

			if (Points.Count <= 1) {
				return;
			}

			var deltaToCurrent = from - Points[0];
			var deltaToNext = from - Points[1];
			var delta = Points[1] - Points[0];
			var deltaNormal = delta.Normal;

			if (deltaToNext.WithZ(0).Length < 20) {
				Points.RemoveAt(0);
				return;
			}

			// If we're in front of this line then
			// remove it and move on to next one
			if (deltaToNext.Normal.Dot(deltaNormal) >= 1.0f) {
				Points.RemoveAt(0);
			}
		}

		public float Distance(int point, Vector3 from) {
			if (Points.Count <= point) return float.MaxValue;

			return Points[point].WithZ(from.z).Distance(from);
		}

		public Vector3 GetDirection(Vector3 position) {
			if (Points.Count == 1) {
				return (Points[0] - position).WithZ(0).Normal;
			} else if (Points.Count == 0) {
				return position;
			}
			return (Points[1] - position).WithZ(0).Normal;
		}

		/*public virtual void DebugDrawPath() {
			using (Sandbox.Debug.Profile.Scope("Path Debug Draw")) {
				DebugDraw(0.1f, 0.1f);
			}
		}*/
	}
}