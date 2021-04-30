using Server.Engines.Craft;
using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.SkillHandlers;
using Server.Spells;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public enum TreasureLevel
    {
        Stash,
        Supply,
        Cache,
        Hoard,
        Trove
    }

    public enum TreasurePackage
    {
        Artisan,
        Assassin,
        Mage,
        Ranger,
        Warrior
    }

    public enum TreasureFacet
    {
        Trammel,
        Felucca,
        Ilshenar,
        Malas,
        Tokuno,
        TerMur,
        Eodon
    }

    public enum ChestQuality
    {
        None,
        Rusty,
        Standard,
        Gold
    }

    public static class TreasureMapInfo
    {
        /// <summary>
        /// This is called from BaseCreature. Instead of editing EVERY creature that drops a map, we'll simply convert it here.
        /// </summary>
        /// <param name="level"></param>
        public static int ConvertLevel(int level)
        {
            if (level == -1)
                return level;

            switch (level)
            {
                default: return (int)TreasureLevel.Stash;
                case 2: return (int)TreasureLevel.Supply;
                case 3: return (int)TreasureLevel.Cache;
                case 4: return (int)TreasureLevel.Hoard;
                case 5: return (int)TreasureLevel.Trove;
            }
        }

        public static TreasureFacet GetFacet(IEntity e)
        {
            return GetFacet(e.Location, e.Map);
        }

        public static int PackageLocalization(TreasurePackage package)
        {
            switch (package)
            {
                case TreasurePackage.Artisan: return 1158989;
                case TreasurePackage.Assassin: return 1158987;
                case TreasurePackage.Mage: return 1158986;
                case TreasurePackage.Ranger: return 1158990;
                case TreasurePackage.Warrior: return 1158988;
            }

            return 0;
        }

        public static TreasureFacet GetFacet(IPoint2D p, Map map)
        {
            if (map == Map.TerMur)
            {
                if (SpellHelper.IsEodon(map, new Point3D(p.X, p.Y, 0)))
                {
                    return TreasureFacet.Eodon;
                }

                return TreasureFacet.TerMur;
            }

            if (map == Map.Felucca)
            {
                return TreasureFacet.Felucca;
            }

            if (map == Map.Malas)
            {
                return TreasureFacet.Malas;
            }

            if (map == Map.Ilshenar)
            {
                return TreasureFacet.Ilshenar;
            }

            if (map == Map.Tokuno)
            {
                return TreasureFacet.Tokuno;
            }

            return TreasureFacet.Trammel;
        }

        public static IEnumerable<Type> GetRandomEquipment(TreasurePackage package, TreasureFacet facet, int amount)
        {
            Type[] weapons = GetWeaponList(package, facet);
            Type[] armor = GetArmorList(package, facet);
            Type[] jewels = GetJewelList(facet);
            Type[] list;

            for (int i = 0; i < amount; i++)
            {
                switch (Utility.Random(5))
                {
                    default:
                    case 0: list = weapons; break;
                    case 1:
                    case 2: list = armor; break;
                    case 3:
                    case 4: list = jewels; break;
                }

                yield return list[Utility.Random(list.Length)];
            }
        }

        public static Type[] GetWeaponList(TreasurePackage package, TreasureFacet facet)
        {
            Type[] list = null;

            switch (facet)
            {
                case TreasureFacet.Trammel:
                case TreasureFacet.Felucca: list = _WeaponTable[(int)package][0]; break;
                case TreasureFacet.Ilshenar: list = _WeaponTable[(int)package][1]; break;
                case TreasureFacet.Malas: list = _WeaponTable[(int)package][2]; break;
                case TreasureFacet.Tokuno: list = _WeaponTable[(int)package][3]; break;
                case TreasureFacet.TerMur: list = _WeaponTable[(int)package][4]; break;
                case TreasureFacet.Eodon: list = _WeaponTable[(int)package][5]; break;
            }

            // tram/fel lists are always default
            if (list == null || list.Length == 0)
            {
                list = _WeaponTable[(int)package][0];
            }

            return list;
        }

        public static Type[] GetArmorList(TreasurePackage package, TreasureFacet facet)
        {
            Type[] list = null;

            switch (facet)
            {
                case TreasureFacet.Trammel:
                case TreasureFacet.Felucca: list = _ArmorTable[(int)package][0]; break;
                case TreasureFacet.Ilshenar: list = _ArmorTable[(int)package][1]; break;
                case TreasureFacet.Malas: list = _ArmorTable[(int)package][2]; break;
                case TreasureFacet.Tokuno: list = _ArmorTable[(int)package][3]; break;
                case TreasureFacet.TerMur: list = _ArmorTable[(int)package][4]; break;
                case TreasureFacet.Eodon: list = _ArmorTable[(int)package][5]; break;
            }

            // tram/fel lists are always default
            if (list == null || list.Length == 0)
            {
                list = _ArmorTable[(int)package][0];
            }

            return list;
        }

        public static Type[] GetJewelList(TreasureFacet facet)
        {
            if (facet == TreasureFacet.TerMur)
            {
                return _JewelTable[1];
            }

            return _JewelTable[0];
        }

        public static SkillName[] GetTranscendenceList(TreasureLevel level, TreasurePackage package)
        {
            if (level == TreasureLevel.Supply || level == TreasureLevel.Cache)
            {
                return null;
            }

            return _TranscendenceTable[(int)package];
        }

        public static SkillName[] GetAlacrityList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
        {
            if (level == TreasureLevel.Stash || facet == TreasureFacet.Felucca && level == TreasureLevel.Cache)
            {
                return null;
            }

            return _AlacrityTable[(int)package];
        }

        public static SkillName[] GetPowerScrollList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
        {
            if (facet != TreasureFacet.Felucca)
                return null;

            if (level >= TreasureLevel.Cache)
            {
                return _PowerscrollTable[(int)package];
            }

            return null;
        }

        public static Type[] GetCraftingMaterials(TreasureLevel level, TreasurePackage package, ChestQuality quality)
        {
            if (package == TreasurePackage.Artisan && level <= TreasureLevel.Supply && quality != ChestQuality.None)
            {
                return _MaterialTable[(int)quality - 1];
            }

            return null;
        }

        public static Type[] GetSpecialMaterials(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
        {
            if (package == TreasurePackage.Artisan && level == TreasureLevel.Supply)
            {
                return _SpecialMaterialTable[(int)facet];
            }

            return null;
        }

        public static Type[] GetDecorativeList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
        {
            Type[] list = null;

            if (level >= TreasureLevel.Cache)
            {
                list = _DecorativeTable[(int)package];

                if (facet == TreasureFacet.Malas)
                {
                    list = _DecorativeMalasArtifacts;
                }
            }
            else if (level == TreasureLevel.Supply)
            {
                list = _DecorativeMinorArtifacts;
            }

            return list;
        }

        public static Type[] GetReagentList(TreasureLevel level, TreasurePackage package, TreasureFacet facet)
        {
            if (level != TreasureLevel.Stash || package != TreasurePackage.Mage)
                return null;

            switch (facet)
            {
                case TreasureFacet.Felucca:
                case TreasureFacet.Trammel: return Loot.RegTypes;
                case TreasureFacet.Malas: return Loot.NecroRegTypes;
                case TreasureFacet.TerMur: return Loot.MysticRegTypes;
            }

            return null;
        }

        public static Recipe[] GetRecipeList(TreasureLevel level, TreasurePackage package)
        {
            if (package == TreasurePackage.Artisan && level == TreasureLevel.Supply)
            {
                List<Recipe> recipeList = new List<Recipe>();

                foreach (var value in Recipe.Recipes.Values)
                {
                    recipeList.Add(value);
                }

                return recipeList.ToArray();
            }

            return null;
        }

        public static Type[] GetSpecialLootList(TreasureLevel level, TreasurePackage package)
        {
            if (level == TreasureLevel.Stash)
                return null;

            Type[] list;

            if (level == TreasureLevel.Supply)
            {
                list = _SpecialSupplyLoot[(int)package];
            }
            else
            {
                list = _SpecialCacheHordeAndTrove;
            }

            if (package > TreasurePackage.Artisan)
            {
                list.Concat(_FunctionalMinorArtifacts);
            }

            return list;
        }

        public static int GetGemCount(ChestQuality quality, TreasureLevel level)
        {
            int baseAmount = 0;

            switch (quality)
            {
                case ChestQuality.Rusty: baseAmount = 7; break;
                case ChestQuality.Standard: baseAmount = Utility.RandomBool() ? 7 : 9; break;
                case ChestQuality.Gold: baseAmount = Utility.RandomList(7, 9, 11); break;
            }

            return baseAmount + (int)level * 5;
        }

        public static int GetGoldCount(TreasureLevel level)
        {
            switch (level)
            {
                default:
                case TreasureLevel.Stash: return Utility.RandomMinMax(10000, 40000);
                case TreasureLevel.Supply: return Utility.RandomMinMax(20000, 50000);
                case TreasureLevel.Cache: return Utility.RandomMinMax(30000, 60000);
                case TreasureLevel.Hoard: return Utility.RandomMinMax(40000, 70000);
                case TreasureLevel.Trove: return Utility.RandomMinMax(50000, 70000);
            }
        }

        public static int GetRefinementRolls(ChestQuality quality)
        {
            switch (quality)
            {
                default:
                case ChestQuality.Rusty: return 2;
                case ChestQuality.Standard: return 4;
                case ChestQuality.Gold: return 6;
            }
        }

        public static int GetResourceAmount(TreasureLevel level)
        {
            switch (level)
            {
                case TreasureLevel.Stash: return 50;
                case TreasureLevel.Supply: return 100;
            }

            return 0;
        }

        public static int GetRegAmount(ChestQuality quality)
        {
            switch (quality)
            {
                default:
                case ChestQuality.Rusty: return 20;
                case ChestQuality.Standard: return 40;
                case ChestQuality.Gold: return 60;
            }
        }

        public static int GetSpecialResourceAmount(ChestQuality quality)
        {
            switch (quality)
            {
                default:
                case ChestQuality.Rusty: return 1;
                case ChestQuality.Standard: return 2;
                case ChestQuality.Gold: return 3;
            }
        }

        public static int GetEquipmentAmount(Mobile from, TreasureLevel level, TreasurePackage package)
        {
            int amount = 0;

            switch (level)
            {
                default:
                case TreasureLevel.Stash: amount = 6; break;
                case TreasureLevel.Supply: amount = 8; break;
                case TreasureLevel.Cache: amount = package == TreasurePackage.Assassin ? 24 : 12; break;
                case TreasureLevel.Hoard: amount = 18; break;
                case TreasureLevel.Trove: amount = 36; break;
            }

            Party p = Party.Get(from);

            if (p != null && p.Count > 1)
            {
                for (int i = 0; i < p.Count - 1; i++)
                {
                    if (Utility.RandomBool())
                    {
                        amount++;
                    }
                }
            }

            return amount;
        }

        public static void GetMinMaxBudget(TreasureLevel level, Item item, out int min, out int max)
        {
            int preArtifact = Imbuing.GetMaxWeight(item) + 100;
            min = max = 0;

            switch (level)
            {
                default:
                case TreasureLevel.Stash:
                case TreasureLevel.Supply: min = 250; max = preArtifact; break;
                case TreasureLevel.Cache:
                case TreasureLevel.Hoard:
                case TreasureLevel.Trove: min = 500; max = 1300; break;
            }
        }

        private static readonly Type[][][] _WeaponTable =
        {
            new[] // Artisan
            {
                new[] { typeof(HammerPick), typeof(SledgeHammerWeapon), typeof(SmithyHammer), typeof(WarAxe), typeof(WarHammer), typeof(Axe), typeof(BattleAxe), typeof(DoubleAxe), typeof(ExecutionersAxe), typeof(Hatchet), typeof(LargeBattleAxe), typeof(OrnateAxe), typeof(TwoHandedAxe), typeof(Pickaxe) }, // Trammel, Felucca
                null, // Ilshenar
                null, // Malas
                null, // Tokuno
                new[] { typeof(HammerPick), typeof(SledgeHammerWeapon), typeof(SmithyHammer), typeof(WarAxe), typeof(WarHammer), typeof(Axe), typeof(BattleAxe), typeof(DoubleAxe), typeof(ExecutionersAxe), typeof(Hatchet), typeof(LargeBattleAxe), typeof(OrnateAxe), typeof(TwoHandedAxe), typeof(Pickaxe), typeof(DualShortAxes) },  // TerMur
                new Type[] {  }  // Eodon
            },
            new[] // Assassin
            {
                new[] { typeof(Dagger), typeof(Kryss), typeof(Cleaver), typeof(Cutlass), typeof(ElvenMachete) },
                null,
                null,
                null,
                new[] { typeof(Dagger), typeof(Kryss), typeof(Cleaver), typeof(Cutlass) },
                new[] { typeof(Dagger), typeof(Kryss), typeof(Cleaver), typeof(Cutlass), typeof(BladedWhip), typeof(BarbedWhip), typeof(SpikedWhip) }
            },
            new[] // Mage
            {
                new[] { typeof(BlackStaff), typeof(ShepherdsCrook), typeof(GnarledStaff), typeof(QuarterStaff) },
                null,
                null,
                null,
                null,
                null
            },
            new[] // Ranger
            {
                new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife) },
                new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife), typeof(SoulGlaive) },
                new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife), typeof(ElvenCompositeLongbow) },
                null,
                new[] { typeof(Bow), typeof(Crossbow), typeof(HeavyCrossbow), typeof(CompositeBow), typeof(ButcherKnife), typeof(SkinningKnife), typeof(GargishButcherKnife), typeof(Cyclone), typeof(SoulGlaive) },
                null
            },
            new[] // Warrior
            {
                new[] { typeof(Lance), typeof(Pike), typeof(Pitchfork), typeof(ShortSpear), typeof(WarFork), typeof(Club), typeof(Mace), typeof(Maul), typeof(WarAxe), typeof(Bardiche), typeof(Broadsword), typeof(CrescentBlade), typeof(Halberd), typeof(Longsword), typeof(Scimitar), typeof(VikingSword) },
                null,
                null,
                new[] { typeof(Lance), typeof(Pike), typeof(Pitchfork), typeof(ShortSpear), typeof(WarFork), typeof(Club), typeof(Mace), typeof(Maul), typeof(WarAxe), typeof(Bardiche), typeof(Broadsword), typeof(CrescentBlade), typeof(Halberd), typeof(Longsword), typeof(Scimitar), typeof(VikingSword), typeof(Bokuto), typeof(Daisho) },
                null,
                null
            }
        };

        private static readonly Type[][][] _ArmorTable =
        {
            new[] // Artisan
            {
                new[] { typeof(Bonnet), typeof(Cap), typeof(Circlet), typeof(ElvenGlasses), typeof(FeatheredHat), typeof(FlowerGarland), typeof(JesterHat), typeof(SkullCap), typeof(StrawHat), typeof(TallStrawHat), typeof(WideBrimHat) }, // Trammel/Fel
                null, // Ilshenar
                null, // Malas
                null, // Tokuno
                null, // TerMur
                new[] { typeof(Bonnet), typeof(Cap), typeof(Circlet), typeof(ElvenGlasses), typeof(FeatheredHat), typeof(FlowerGarland), typeof(JesterHat), typeof(SkullCap), typeof(StrawHat), typeof(TallStrawHat), typeof(WideBrimHat), typeof(ChefsToque) } // Eodon
            },
            new[] // Assassin
            {
                new[] { typeof(ChainLegs), typeof(ChainCoif), typeof(ChainChest), typeof(RingmailLegs), typeof(RingmailGloves), typeof(RingmailChest), typeof(RingmailArms), typeof(Bandana) }, // Trammel/Fel
                null, // Ilshenar
                null, // Malas
                new[] { typeof(ChainLegs), typeof(ChainCoif), typeof(ChainChest), typeof(RingmailLegs), typeof(RingmailGloves), typeof(RingmailArms), typeof(RingmailArms), typeof(Bandana), typeof(LeatherSuneate), typeof(LeatherMempo), typeof(LeatherJingasa), typeof(LeatherHiroSode), typeof(LeatherHaidate), typeof(LeatherDo) }, // Tokuno
                null, // TerMur
                null  // Eodon
            },
            new[] // Mage
            {
                new[] { typeof(LeafGloves), typeof(LeafLegs), typeof(LeafTonlet), typeof(LeafGorget), typeof(LeafArms),typeof(LeafChest), typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat) }, // Trammel/Fel
                null, // Ilshenar
                new[] { typeof(LeafGloves), typeof(LeafLegs), typeof(LeafTonlet), typeof(LeafGorget), typeof(LeafArms),typeof(LeafChest), typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat), typeof(BoneLegs), typeof(BoneHelm), typeof(BoneGloves), typeof(BoneChest), typeof(BoneArms) }, // Malas
                null, // Tokuno
                new[] { typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat) }, // TerMur
                new[] { typeof(LeatherArms), typeof(LeatherChest), typeof(LeatherLegs), typeof(LeatherGloves), typeof(LeatherGorget), typeof(WizardsHat) }  // Eodon
            },
            new[] // Ranger
            {
                new[] { typeof(HidePants), typeof(HidePauldrons), typeof(HideGorget), typeof(HideFemaleChest), typeof(HideChest), typeof(HideGloves), typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms), typeof(RavenHelm), typeof(VultureHelm), typeof(WingedHelm) }, // Trammel/Fel
                null, // Ilshenar
                null, // Malas
                new[] { typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms) }, // Tokuno
                new[] { typeof(HidePants), typeof(HidePauldrons), typeof(HideGorget), typeof(HideFemaleChest), typeof(HideChest), typeof(HideGloves), typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms), typeof(GargishLeatherKilt), typeof(GargishLeatherLegs), typeof(GargishLeatherArms), typeof(GargishLeatherChest) }, // TerMur
                new[] { typeof(StuddedLegs), typeof(StuddedGorget), typeof(StuddedGloves), typeof(StuddedChest), typeof(StuddedBustierArms), typeof(StuddedArms), typeof(TigerPeltSkirt), typeof(TigerPeltShorts), typeof(TigerPeltLegs), typeof(TigerPeltLongSkirt), typeof(TigerPeltHelm), typeof(TigerPeltChest), typeof(TigerPeltCollar), typeof(TigerPeltBustier), typeof(VultureHelm), typeof(TribalMask) } // Eodon
            },
            new[] // Warrior
            {
                new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield) }, // Trammel/Fel
                null, // Ilshenar
                new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield), typeof(DragonHelm), typeof(DragonGloves), typeof(DragonChest), typeof(DragonArms), typeof(DragonLegs) }, // Malas
                new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield), typeof(PlateSuneate), typeof(PlateMempo), typeof(PlateHiroSode), typeof(PlateHatsuburi), typeof(PlateHaidate), typeof(PlateDo), typeof(PlateBattleKabuto), typeof(DecorativePlateKabuto), typeof(LightPlateJingasa), typeof(SmallPlateJingasa)  }, // Tokuno
                new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield), typeof(GargishPlateArms), typeof(GargishPlateChest), typeof(GargishPlateKilt), typeof(GargishPlateLegs), typeof(GargishStoneKilt), typeof(GargishStoneLegs), typeof(GargishStoneArms), typeof(GargishStoneChest) }, // TerMur
                new[] { typeof(PlateLegs), typeof(PlateHelm), typeof(PlateGorget), typeof(PlateGloves), typeof(PlateChest), typeof(PlateArms), typeof(Bascinet), typeof(CloseHelm), typeof(Helmet), typeof(LeatherCap), typeof(NorseHelm), typeof(TricorneHat), typeof(BronzeShield), typeof(Buckler), typeof(ChaosShield), typeof(HeaterShield), typeof(MetalKiteShield), typeof(MetalShield), typeof(OrderShield), typeof(WoodenKiteShield), typeof(DragonTurtleHideHelm), typeof(DragonTurtleHideLegs), typeof(DragonTurtleHideChest), typeof(DragonTurtleHideBustier), typeof(DragonTurtleHideArms) } // Eodon
            }
        };

        private static readonly Type[][] _MaterialTable =
        {
            new[] { typeof(SpinedLeather), typeof(OakBoard), typeof(AshBoard), typeof(DullCopperIngot), typeof(ShadowIronIngot), typeof(CopperIngot) },
            new[] { typeof(HornedLeather), typeof(YewBoard), typeof(HeartwoodBoard), typeof(BronzeIngot), typeof(GoldIngot), typeof(AgapiteIngot) },
            new[] { typeof(BarbedLeather), typeof(BloodwoodBoard), typeof(FrostwoodBoard), typeof(ValoriteIngot), typeof(VeriteIngot) }
        };

        private static readonly Type[][] _JewelTable =
        {
            new[] { typeof(GoldRing), typeof(GoldBracelet), typeof(SilverRing), typeof(SilverBracelet) }, // standard
            new[] { typeof(GoldRing), typeof(GoldBracelet), typeof(SilverRing), typeof(SilverBracelet), typeof(GargishBracelet) } // Ranger/TerMur
        };

        private static readonly Type[][] _DecorativeTable =
        {
            new[] { typeof(SkullTiledFloorAddonDeed) },
            new[] { typeof(AncientWeapon3) },
            new[] { typeof(DecorativeHourglass) },
            new[] { typeof(AncientWeapon1), typeof(CreepingVine) },
            new[] { typeof(AncientWeapon2) }
        };

        private static readonly Type[][] _SpecialMaterialTable =
        {
            null, // tram
            null, // fel
            null, // ilsh
            new[] { typeof(LuminescentFungi), typeof(BarkFragment), typeof(Blight), typeof(Corruption), typeof(Muculent), typeof(Putrefaction), typeof(Scourge), typeof(Taint)  }, // malas
            null, // tokuno
            new[] { typeof(AbyssalCloth), typeof(EssencePrecision), typeof(EssenceAchievement), typeof(EssenceBalance), typeof(EssenceControl), typeof(EssenceDiligence), typeof(EssenceDirection), typeof(EssenceFeeling), typeof(EssenceOrder), typeof(EssencePassion), typeof(EssencePersistence), typeof(EssenceSingularity) }, // ter
            null  // eodon
        };

        private static readonly Type[][] _SpecialSupplyLoot =
        {
            new[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding), typeof(ShieldEngravingTool), null },
            new[] { typeof(ForgedPardon), typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding), typeof(Skeletonkey), typeof(MasterSkeletonKey), typeof(SurgeShield) },
            new[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding) },
            new[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding), typeof(TastyTreat) },
            new[] { typeof(LegendaryMapmakersGlasses), typeof(ManaPhasingOrb), typeof(RunedSashOfWarding) }
        };

        private static readonly Type[] _SpecialCacheHordeAndTrove =
        {
            typeof(OctopusNecklace), typeof(SkullGnarledStaff), typeof(SkullLongsword)
        };

        private static readonly Type[] _DecorativeMinorArtifacts =
        {
            typeof(CandelabraOfSouls), typeof(GoldBricks), typeof(PhillipsWoodenSteed), typeof(AncientShipModelOfTheHMSCape), typeof(AdmiralsHeartyRum)
        };

        private static readonly Type[] _DecorativeMalasArtifacts =
        {
            typeof(CoffinPiece)
        };

        private static readonly Type[] _FunctionalMinorArtifacts =
        {
            typeof(ArcticDeathDealer), typeof(BlazeOfDeath), typeof(BurglarsBandana),
            typeof(CavortingClub), typeof(DreadPirateHat),
            typeof(EnchantedTitanLegBone), typeof(GwennosHarp), typeof(IolosLute),
            typeof(LunaLance), typeof(NightsKiss), typeof(NoxRangersHeavyCrossbow),
            typeof(PolarBearMask), typeof(VioletCourage), typeof(HeartOfTheLion),
            typeof(ColdBlood), typeof(AlchemistsBauble), typeof(CaptainQuacklebushsCutlass),
            typeof(ShieldOfInvulnerability)
        };

        private static readonly SkillName[][] _TranscendenceTable =
        {
            new[] { SkillName.ArmsLore, SkillName.Blacksmith, SkillName.Carpentry, SkillName.Cartography, SkillName.Cooking, SkillName.Cooking, SkillName.Fletching, SkillName.Mining, SkillName.Tailoring },
            new[] { SkillName.Anatomy, SkillName.DetectHidden, SkillName.Fencing, SkillName.Poisoning, SkillName.RemoveTrap, SkillName.Snooping, SkillName.Stealth },
            new[] { SkillName.Magery, SkillName.Meditation, SkillName.MagicResist, SkillName.Spellweaving },
            new[] { SkillName.Alchemy, SkillName.AnimalLore, SkillName.AnimalTaming, SkillName.Archery },
            new[] { SkillName.Chivalry, SkillName.Focus, SkillName.Parry, SkillName.Swords, SkillName.Tactics, SkillName.Wrestling }
        };

        private static readonly SkillName[][] _AlacrityTable =
        {
            new[] { SkillName.ArmsLore, SkillName.Blacksmith, SkillName.Carpentry, SkillName.Cartography, SkillName.Cooking, SkillName.Cooking, SkillName.Fletching, SkillName.Mining, SkillName.Tailoring, SkillName.Lumberjacking },
            new[] { SkillName.DetectHidden, SkillName.Fencing, SkillName.Hiding, SkillName.Lockpicking, SkillName.Poisoning, SkillName.RemoveTrap, SkillName.Snooping, SkillName.Stealing, SkillName.Stealth },
            new[] { SkillName.Alchemy, SkillName.EvalInt, SkillName.Inscribe, SkillName.Magery, SkillName.Meditation, SkillName.Spellweaving, SkillName.SpiritSpeak },
            new[] { SkillName.AnimalLore, SkillName.AnimalTaming, SkillName.Archery, SkillName.Musicianship, SkillName.Peacemaking, SkillName.Provocation, SkillName.Tinkering, SkillName.Tracking, SkillName.Veterinary },
            new[] { SkillName.Chivalry, SkillName.Focus, SkillName.Macing, SkillName.Parry, SkillName.Swords, SkillName.Wrestling }
        };

        private static readonly SkillName[][] _PowerscrollTable =
        {
            null,
            new[] { SkillName.Ninjitsu },
            new[] { SkillName.Magery, SkillName.Meditation, SkillName.Mysticism, SkillName.Spellweaving, SkillName.SpiritSpeak },
            new[] { SkillName.AnimalTaming, SkillName.Discordance, SkillName.Provocation, SkillName.Veterinary },
            new[] { SkillName.Bushido, SkillName.Chivalry, SkillName.Focus, SkillName.Healing, SkillName.Parry, SkillName.Swords, SkillName.Tactics }
        };

        public static void Fill(Mobile from, TreasureMapChest chest, TreasureMap tMap)
        {
            TreasureLevel level = tMap.TreasureLevel;
            TreasurePackage package = tMap.Package;
            TreasureFacet facet = tMap.TreasureFacet;
            ChestQuality quality = chest.ChestQuality;

            chest.Movable = false;
            chest.Locked = true;

            chest.TrapType = TrapType.ExplosionTrap;

            switch ((int)level)
            {
                default:
                case 0:
                    chest.RequiredSkill = 5;
                    chest.TrapPower = 25;
                    chest.TrapLevel = 1;
                    break;
                case 1:
                    chest.RequiredSkill = 45;
                    chest.TrapPower = 75;
                    chest.TrapLevel = 3;
                    break;
                case 2:
                    chest.RequiredSkill = 75;
                    chest.TrapPower = 125;
                    chest.TrapLevel = 5;
                    break;
                case 3:
                    chest.RequiredSkill = 80;
                    chest.TrapPower = 150;
                    chest.TrapLevel = 6;
                    break;
                case 4:
                    chest.RequiredSkill = 80;
                    chest.TrapPower = 170;
                    chest.TrapLevel = 7;
                    break;
            }

            chest.LockLevel = chest.RequiredSkill - 10;
            chest.MaxLockLevel = chest.RequiredSkill + 40;

            if (Engines.JollyRoger.JollyRogerEvent.Instance.Running && 0.10 > Utility.RandomDouble())
            {
                chest.DropItem(new MysteriousFragment());
            }

            #region Refinements
            if (level == TreasureLevel.Stash)
            {
                RefinementComponent.Roll(chest, GetRefinementRolls(quality), 0.9);
            }
            #endregion

            #region TMaps
            bool dropMap = false;
            if (level < TreasureLevel.Trove && 0.1 > Utility.RandomDouble())
            {
                chest.DropItem(new TreasureMap(tMap.Level + 1, chest.Map));
                dropMap = true;
            }
            #endregion

            Type[] list = null;
            int amount = 0;
            double dropChance = 0.0;

            #region Gold
            int goldAmount = GetGoldCount(level);
            Bag lootBag = new BagOfGold();

            while (goldAmount > 0)
            {
                if (goldAmount <= 20000)
                {
                    lootBag.DropItem(new Gold(goldAmount));
                    goldAmount = 0;
                }
                else
                {
                    lootBag.DropItem(new Gold(20000));
                    goldAmount -= 20000;
                }

                chest.DropItem(lootBag);
            }
            #endregion

            #region Regs
            list = GetReagentList(level, package, facet);

            if (list != null)
            {
                amount = GetRegAmount(quality);
                lootBag = new BagOfRegs();

                for (int i = 0; i < amount; i++)
                {
                    lootBag.DropItemStacked(Loot.Construct(list));
                }

                chest.DropItem(lootBag);
                list = null;
            }
            #endregion

            #region Gems
            amount = GetGemCount(quality, level);

            if (amount > 0)
            {
                lootBag = new BagOfGems();

                for (var index = 0; index < Loot.GemTypes.Length; index++)
                {
                    Type gemType = Loot.GemTypes[index];

                    Item gem = Loot.Construct(gemType);
                    gem.Amount = amount;

                    lootBag.DropItem(gem);
                }

                chest.DropItem(lootBag);
            }
            #endregion

            #region Crafting Resources
            // TODO: DO each drop, or do only 1 drop?
            list = GetCraftingMaterials(level, package, quality);

            if (list != null)
            {
                amount = GetResourceAmount(level);

                for (var index = 0; index < list.Length; index++)
                {
                    Type type = list[index];

                    Item craft = Loot.Construct(type);
                    craft.Amount = amount;

                    chest.DropItem(craft);
                }

                list = null;
            }
            #endregion

            #region Special Resources
            // TODO: DO each drop, or do only 1 drop?
            list = GetSpecialMaterials(level, package, facet);

            if (list != null)
            {
                amount = GetSpecialResourceAmount(quality);

                for (var index = 0; index < list.Length; index++)
                {
                    Type type = list[index];

                    Item specialCraft = Loot.Construct(type);
                    specialCraft.Amount = amount;

                    chest.DropItem(specialCraft);
                }

                list = null;
            }
            #endregion

            #region Special Scrolls
            amount = (int)level + 1;

            if (dropMap)
            {
                amount--;
            }

            if (amount > 0)
            {
                SkillName[] transList = GetTranscendenceList(level, package);
                SkillName[] alacList = GetAlacrityList(level, package, facet);
                SkillName[] pscrollList = GetPowerScrollList(level, package, facet);

                List<Tuple<int, SkillName>> scrollList = new List<Tuple<int, SkillName>>();

                if (transList != null)
                {
                    for (var index = 0; index < transList.Length; index++)
                    {
                        SkillName sk = transList[index];

                        scrollList.Add(new Tuple<int, SkillName>(1, sk));
                    }
                }

                if (alacList != null)
                {
                    for (var index = 0; index < alacList.Length; index++)
                    {
                        SkillName sk = alacList[index];

                        scrollList.Add(new Tuple<int, SkillName>(2, sk));
                    }
                }

                if (pscrollList != null)
                {
                    for (var index = 0; index < pscrollList.Length; index++)
                    {
                        SkillName sk = pscrollList[index];

                        scrollList.Add(new Tuple<int, SkillName>(3, sk));
                    }
                }

                if (scrollList.Count > 0)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        Tuple<int, SkillName> random = scrollList[Utility.Random(scrollList.Count)];

                        switch (random.Item1)
                        {
                            case 1: chest.DropItem(new ScrollOfTranscendence(random.Item2, Utility.RandomMinMax(1.0, chest.Map == Map.Felucca ? 7.0 : 5.0) / 10)); break;
                            case 2: chest.DropItem(new ScrollOfAlacrity(random.Item2)); break;
                            case 3: chest.DropItem(new PowerScroll(random.Item2, 110.0)); break;
                        }
                    }
                }
            }
            #endregion

            #region Decorations
            switch (level)
            {
                case TreasureLevel.Stash: dropChance = 0.00; break;
                case TreasureLevel.Supply: dropChance = 0.10; break;
                case TreasureLevel.Cache: dropChance = 0.20; break;
                case TreasureLevel.Hoard: dropChance = 0.40; break;
                case TreasureLevel.Trove: dropChance = 0.50; break;
            }

            if (Utility.RandomDouble() < dropChance)
            {
                list = GetDecorativeList(level, package, facet);

                if (list != null)
                {
                    if (list.Length > 0)
                    {
                        Item deco = Loot.Construct(list[Utility.Random(list.Length)]);

                        bool decorativeArtifact = false;

                        for (var index = 0; index < _DecorativeMinorArtifacts.Length; index++)
                        {
                            var t = _DecorativeMinorArtifacts[index];

                            if (t == deco.GetType())
                            {
                                decorativeArtifact = true;
                                break;
                            }
                        }

                        if (decorativeArtifact)
                        {
                            Container pack = new Backpack
                            {
                                Hue = 1278
                            };

                            pack.DropItem(deco);
                            chest.DropItem(pack);
                        }
                        else
                        {
                            chest.DropItem(deco);
                        }
                    }

                    list = null;
                }
            }

            switch (level)
            {
                case TreasureLevel.Stash: dropChance = 0.00; break;
                case TreasureLevel.Supply: dropChance = 0.10; break;
                case TreasureLevel.Cache: dropChance = 0.20; break;
                case TreasureLevel.Hoard: dropChance = 0.50; break;
                case TreasureLevel.Trove: dropChance = 0.75; break;
            }

            if (Utility.RandomDouble() < dropChance)
            {
                list = GetSpecialLootList(level, package);

                if (list != null)
                {
                    if (list.Length > 0)
                    {
                        Type type = MutateType(list[Utility.Random(list.Length)], facet);
                        Item deco;

                        if (type == null)
                        {
                            deco = TreasureMapChest.GetRandomRecipe();
                        }
                        else
                        {
                            deco = Loot.Construct(type);
                        }

                        if (deco is SkullGnarledStaff || deco is SkullLongsword)
                        {
                            if (package == TreasurePackage.Artisan)
                            {
                                ((IQuality)deco).Quality = ItemQuality.Exceptional;
                            }
                            else
                            {
                                int min, max;
                                GetMinMaxBudget(level, deco, out min, out max);
                                RunicReforging.GenerateRandomItem(deco, from is PlayerMobile pm ? pm.RealLuck : from.Luck, min, max, chest.Map);
                            }
                        }

                        bool functionalArtifacts = false;

                        for (var index = 0; index < _FunctionalMinorArtifacts.Length; index++)
                        {
                            var t = _FunctionalMinorArtifacts[index];

                            if (t == type)
                            {
                                functionalArtifacts = true;
                                break;
                            }
                        }

                        if (functionalArtifacts)
                        {
                            Container pack = new Backpack
                            {
                                Hue = 1278
                            };

                            pack.DropItem(deco);
                            chest.DropItem(pack);
                        }
                        else
                        {
                            chest.DropItem(deco);
                        }
                    }

                    list = null;
                }
            }
            #endregion

            #region Magic Equipment
            amount = GetEquipmentAmount(from, level, package);

            foreach (Type type in GetRandomEquipment(package, facet, amount))
            {
                Item item = Loot.Construct(type);
                int min, max;
                GetMinMaxBudget(level, item, out min, out max);

                if (item != null)
                {
                    RunicReforging.GenerateRandomItem(item, from is PlayerMobile pm ? pm.RealLuck : from.Luck, min, max, chest.Map);
                    chest.DropItem(item);
                }
            }

            list = null;
            #endregion
        }

        private static Type MutateType(Type type, TreasureFacet facet)
        {
            if (type == typeof(SkullGnarledStaff))
            {
                type = typeof(GargishSkullGnarledStaff);
            }
            else if (type == typeof(SkullLongsword))
            {
                type = typeof(GargishSkullLongsword);
            }

            return type;
        }
    }
}

