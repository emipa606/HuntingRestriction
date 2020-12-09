using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace HuntingRestricted
{
    // Token: 0x02000003 RID: 3
    public class MarvsHuntWhenSane
    {
        // Token: 0x06000005 RID: 5 RVA: 0x000022AC File Offset: 0x000004AC
        public static Trait GetNamedTrait(Pawn pawn, string TraitName)
        {
            List<Trait> allTraits = pawn.story.traits.allTraits;
            for (var i = 0; i < allTraits.Count; i++)
            {
                if (allTraits[i].def.defName == TraitName)
                {
                    return allTraits[i];
                }
            }
            return null;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002310 File Offset: 0x00000510
        public static void PostFix(ref bool __result, Pawn pawn)
        {
            if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < TerribleSightThresh)
            {
                __result = true;
            }
            else
            {
                ThingWithComps primary = pawn.equipment.Primary;
                if (primary != null && primary.def.IsMeleeWeapon && (Hunting_Loader.settings.shouldMeleeHuntBigGame || Hunting_Loader.settings.shouldMeleeHuntMediumGame || Hunting_Loader.settings.shouldMeleeHuntSmallGame))
                {
                    __result = false;
                }
            }
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002394 File Offset: 0x00000594
        public static void PostFix2(ref bool __result, Pawn pawn, Thing t, bool forced)
        {
            ThingWithComps primary = pawn.equipment.Primary;
            if (primary.def.IsMeleeWeapon)
            {
                var flag = Hunting_Loader.settings.shouldMeleeHuntBigGame || Hunting_Loader.settings.shouldMeleeHuntMediumGame || Hunting_Loader.settings.shouldMeleeHuntSmallGame;
                if (flag)
                {
                    __result = t is Pawn && pawn.CanReserve(t, 1, -1, null, false) && pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Hunt) != null && HasJobOnThing(pawn, t, forced);
                }
            }
            else
            {
                __result = __result && HasJobOnThing(pawn, t, forced);
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002440 File Offset: 0x00000640
        private static bool HasJobOnThing(Pawn pawn, Thing t, bool forced = true)
        {
            var imparedSight = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < ImpairedSightThresh;
            if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < TerribleSightThresh)
            {
                JobFailReason.Is(Translator.Translate(TerribleSightMsg));
                return false;
            }
            if (!(t is Pawn pawn2) || !pawn2.AnimalOrWildMan())
            {
                return false;
            }
            ThingWithComps primary = pawn.equipment.Primary;
            var isMeleeWeapon = primary.def.IsMeleeWeapon;
            if (isMeleeWeapon && pawn2.RaceProps.deathActionWorkerClass != null)
            {
                JobFailReason.Is(Translator.Translate(MeleeExplodingPrey));
                return false;
            }
            var rightBodySize = !(pawn2.BodySize < 0.65) && !(pawn2.BodySize > 1.3);
            if (isMeleeWeapon && !Hunting_Loader.settings.shouldMeleeHuntBigGame && pawn2.BodySize > 1.3)
            {
                JobFailReason.Is(Translator.Translate(MeleeHuntNotEnabled));
                return false;
            }
            if (isMeleeWeapon && !Hunting_Loader.settings.shouldMeleeHuntMediumGame && rightBodySize)
            {
                JobFailReason.Is(Translator.Translate(MeleeHuntNotEnabled));
                return false;
            }
            if (isMeleeWeapon && !Hunting_Loader.settings.shouldMeleeHuntSmallGame && pawn2.BodySize < 0.65)
            {
                JobFailReason.Is(Translator.Translate(MeleeHuntNotEnabled));
                return false;
            }
            if (!pawn2.Downed && pawn2.def.race.predator && !Hunting_Loader.settings.shouldHuntPredators)
            {
                JobFailReason.Is(Translator.Translate(PredatorsNotEnabled));
                return false;
            }
            if (!pawn2.Awake() && !pawn2.Downed && pawn2.def.race.predator && !isMeleeWeapon && Hunting_Loader.settings.shouldHuntPredators)
            {
                JobFailReason.Is(Translator.Translate(SleepingPredator));
                return false;
            }
            var shootingAccuracyTrait = false;
            Trait namedTrait = GetNamedTrait(pawn, shootingAccuracy);
            if (namedTrait != null)
            {
                shootingAccuracyTrait = namedTrait.Degree == -1;
                _ = namedTrait.Degree == 1;
            }
            if (shootingAccuracyTrait && !isMeleeWeapon && pawn2.BodySize < 0.65)
            {
                JobFailReason.Is(Translator.Translate(SizeMsg));
                return false;
            }
            if (imparedSight && !(pawn2.BodySize > 1.3))
            {
                JobFailReason.Is(Translator.Translate(TerribleSightMsg));
                return false;
            }
            IntVec3 intVec = pawn.Position - t.Position;
            var distanceCheck = intVec.LengthManhattan > distFar;
            _ = intVec.LengthManhattan > distVeryFar;
            var pawnHungry = pawn.needs.food != null && pawn.needs.food.CurLevelPercentage < hungerBlockPercent;
            var pawnTired = pawn.needs.rest != null && pawn.needs.rest.CurLevelPercentage < sleepBlockPercent;
            var pawnSlowed = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) < movementCapacityBlock;
            var num = 0f;
            foreach (KeyValuePair<ThingDef, int> keyValuePair in pawn.Map.resourceCounter.AllCountedAmounts)
            {
                if (keyValuePair.Key.IsNutritionGivingIngestible && keyValuePair.Key.ingestible.HumanEdible && keyValuePair.Key.ingestible.preferability > FoodPreferability.RawBad)
                {
                    num += keyValuePair.Key.ingestible.CachedNutrition * keyValuePair.Value;
                }
            }
            if (!(num < pawn.Map.mapPawns.FreeColonistsCount * 2) && (pawnHungry || pawnTired) && distanceCheck)
            {
                JobFailReason.Is(Translator.Translate(NeedsMsg));
                return false;
            }
            if (pawnSlowed && distanceCheck)
            {
                JobFailReason.Is(Translator.Translate(SlowMsg));
                return false;
            }
            if ((pawn.Map.mapTemperature.OutdoorTemp > pawn.SafeTemperatureRange().max || pawn.Map.mapTemperature.OutdoorTemp < pawn.SafeTemperatureRange().min) && (distanceCheck || pawnSlowed) && !(num < pawn.Map.mapPawns.FreeColonistsCount * 2))
            {
                JobFailReason.Is(Translator.Translate(TemperatureMsg));
                return false;
            }
            var statValue = pawn.GetStatValue(StatDefOf.ToxicSensitivity, true);
            if (pawn.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && ((statValue > 0f && intVec.LengthManhattan > distFallout / statValue) || pawnSlowed))
            {
                JobFailReason.Is(Translator.Translate(ToxicFalloutMsg));
                return false;
            }
            return true;
        }

        // Token: 0x0400000A RID: 10
        private const float TerribleSightThresh = 0.55f;

        // Token: 0x0400000B RID: 11
        private const float ImpairedSightThresh = 0.85f;

        // Token: 0x0400000C RID: 12
        private const float hungerBlockPercent = 0.4f;

        // Token: 0x0400000D RID: 13
        private const float sleepBlockPercent = 0.3f;

        // Token: 0x0400000E RID: 14
        private const float accuracyPenaltyBlock = 0.7f;

        // Token: 0x0400000F RID: 15
        private const float movementCapacityBlock = 0.5f;

        // Token: 0x04000010 RID: 16
        private const int distFar = 35;

        // Token: 0x04000011 RID: 17
        private const int distVeryFar = 150;

        // Token: 0x04000012 RID: 18
        private const int distFallout = 50;

        // Token: 0x04000013 RID: 19
        private const string shootingAccuracy = "ShootingAccuracy";

        // Token: 0x04000014 RID: 20
        private const string TerribleSightMsg = "CantHuntTerribleSight";

        // Token: 0x04000015 RID: 21
        private const string ToxicFalloutMsg = "CantHuntToxicBuildup";

        // Token: 0x04000016 RID: 22
        private const string NeedsMsg = "CantHuntNeedsLow";

        // Token: 0x04000017 RID: 23
        private const string SizeMsg = "CantHuntTargetTooSmall";

        // Token: 0x04000018 RID: 24
        private const string SlowMsg = "CantHuntTooSlow";

        // Token: 0x04000019 RID: 25
        private const string TemperatureMsg = "CantHuntTooWarmCold";

        // Token: 0x0400001A RID: 26
        private const string SleepingPredator = "CantHuntDangerousTarget";

        // Token: 0x0400001B RID: 27
        private const string PredatorsNotEnabled = "CantHuntPredatorsNotEnabled";

        // Token: 0x0400001C RID: 28
        private const string MeleeHuntNotEnabled = "CantHuntMeleeNotEnabled";

        // Token: 0x0400001D RID: 29
        private const string MeleeExplodingPrey = "CantHuntMeleeExplodingPrey";
    }
}
