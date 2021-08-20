using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

[Library( "physgun" )]
public partial class PhysGun : Carriable
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	protected PhysicsBody holdBody;
	protected WeldJoint holdJoint;

	protected PhysicsBody heldBody;
	protected Vector3 heldPos;
	protected Rotation heldRot;

	protected float holdDistance;
	protected bool grabbing;

	protected virtual float MinTargetDistance => 0.0f;
	protected virtual float MaxTargetDistance => 10000.0f;
	protected virtual float LinearFrequency => 20.0f;
	protected virtual float LinearDampingRatio => 1.0f;
	protected virtual float AngularFrequency => 20.0f;
	protected virtual float AngularDampingRatio => 1.0f;
	protected virtual float TargetDistanceSpeed => 50.0f;
	protected virtual float RotateSpeed => 0.2f;
	protected virtual float RotateSnapAt => 45.0f;

	[Net] public bool BeamActive { get; set; }
	[Net] public Entity GrabbedEntity { get; set; }
	[Net] public int GrabbedBone { get; set; }
	[Net] public Vector3 GrabbedPos { get; set; }

	public PhysicsBody HeldBody => heldBody;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );

		CollisionGroup = CollisionGroup.Weapon;
		SetInteractsAs( CollisionLayer.Debris );
	}

	public override void Simulate( Client client )
	{
		if ( Owner is not Player owner ) return;

		var eyePos = owner.EyePos;
		var eyeDir = owner.EyeRot.Forward;
		var eyeRot = Rotation.From( new Angles( 0.0f, owner.EyeRot.Angles().yaw, 0.0f ) );

		if ( Input.Pressed( InputButton.Attack1 ) )
		{
			(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

			if ( !grabbing )
				grabbing = true;
		}

		bool grabEnabled = grabbing && Input.Down( InputButton.Attack1 );
		bool wantsToFreeze = Input.Pressed( InputButton.Attack2 );

		if ( GrabbedEntity.IsValid() && wantsToFreeze )
		{
			(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );
		}

		BeamActive = grabEnabled;

		if ( IsServer )
		{
			using ( Prediction.Off() )
			{
				if ( !holdBody.IsValid() )
					return;

				if ( grabEnabled )
				{
					if ( heldBody.IsValid() )
					{
						UpdateGrab( eyePos, eyeRot, eyeDir, wantsToFreeze );
					}
					else
					{
						TryStartGrab( owner, eyePos, eyeRot, eyeDir );
					}
				}
				else if ( grabbing )
				{
					GrabEnd();
				}

				if ( !grabbing && Input.Pressed( InputButton.Reload ) )
				{
					TryUnfreezeAll( owner, eyePos, eyeRot, eyeDir );
				}
			}
		}

		if ( BeamActive )
		{
			Input.MouseWheel = 0;
		}
	}

	private static bool IsBodyGrabbed( PhysicsBody body )
	{
		// There for sure is a better way to deal with this
		if ( All.OfType<PhysGun>().Any( x => x?.HeldBody?.PhysicsGroup == body?.PhysicsGroup ) ) return true;
		if ( All.OfType<GravGun>().Any( x => x?.HeldBody?.PhysicsGroup == body?.PhysicsGroup ) ) return true;

		return false;
	}

	private void TryUnfreezeAll( Player owner, Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir )
	{
		var tr = Trace.Ray( eyePos, eyePos + eyeDir * MaxTargetDistance )
			.UseHitboxes()
			.Ignore( owner, false )
			.HitLayer( CollisionLayer.Debris )
			.Run();

		if ( !tr.Hit || !tr.Entity.IsValid() || tr.Entity.IsWorld ) return;

		var rootEnt = tr.Entity.Root;
		if ( !rootEnt.IsValid() ) return;

		var physicsGroup = rootEnt.PhysicsGroup;
		if ( physicsGroup == null ) return;

		bool unfrozen = false;

		for ( int i = 0; i < physicsGroup.BodyCount; ++i )
		{
			var body = physicsGroup.GetBody( i );
			if ( !body.IsValid() ) continue;

			if ( body.BodyType == PhysicsBodyType.Static )
			{
				body.BodyType = PhysicsBodyType.Dynamic;
				unfrozen = true;
			}
		}

		if ( unfrozen )
		{
			var freezeEffect = Particles.Create( "particles/physgun_freeze.vpcf" );
			freezeEffect.SetPosition( 0, tr.EndPos );
		}
	}

	private void TryStartGrab( Player owner, Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir )
	{
		var tr = Trace.Ray( eyePos, eyePos + eyeDir * MaxTargetDistance )
			.UseHitboxes()
			.Ignore( owner, false )
			.HitLayer( CollisionLayer.Debris )
			.Run();

		if ( !tr.Hit || !tr.Entity.IsValid() || !tr.Body.IsValid() || tr.Entity.IsWorld ) return;

		var rootEnt = tr.Entity.Root;
		var body = tr.Body;

		if ( tr.Entity.Parent.IsValid() )
		{
			if ( rootEnt.IsValid() && rootEnt.PhysicsGroup != null )
			{
				body = rootEnt.PhysicsGroup.GetBody( 0 );
			}
		}

		if ( !body.IsValid() )
			return;

		//
		// Don't move keyframed 
		//
		if ( body.BodyType == PhysicsBodyType.Keyframed )
			return;

		// Unfreeze
		if ( body.BodyType == PhysicsBodyType.Static )
		{
			body.BodyType = PhysicsBodyType.Dynamic;
		}

		if ( IsBodyGrabbed( body ) )
			return;

		GrabInit( body, eyePos, tr.EndPos, eyeRot );

		GrabbedEntity = rootEnt;
		GrabbedPos = body.Transform.PointToLocal( tr.EndPos );
		GrabbedBone = tr.Entity.PhysicsGroup.GetBodyIndex( body );

		var client = GetClientOwner();
		if ( client != null )
		{
			client.Pvs.Add( GrabbedEntity );
		}
	}

	private void UpdateGrab( Vector3 eyePos, Rotation eyeRot, Vector3 eyeDir, bool wantsToFreeze )
	{
		if ( wantsToFreeze )
		{
			heldBody.BodyType = PhysicsBodyType.Static;

			if ( GrabbedEntity.IsValid() )
			{
				var freezeEffect = Particles.Create( "particles/physgun_freeze.vpcf" );
				freezeEffect.SetPosition( 0, heldBody.Transform.PointToWorld( GrabbedPos ) );
			}

			GrabEnd();
			return;
		}

		MoveTargetDistance( Input.MouseWheel * TargetDistanceSpeed );

		bool rotating = Input.Down( InputButton.Use );
		bool snapping = false;

		if ( rotating )
		{
			EnableAngularSpring( Input.Down( InputButton.Run ) ? 100.0f : 0.0f );
			DoRotate( eyeRot, Input.MouseDelta * RotateSpeed );

			snapping = Input.Down( InputButton.Run );
		}
		else
		{
			DisableAngularSpring();
		}

		GrabMove( eyePos, eyeDir, eyeRot, snapping );
	}

	private void EnableAngularSpring( float scale )
	{
		if ( holdJoint.IsValid )
		{
			holdJoint.AngularDampingRatio = AngularDampingRatio * scale;
			holdJoint.AngularFrequency = AngularFrequency * scale;
		}
	}

	private void DisableAngularSpring()
	{
		if ( holdJoint.IsValid )
		{
			holdJoint.AngularDampingRatio = 0.0f;
			holdJoint.AngularFrequency = 0.0f;
		}
	}

	private void Activate()
	{
		if ( !IsServer )
			return;

		if ( !holdBody.IsValid() )
		{
			holdBody = new PhysicsBody
			{
				BodyType = PhysicsBodyType.Keyframed
			};
		}
	}

	private void Deactivate()
	{
		if ( IsServer )
		{
			GrabEnd();

			holdBody?.Remove();
			holdBody = null;
		}

		KillEffects();
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		Activate();
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		Deactivate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Deactivate();
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	private void GrabInit( PhysicsBody body, Vector3 startPos, Vector3 grabPos, Rotation rot )
	{
		if ( !body.IsValid() )
			return;

		GrabEnd();

		grabbing = true;
		heldBody = body;
		holdDistance = Vector3.DistanceBetween( startPos, grabPos );
		holdDistance = holdDistance.Clamp( MinTargetDistance, MaxTargetDistance );
		heldPos = heldBody.Transform.PointToLocal( grabPos );
		heldRot = rot.Inverse * heldBody.Rotation;

		holdBody.Position = grabPos;
		holdBody.Rotation = heldBody.Rotation;

		heldBody.Wake();
		heldBody.EnableAutoSleeping = false;

		holdJoint = PhysicsJoint.Weld
			.From( holdBody )
			.To( heldBody, heldPos )
			.WithLinearSpring( LinearFrequency, LinearDampingRatio, 0.0f )
			.WithAngularSpring( 0.0f, 0.0f, 0.0f )
			.Create();
	}

	private void GrabEnd()
	{
		if ( holdJoint.IsValid )
		{
			holdJoint.Remove();
		}

		if ( heldBody.IsValid() )
		{
			heldBody.EnableAutoSleeping = true;
		}

		var client = GetClientOwner();
		if ( client != null && GrabbedEntity.IsValid() )
		{
			client.Pvs.Remove( GrabbedEntity );
		}

		heldBody = null;
		GrabbedEntity = null;
		grabbing = false;
	}

	private void GrabMove( Vector3 startPos, Vector3 dir, Rotation rot, bool snapAngles )
	{
		if ( !heldBody.IsValid() )
			return;

		holdBody.Position = startPos + dir * holdDistance;
		holdBody.Rotation = rot * heldRot;

		if ( snapAngles )
		{
			var angles = holdBody.Rotation.Angles();

			holdBody.Rotation = Rotation.From(
				MathF.Round( angles.pitch / RotateSnapAt ) * RotateSnapAt,
				MathF.Round( angles.yaw / RotateSnapAt ) * RotateSnapAt,
				MathF.Round( angles.roll / RotateSnapAt ) * RotateSnapAt
			);
		}
	}

	private void MoveTargetDistance( float distance )
	{
		holdDistance += distance;
		holdDistance = holdDistance.Clamp( MinTargetDistance, MaxTargetDistance );
	}

	protected virtual void DoRotate( Rotation eye, Vector3 input )
	{
		var localRot = eye;
		localRot *= Rotation.FromAxis( Vector3.Up, input.x );
		localRot *= Rotation.FromAxis( Vector3.Right, input.y );
		localRot = eye.Inverse * localRot;

		heldRot = localRot * heldRot;
	}

	public override void BuildInput( InputBuilder owner )
	{
		if ( !GrabbedEntity.IsValid() )
			return;

		if ( !owner.Down( InputButton.Attack1 ) )
			return;

		if ( owner.Down( InputButton.Use ) )
		{
			owner.ViewAngles = owner.OriginalViewAngles;
		}
	}

	public override bool IsUsable( Entity user )
	{
		return Owner == null || HeldBody.IsValid();
	}
}
