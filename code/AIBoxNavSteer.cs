using Sandbox;
using Steamworks;
using System;
using System.Buffers;

namespace AIBox {
	public class AIBoxNavSteer {
		protected AIBoxNavPath Path { get; private set; }

		public AIBoxNavSteer() {
			Path = new AIBoxNavPath();
		}

		public virtual void Tick(Vector3 currentPosition) {
			//using (Sandbox.Debug.Profile.Scope("Update Path")) {
			Path.Update(currentPosition, Target);
			//}

			Output.Finished = Path.IsEmpty;

			if (Output.Finished) {
				Output.Direction = Vector3.Zero;
				return;
			}

			//using (Sandbox.Debug.Profile.Scope("Update Direction")) {
			Output.Direction = Path.GetDirection(currentPosition);
			//}

			var avoid = GetAvoidance(currentPosition, 500);
			if (!avoid.IsNearlyZero()) {
				Output.Direction = (Output.Direction + avoid).Normal;
			}

			// For wandering
			/*
			base.Tick(position);

			if (Path.IsEmpty) {
				FindNewTarget(position);
			}
			*/
		}

		public float MinRadius { get; set; } = 200;
		public float MaxRadius { get; set; } = 500;

		public virtual bool FindNewTarget(Vector3 center) {
			var t = NavMesh.GetPointWithinRadius(center, MinRadius, MaxRadius);
			if (t.HasValue) {
				Target = t.Value;
			}

			return t.HasValue;
		}

		Vector3 GetAvoidance(Vector3 position, float radius) {
			var center = position + Output.Direction * radius * 0.5f;

			var objectRadius = 200.0f;
			Vector3 avoidance = default;

			foreach (var ent in Physics.GetEntitiesInSphere(center, radius)) {
				if (ent is not AIBoxNPC) continue;
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

		/*public virtual void DebugDrawPath() {
			using (Sandbox.Debug.Profile.Scope("Path Debug Draw")) {
				Path.DebugDraw(0.1f, 0.1f);
			}
		}*/

		public Vector3 Target { get; set; }

		public AIBoxNavSteerOutput Output;


		public struct AIBoxNavSteerOutput {
			public bool Finished;
			public Vector3 Direction;
		}
	}
}