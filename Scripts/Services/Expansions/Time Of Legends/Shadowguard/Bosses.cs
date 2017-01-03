using Server;
using System;
using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;
using System.Linq;
using Server.Network;

namespace Server.Engines.Shadowguard
{
	public class ShadowguardBoss : BaseCreature
	{
        public const int MaxSummons = 8;

		public List<BaseCreature> SummonedHelpers { get; set; }
        public bool IsLastBoss { get; set; }

		public DateTime _NextSummon;
		
		public virtual Type[] SummonTypes { get { return null; } }
		public virtual Type[] ArtifactDrops { get { return _ArtifactTypes; } }
		
		private Type[] _ArtifactTypes = new Type[]
		{
			typeof(AnonsBoots),					typeof(AnonsBootsGargoyle),			typeof(AnonsSpellbook),			typeof(BalakaisShamanStaff),
			typeof(BalakaisShamanStaffGargoyle),typeof(EnchantressCameo),			typeof(GrugorsShield),			typeof(GrugorsShieldGargoyle),
			typeof(HalawasHuntingBow),			typeof(HalawasHuntingBowGargoyle),	typeof(HawkwindsRobe),			typeof(JumusSacredHide),
			typeof(JumusSacredHideGargoyle), 	typeof(JuonarsGrimoire), 			typeof(LereisHuntingSpear), 	typeof(LereisHuntingSpearGargoyle), 
			typeof(MinaxsSandles), 				typeof(MinaxsSandlesGargoyle), 		typeof(MocapotilsObsidianSword),typeof(OzymandiasObi),
			typeof(OzymandiasObiGargoyle), 		typeof(ShantysWaders), 				typeof(ShantysWadersGargoyle), 	typeof(TotemOfTheTribe),
			typeof(WamapsBoneEarrings), 		typeof(WamapsBoneEarringsGargoyle), typeof(UnstableTimeRift)
		};
		
		public ShadowguardBoss(AIType ai) : base(ai, FightMode.Closest, 10, 1, .15, .3)
		{
			_NextSummon = DateTime.UtcNow;
			
			SetHits(25000);
			SetMana(4500);
			SetStam(250);

            SetStr(225);
            SetInt(225);
            SetDex(250);

            Fame = 32000;
            Karma = -32000;
		}
		
		public override Poison PoisonImmune{ get { return Poison.Lethal; } }
        public override bool AlwaysMurderer { get { return true; } }

        public override void GenerateLoot()
        {
            if (IsLastBoss)
            {
                this.AddLoot(LootPack.SuperBoss, 6);
            }
            else
            {
                this.AddLoot(LootPack.SuperBoss, 3);
            }
        }
		
		public ShadowguardBoss(Serial serial) : base(serial)
		{
		}

        public int TotalSummons()
        {
            if (SummonedHelpers == null || SummonedHelpers.Count == 0)
                return 0;

            return SummonedHelpers.Where(bc => bc != null && bc.Alive).Count();
        }

		public override void OnGotMeleeAttack(Mobile m)
		{
			if(_NextSummon < DateTime.UtcNow)
				Summon();
				
			base.OnGotMeleeAttack(m);
		}
		
		public override void OnDamagedBySpell(Mobile m)
		{
			if(_NextSummon < DateTime.UtcNow)
				Summon();
				
			base.OnDamagedBySpell(m);
		}
		
		public override void OnDeath(Container c)
		{
			List<DamageStore> rights = GetLootingRights();
			
			foreach(DamageStore ds in rights.Where(s => s.m_HasRight))
			{
				int chance = 75 + (ds.m_Mobile.Luck / 15);
				
				if(chance > Utility.Random(5000))
				{
					Mobile m = ds.m_Mobile;
					Item artifact = Loot.Construct(ArtifactDrops[Utility.Random(ArtifactDrops.Length)]);
					
					if(artifact != null)
					{
						if(m.Backpack == null || !m.Backpack.TryDropItem(m, artifact, false))
						{
							m.BankBox.DropItem(artifact);
							m.SendMessage("For your valor in combating the fallen beast, a special reward has been placed in your bankbox.");
						}
						else
							m.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special reward has been bestowed on you.
					}
				}
			}

            if (IsLastBoss)
                DoGoldSpray();

			base.OnDeath(c);
		}

