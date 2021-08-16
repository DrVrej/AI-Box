using Sandbox;
using System.Threading.Tasks;

namespace AIBox {
	[Library("aibox_npc_citizen", Title = "Citizen NPC", Description = "Citizen NPC (default model).", Icon = "person", Spawnable = true)]
	public partial class NPC_Citizen : NPC {

		public override void Spawn() {
			base.Spawn();

			SetModel("models/citizen/citizen.vmdl");
			EyePos = Position + Vector3.Up * 64;
			CollisionGroup = CollisionGroup.Player;
			SetupPhysicsFromCapsule(PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius(72, 10));

			EnableHitboxes = true;

			SetMaterialGroup(Rand.Int(0, 4));


			// Head
			int head = Rand.Int(3); // 0 = Nothing
			if (head == 1) {
				// Random Hair
				int hair = Rand.Int(5);
				if (hair == 0) {
					new ModelEntity("models/citizen_clothes/hair/hair_femalebun.black.vmdl", this);
				} else if (hair == 1) {
					new ModelEntity("models/citizen_clothes/hair/hair_femalebun.blonde.vmdl", this);
				} else if (hair == 2) {
					new ModelEntity("models/citizen_clothes/hair/hair_femalebun.brown.vmdl", this);
				} else if (hair == 3) {
					new ModelEntity("models/citizen_clothes/hair/hair_femalebun.red.vmdl", this);
				} else if (hair == 4) {
					new ModelEntity("models/citizen_clothes/hair/hair_malestyle02.vmdl", this);
				} else if (hair == 5) {
					new ModelEntity("models/citizen_clothes/hair/hair_looseblonde/hair_looseblonde.vmdl", this);
				}
			} else if (head == 2) {
				// Random Hat
				int hat = Rand.Int(12);
				if (hat == 0) {
					new ModelEntity("models/citizen_clothes/hat/hat.tophat.vmdl", this);
				} else if (hat == 1) {
					new ModelEntity("models/citizen_clothes/hat/hat_beret.black.vmdl", this);
				} else if (hat == 2) {
					new ModelEntity("models/citizen_clothes/hat/hat_beret.red.vmdl", this);
				} else if (hat == 3) {
					new ModelEntity("models/citizen_clothes/hat/hat_cap.vmdl", this);
				} else if (hat == 4) {
					new ModelEntity("models/citizen_clothes/hat/hat_hardhat.vmdl", this);
				} else if (hat == 5) {
					new ModelEntity("models/citizen_clothes/hat/hat_leathercap.vmdl", this);
				} else if (hat == 6) {
					new ModelEntity("models/citizen_clothes/hat/hat_leathercapnobadge.vmdl", this);
				} else if (hat == 7) {
					new ModelEntity("models/citizen_clothes/hat/hat_securityhelmet.vmdl", this);
				} else if (hat == 8) {
					new ModelEntity("models/citizen_clothes/hat/hat_securityhelmetnostrap.vmdl", this);
				} else if (hat == 9) {
					new ModelEntity("models/citizen_clothes/hat/hat_service.vmdl", this);
				} else if (hat == 10) {
					new ModelEntity("models/citizen_clothes/hat/hat_uniform.police.vmdl", this);
				} else if (hat == 11) {
					new ModelEntity("models/citizen_clothes/hat/hat_woolly.vmdl", this);
				} else if (hat == 12) {
					new ModelEntity("models/citizen_clothes/hat/hat_woollybobble.vmdl", this);
				}
			}

			// Body Clothing
			int body = Rand.Int(8); // 0 = Nothing
									//if (body == 1) {
									//var dress = new ModelEntity("models/citizen_clothes/dress/dress.kneelength.vmdl", this);
									//dress.SetMaterialGroup(this.GetMaterialGroup());
			if (body == 1) {
				new ModelEntity("models/citizen_clothes/shirt/shirt_longsleeve.plain.vmdl", this);
			} else if (body == 2) {
				new ModelEntity("models/citizen_clothes/shirt/shirt_longsleeve.police.vmdl", this);
			} else if (body == 3) {
				new ModelEntity("models/citizen_clothes/shirt/shirt_longsleeve.scientist.vmdl", this);
			} else if (body == 4) {
				new ModelEntity("models/citizen_clothes/jacket/jacket.red.vmdl", this);
			} else if (body == 5) {
				new ModelEntity("models/citizen_clothes/jacket/jacket.tuxedo.vmdl", this);
			} else if (body == 6) {
				new ModelEntity("models/citizen_clothes/jacket/suitjacket/suitjacket.vmdl", this);
			} else if (body == 7) {
				new ModelEntity("models/citizen_clothes/jacket/labcoat.vmdl", this);
			} else if (body == 8) {
				new ModelEntity("models/citizen_clothes/jacket/jacket_heavy.vmdl", this);
			}

			// Pants or Shorts
			int pants = Rand.Int(0); // 0 = Nothing
			if (pants == 1) {
				var shorts = new ModelEntity("models/citizen_clothes/shoes/shorts.cargo.vmdl", this);
				shorts.SetMaterialGroup(this.GetMaterialGroup());
			} else if (pants == 2) {
				new ModelEntity("models/citizen_clothes/trousers/trousers.jeans.vmdl", this);
			} else if (pants == 3) {
				new ModelEntity("models/citizen_clothes/trousers/trousers.lab.vmdl", this);
			} else if (pants == 4) {
				new ModelEntity("models/citizen_clothes/trousers/trousers.police.vmdl", this);
			} else if (pants == 5) {
				new ModelEntity("models/citizen_clothes/trousers/trousers.smart.vmdl", this);
			} else if (pants == 6) {
				new ModelEntity("models/citizen_clothes/trousers/trousers.smarttan.vmdl", this);
			} else if (pants == 7) {
				new ModelEntity("models/citizen_clothes/trousers/trousers_tracksuit.vmdl", this);
			} else if (pants == 8) {
				new ModelEntity("models/citizen_clothes/trousers/trousers_tracksuitblue.vmdl", this);
			} else if (pants == 9) {
				new ModelEntity("models/citizen_clothes/trousers/smarttrousers/smarttrousers.vmdl", this);
			}

			// Shoes
			int shoes = Rand.Int(4); // 0 = Nothing
			if (shoes == 1) {
				new ModelEntity("models/citizen_clothes/shoes/shoes.police.vmdl", this);
			} else if (shoes == 2) {
				new ModelEntity("models/citizen_clothes/shoes/shoes.workboots.vmdl", this);
			} else if (shoes == 3) {
				new ModelEntity("models/citizen_clothes/shoes/trainers.vmdl", this);
			} else if (shoes == 4) {
				new ModelEntity("models/citizen_clothes/shoes/smartshoes/smartshoes.vmdl", this);
			}

			// Accessories
			int accessory = Rand.Int(1); // 0 = Nothing
			if (accessory == 1) {
				new ModelEntity("models/citizen_clothes/gloves/gloves_workgloves.vmdl", this);
			}

			Health = 100;
			Speed = Rand.Float(100, 300);
			Scale = Rand.Float(0.9f, 1.2f);
		}
	}
}