        private void DoGoldSpray()
        {
            if (this.Map != null)
            {
                for (int x = -12; x <= 12; ++x)
                {
                    for (int y = -12; y <= 12; ++y)
                    {
                        double dist = Math.Sqrt(x * x + y * y);

                        if (dist <= 12)
                            new GoodiesTimer(this.Map, X + x, Y + y).Start();
                    }
                }
            }
        }

        private class GoodiesTimer : Timer
        {
            private Map m_Map;
            private int m_X, m_Y;

            public GoodiesTimer(Map map, int x, int y)
                : base(TimeSpan.FromSeconds(Utility.RandomDouble() * 10.0))
            {
                m_Map = map;
                m_X = x;
                m_Y = y;
            }

            protected override void OnTick()
            {
                int z = m_Map.GetAverageZ(m_X, m_Y);
                bool canFit = m_Map.CanFit(m_X, m_Y, z, 6, false, false);

                for (int i = -3; !canFit && i <= 3; ++i)
                {
                    canFit = m_Map.CanFit(m_X, m_Y, z + i, 6, false, false);

                    if (canFit)
                        z += i;
                }

                if (!canFit)
                    return;

                Gold g = new Gold(500, 1000); // [WarLocke] Changed to vary by champion. Originally 500, 1000.
                g.MoveToWorld(new Point3D(m_X, m_Y, z), m_Map);

                if (0.5 >= Utility.RandomDouble())
                {
                    switch (Utility.Random(3))
                    {
                        case 0: // Fire column
                            {
                                Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
                                Effects.PlaySound(g, g.Map, 0x208);

                                break;
                            }
                        case 1: // Explosion
                            {
                                Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36BD, 20, 10, 5044);
                                Effects.PlaySound(g, g.Map, 0x307);

                                break;
                            }
                        case 2: // Ball of fire
                            {
                                Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36FE, 10, 10, 5052);

                                break;
                            }
                    }
                }
            }
        }
		
		public virtual void Summon()
		{
            int max = MaxSummons;

            ShadowguardEncounter inst = ShadowguardController.GetEncounter(this.Location, this.Map);

            if(inst != null)
                max += inst.PartySize() * 2;

			if(this.Map == null || this.SummonTypes == null || this.SummonTypes.Length == 0 || TotalSummons() > max)
				return;
				
			int count = Utility.RandomList(1, 2, 2, 2, 3, 3, 4, 5);
			
			for(int i = 0; i < count; i++)
			{
				Point3D p = Combatant.Location;
				
				for(int j = 0; j < 10; j++)
				{
					int x = Utility.RandomMinMax(p.X - 3, p.X + 3);
					int y = Utility.RandomMinMax(p.Y - 3, p.Y + 3);
					int z = this.Map.GetAverageZ(x, y);
					
					if(this.Map.CanSpawnMobile(x, y, z))
					{
						p = new Point3D(x, y, z);
						break;
					}
				}
				
				BaseCreature spawn = Activator.CreateInstance(SummonTypes[Utility.Random(SummonTypes.Length)]) as BaseCreature;
				
				if(spawn != null)
				{
					spawn.MoveToWorld(p, this.Map);
					spawn.Team = this.Team;
					spawn.SummonMaster = this;
					
					Timer.DelayCall(TimeSpan.FromSeconds(1), (o) =>
					{
						BaseCreature s = o as BaseCreature;
						
						if(s != null)
							s.Combatant = this.Combatant;
							
					}, spawn);

                    AddHelper(spawn);
				}
			}
			
			_NextSummon = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(20, 40));
		}

        protected virtual void AddHelper(BaseCreature bc)
        {
            if (SummonedHelpers == null)
                SummonedHelpers = new List<BaseCreature>();

            if (!SummonedHelpers.Contains(bc))
                SummonedHelpers.Add(bc);
        }

        public override void Delete()
        {
            base.Delete();

            if (SummonedHelpers != null)
            {
                SummonedHelpers.Free();
            }
        }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			
            writer.Write(IsLastBoss);

			writer.Write(SummonedHelpers == null ? 0 : SummonedHelpers.Count);
			
			if(SummonedHelpers != null)
				SummonedHelpers.ForEach(m => writer.Write(m));
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

            IsLastBoss = reader.ReadBool();

			int count = reader.ReadInt();
			
			if(count > 0)
			{
				for(int i = 0; i < count; i++)
				{
					BaseCreature summon = reader.ReadMobile() as BaseCreature;
					
					if(summon != null)
					{
						if(SummonedHelpers == null)
                            SummonedHelpers = new List<BaseCreature>();

                        SummonedHelpers.Add(summon);
					}
				}
			}
			
			_NextSummon = DateTime.UtcNow;
		}
	}

    public enum Form
    {
        Human = 0x190,
        Fire = 15,
        Cold = 163,
        Poison = 162,
        Energy = 164
    }

	public class Anon : ShadowguardBoss
	{
		public override Type[] SummonTypes { get { return _SummonTypes; } }
		private Type[] _SummonTypes = new Type[] { typeof(ElderGazer), typeof(EvilMage), typeof(Wisp) };
		
		private DateTime _LastChange;
		private Form _Form;
		
		public bool CanChange { get { return _LastChange + TimeSpan.FromSeconds(Utility.RandomMinMax(75, 90)) < DateTime.UtcNow; } }
	
        [CommandProperty(AccessLevel.GameMaster)]
		public Form Form 
		{ 
			get { return _Form; }
			set
			{
				Form old = _Form;
				
				if(old != value)
				{
					_Form = value;
					InvalidateForm();
					_LastChange = DateTime.UtcNow;
				}
			}
		}
	
        [Constructable]
		public Anon() : base(AIType.AI_Mage)
		{
			Name = "anon";
			Title = "the mage";

            Body = 0x190;
            HairItemID = 0x203C;

            Hue = Race.RandomSkinHue();

			SetInt(225, 250);
			SetDex(100);
			
			SetDamage( 12, 17 );
			
			SetDamageType(ResistanceType.Physical, 50);
			SetDamageType(ResistanceType.Energy, 50);
			
			SetSkill( SkillName.Wrestling, 110.0 );
			SetSkill( SkillName.Swords, 110.0 );
			SetSkill( SkillName.Anatomy, 10.0 );
			SetSkill( SkillName.MagicResist,  110.0 );
			SetSkill( SkillName.Magery, 120.0 );
			SetSkill( SkillName.EvalInt, 150.0 );
			SetSkill( SkillName.Meditation, 120.0 );
			
			SetResistance(ResistanceType.Physical, 90, 99);
			SetResistance(ResistanceType.Fire, 50, 60);
			SetResistance(ResistanceType.Cold, 50, 60);
			SetResistance(ResistanceType.Poison, 50, 60);
			SetResistance(ResistanceType.Energy, 50, 60);

            SetSkill(SkillName.Wrestling, 120);
            SetSkill(SkillName.Magery, 120);
            SetSkill(SkillName.EvalInt, 180);
            SetSkill(SkillName.Meditation, 200);
            SetSkill(SkillName.Tactics, 100);
            SetSkill(SkillName.MagicResist, 200);

			SetWearable(new Robe(), 1320);
			SetWearable(new WizardsHat(), 1320);
			SetWearable(new GnarledStaff(), 1320);
            SetWearable(new LeatherGloves(), 1320);

			_LastChange = DateTime.UtcNow;
		}

        public override void OnThink()
        {
            base.OnThink();

            if (Form != Form.Human && _LastChange + TimeSpan.FromSeconds(60) < DateTime.UtcNow)
                Form = Form.Human;
        }
		
		private void SetHighResistance(ResistanceType type)
		{
			//SetResistance(ResistanceType.Physical, type == ResistanceType.Physical ? 80 : 50, type == ResistanceType.Physical ? 90 : 60);
			SetResistance(ResistanceType.Fire, type == ResistanceType.Fire ? 80 : 50, type == ResistanceType.Fire ? 90 : 60);
			SetResistance(ResistanceType.Cold, type == ResistanceType.Cold ? 80 : 50, type == ResistanceType.Cold ? 90 : 60);
			SetResistance(ResistanceType.Poison, type == ResistanceType.Poison ? 80 : 50, type == ResistanceType.Poison ? 90 : 60);
			SetResistance(ResistanceType.Energy, type == ResistanceType.Energy ? 80 : 50, type == ResistanceType.Energy ? 90 : 60);
		}
		
		public void InvalidateForm()
		{
			switch(_Form)
			{
				case Form.Human: 
					if(Body != (int)Form.Human) 
					{
						Body = (int)Form.Human;
                        HueMod = -1;
						SetHighResistance(ResistanceType.Physical);
					}
					break;
				case Form.Fire:
					if(Body != (int)Form.Fire) 
					{
						Body = (int)Form.Fire;
                        HueMod = 0;
						SetHighResistance(ResistanceType.Fire);
					}
					break;
				case Form.Cold:
					if(Body != (int)Form.Cold) 
					{
						Body = (int)Form.Cold;
                        HueMod = 0;
						SetHighResistance(ResistanceType.Cold);
					}
					break;
				case Form.Poison:
					if(Body != (int)Form.Poison) 
					{
						Body = (int)Form.Poison;
                        HueMod = 0;
						SetHighResistance(ResistanceType.Poison);
					}
					break;
				case Form.Energy:
					if(Body != (int)Form.Energy) 
					{
						Body = (int)Form.Energy;
                        HueMod = 0;
						SetHighResistance(ResistanceType.Energy);
					}
					break;
			}
		}
		
		public override void OnGotMeleeAttack(Mobile m)
		{
			base.OnGotMeleeAttack(m);
			
			if(CanChange)
				CheckChange(m);
		}
		
		public void CheckChange(Mobile m)
		{
			BaseWeapon weapon = m.Weapon as BaseWeapon;
			
			int highest;
			int type = GetHighestDamageType(weapon, out highest);

			if(weapon != null)
			{
				switch(type)
				{
                    case 0: if (Form != Form.Human) Form = Form.Human; break;
                    case 1: if (Form != Form.Fire) Form = Form.Fire; break;
                    case 2: if (Form != Form.Cold) Form = Form.Cold; break;
                    case 3: if (Form != Form.Poison) Form = Form.Poison; break;
                    case 4: if (Form != Form.Energy) Form = Form.Energy; break;
				}
			}
		}
		
		private int GetHighestDamageType(BaseWeapon weapon, out int highest)
		{
			int phys, fire, cold, pois, nrgy, chaos, direct;
			weapon.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);
			
			int type = 0;
			highest = phys;
			
			if(fire > highest) { type = 1; highest = fire; }
			if(cold > highest) { type = 2; highest = cold; }
			if(pois > highest) { type = 3; highest = pois; }
			if(nrgy > highest) { type = 4; highest = nrgy; }
			
			return type;
		}
		
		public override void AlterMeleeDamageFrom( Mobile m, ref int damage )
		{
			base.AlterMeleeDamageFrom(m, ref damage);
			
			BaseWeapon weapon = m.Weapon as BaseWeapon;
			
			if(weapon != null)
			{
				SlayerEntry slayer = SlayerGroup.GetEntryByName( weapon.Slayer );
				
				if(slayer != null && slayer.Slays(m))
				{
					if(slayer == slayer.Group.Super)
						damage *= 2;
					else
						damage *= 3;
				}
				
				int highest;
				int type = GetHighestDamageType(weapon, out highest);
				int heal = (int)((double)damage * ((double)highest / 100.0));
				
				switch(this.Form)
				{
					case Form.Human:
						/*if(type == 0)
						{
							damage -= heal;
							Hits = Math.Min(Hits + heal, HitsMax);
						}*/
                        break;
					case Form.Fire:
						if(type == 1)
						{
							damage -= heal;
							Hits = Math.Min(Hits + heal, HitsMax);
						}
                        break;
					case Form.Cold:
						if(type == 2)
						{
							damage -= heal;
							Hits = Math.Min(Hits + heal, HitsMax);
						}
                        break;
					case Form.Poison:
						if(type == 3)
						{
							damage -= heal;
							Hits = Math.Min(Hits + heal, HitsMax);
						}
                        break;
					case Form.Energy:
						if(type == 4)
						{
							damage -= heal;
							Hits = Math.Min(Hits + heal, HitsMax);
						}
                        break;
				}
			}
		}
		
		public Anon(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			
			_LastChange = DateTime.UtcNow;
		}
	}
	
	public class Juonar : ShadowguardBoss
	{
		public override Type[] SummonTypes { get { return _SummonTypes; } }
		private Type[] _SummonTypes = new Type[] { typeof(SkeletalDragon), typeof(LichLord), typeof(WailingBanshee), typeof(FleshGolem) };
		
		public override bool CanAnimateDead{ get { return true; } }
		public override double AnimateChance{ get{ return 0.15; } }
		public override int AnimateScalar{ get{ return 150; } }
		public override BaseCreature Animates{ get{ return new FleshGolem(); } }

        public override bool CanDiscord { get { return true; } }
        public override bool PlayInstrumentSound { get { return false; } }

        private DateTime _NextTeleport;

        [Constructable]
		public Juonar() : base(AIType.AI_NecroMage)
		{
			Name = "juo'nar";
            Body = 78;
            BaseSoundID = 412;
            Hue = 2951;

			SetInt(225, 250);
			SetDex(100);
			
			SetDamage( 15, 20 );
			
			SetDamageType(ResistanceType.Physical, 50);
			SetDamageType(ResistanceType.Energy, 50);
			
			SetSkill( SkillName.Wrestling, 110.0 );
			SetSkill( SkillName.Anatomy, 10.0 );
			SetSkill( SkillName.MagicResist,  110.0 );
			SetSkill( SkillName.Magery, 120.0 );
			SetSkill( SkillName.EvalInt, 150.0 );
			SetSkill( SkillName.Meditation, 120.0 );
			SetSkill( SkillName.Necromancy, 120.0 );
			SetSkill( SkillName.SpiritSpeak, 120.0 );
            SetSkill( SkillName.Musicianship, 120.0 );
            SetSkill( SkillName.Discordance, 120.0 );
			
			SetResistance(ResistanceType.Physical, 40, 50);
			SetResistance(ResistanceType.Fire, 20, 30);
			SetResistance(ResistanceType.Cold, 50, 60);
			SetResistance(ResistanceType.Poison, 50, 60);
			SetResistance(ResistanceType.Energy, 50, 60);

            _NextTeleport = DateTime.UtcNow;
		}
		
		public Juonar(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

            _NextTeleport = DateTime.UtcNow;
		}
	}
	
	public class Virtuebane : ShadowguardBoss
	{
		public override Type[] SummonTypes { get { return _SummonTypes; } }
		private Type[] _SummonTypes = new Type[] { typeof(MinotaurCaptain), typeof(Daemon), typeof(Titan) };
		
		public override bool CanAnimateDead{ get { return true; } }
		public override double AnimateChance{ get{ return 0.15; } }
		public override int AnimateScalar{ get{ return 150; } }
		public override BaseCreature Animates{ get{ return new FleshGolem(); } }
        public override bool BardImmune { get { return true; } }

		private DateTime _NextNuke;
		private DateTime _NextDismount;

        [Constructable]
		public Virtuebane() : base(AIType.AI_NecroMage)
		{
			Name = "virtuebane";
		
			Body = 1071; // Giant monotaur?
            SpeechHue = 452;

			SetInt(225, 250);
			SetDex(100);
			
			SetDamage( 22, 29 );
			
			SetDamageType(ResistanceType.Physical, 50);
			SetDamageType(ResistanceType.Energy, 50);
			
			SetSkill( SkillName.Wrestling, 150.0 );
			SetSkill( SkillName.Anatomy, 10.0 );
			SetSkill( SkillName.MagicResist,  110.0 );
			SetSkill( SkillName.Magery, 120.0 );
			SetSkill( SkillName.EvalInt, 150.0 );
			SetSkill( SkillName.Meditation, 120.0 );
            SetSkill(SkillName.Necromancy, 120.0);
            SetSkill(SkillName.SpiritSpeak, 150.0);
			
			SetResistance(ResistanceType.Physical, 60, 70);
			SetResistance(ResistanceType.Fire, 60, 70);
			SetResistance(ResistanceType.Cold, 20, 30);
			SetResistance(ResistanceType.Poison, 60, 70);
			SetResistance(ResistanceType.Energy, 60, 70);
			
			_NextNuke = DateTime.UtcNow + TimeSpan.FromMinutes(1);
			_NextDismount = DateTime.UtcNow + TimeSpan.FromMinutes(1);
		}

        public override int GetDeathSound() { return 0x596; }
        public override int GetAttackSound() { return 0x597; }
        public override int GetIdleSound() { return 0x598; }
        public override int GetAngerSound() { return 0x599; }
        public override int GetHurtSound() { return 0x59A; }
		
		public override void OnThink()
		{
			base.OnThink();
			
			if(Combatant is Mobile && InRange(Combatant.Location, 10))
			{
				if(_NextNuke < DateTime.UtcNow && 0.05 > Utility.RandomDouble())
				{
					_NextNuke = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(60, 90));
					
					Say(1112362); // You will burn to a pile of ash! yellow hue
					Point3D p = Combatant.Location;

					Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
					{
						DoNuke(this.Location);
					});
				}
				else if (_NextDismount < DateTime.UtcNow)
				{
					_NextDismount = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(60, 90));
					
					DoDismount((Mobile)Combatant);
				}
			}
		}
		
		public void DoNuke(Point3D p)
		{
            if (!this.Alive || this.Map == null)
                return;

            int range = 8;

            //Flame Columns
            for (int i = 0; i < 2; i++)
            {
                Server.Misc.Geometry.Circle2D(this.Location, this.Map, i, (pnt, map) =>
                    {
                        Effects.SendLocationParticles(EffectItem.Create(pnt, map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
                    });
            }

            //Flash then boom
            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
                {
                    if (this.Alive && this.Map != null)
                    {
                        Packet flash = ScreenLightFlash.Instance;
                        IPooledEnumerable e = this.Map.GetClientsInRange(p, (range * 4) + 5);

                        foreach (NetState ns in e)
                        {
                            if (ns.Mobile != null)
                                ns.Mobile.Send(flash);
                        }

                        e.Free();

                        for (int i = 0; i < range; i++)
                        {
                            Server.Misc.Geometry.Circle2D(this.Location, this.Map, i, (pnt, map) =>
                            {
                                Effects.SendLocationEffect(pnt, map, 14000, 14, 10, Utility.RandomMinMax(2497, 2499), 2);
                            });
                        }
                    }
                });
			
			IPooledEnumerable eable = this.GetMobilesInRange(range);
			
			foreach(Mobile m in eable)
			{
				if ((m is PlayerMobile || (m is BaseCreature && ((BaseCreature)m).GetMaster() is PlayerMobile)) && CanBeHarmful(m))
					Timer.DelayCall(TimeSpan.FromSeconds(1.75), new TimerStateCallback(DoDamage_Callback), m);
			}

            eable.Free();
		}
		
		private void DoDamage_Callback(object o)
		{
            Mobile m = o as Mobile;
 
            if (m != null)
            {
                DoHarmful(m);
                AOS.Damage(m, this, Utility.RandomMinMax(100, 150), 50, 50, 0, 0, 0);

                Direction d = Utility.GetDirection(this, m);
                int range = 0;
                int x = m.X;
                int y = m.Y;
                int orx = x;
                int ory = y;

                while (range < 12)
                {
                    range++;
                    int lastx = x;
                    int lasty = y;

                    Movement.Movement.Offset(d, ref x, ref y);

                    if (!this.Map.CanSpawnMobile(x, y, this.Map.GetAverageZ(x, y)))
                    {
                        m.MoveToWorld(new Point3D(lastx, lasty, this.Map.GetAverageZ(lastx, lasty)), this.Map);
                        break;
                    }

                    if (range >= 12 && (orx != x || ory != y))
                    {
                        m.MoveToWorld(new Point3D(x, y, this.Map.GetAverageZ(x, y)), this.Map);
                    }
                }

				m.Paralyze(TimeSpan.FromSeconds(3));
            }
		}
		
		public void DoDismount(Mobile m)
		{
			this.MovingParticles( m, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160 );
            this.PlaySound( 0x15E );
			
			double range = m.GetDistanceToSqrt(this);
			
			Timer.DelayCall(TimeSpan.FromMilliseconds(250 * range), () =>
			{
				IMount mount = m.Mount;
				
				if(mount != null)
				{
					if(m is PlayerMobile)
						((PlayerMobile)m).SetMountBlock(BlockMountType.Dazed, TimeSpan.FromSeconds(10), true);
					else
						mount.Rider = null;
				}
				else if (m.Flying)
				{
					((PlayerMobile)m).SetMountBlock(BlockMountType.Dazed, TimeSpan.FromSeconds(10), true);
				}
				
				AOS.Damage( m, this, Utility.RandomMinMax( 15, 25 ), 100, 0, 0, 0, 0 );
			});
		}
		
		public Virtuebane(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
	
	public class Ozymandias : ShadowguardBoss
	{
	
		public override Type[] SummonTypes { get { return _SummonTypes; } }
		private Type[] _SummonTypes = new Type[] { typeof(LesserHiryu), typeof(EliteNinja), typeof(TsukiWolf) };
		
		public override double WeaponAbilityChance{ get{ return 0.4; } }
		public override WeaponAbility GetWeaponAbility()
		{
			return WeaponAbility.Dismount;
		}
		
		[Constructable]
		public Ozymandias() : base(AIType.AI_Archer)
		{
			Name = "ozymandias";
			Title = "the lord of castle barataria";

            Hue = Race.RandomSkinHue();
            Body = 0x190;
            FacialHairItemID = 0x2040;

			SetInt(225, 250);
			SetDex(225);
			
			SetDamage( 25, 32 );
			
			SetDamageType(ResistanceType.Physical, 100);
			
			SetSkill( SkillName.Wrestling, 150.0 );
			SetSkill( SkillName.Archery, 150.0 );
			SetSkill( SkillName.Anatomy, 100.0 );
            SetSkill(SkillName.Tactics, 125.0);
			SetSkill( SkillName.MagicResist,  110.0 );
			
			SetResistance(ResistanceType.Physical, 60, 70);
			SetResistance(ResistanceType.Fire, 20, 30);
			SetResistance(ResistanceType.Cold, 60, 70);
			SetResistance(ResistanceType.Poison, 60, 70);
			SetResistance(ResistanceType.Energy, 60, 70);

            SetWearable(new LeatherDo());
            SetWearable(new LeatherSuneate());
            SetWearable(new Yumi());
            SetWearable(new Waraji());
            SetWearable(new BoneArms());

            PackItem(new Arrow(25));
		}
	
		public Ozymandias(Serial serial) : base(serial)
		{
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}