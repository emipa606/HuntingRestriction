using RimWorld;
using Verse;
using Verse.AI;

namespace HuntingRestricted;

public class MarvsHuntWhenSane
{
    private const float TerribleSightThresh = 0.55f;

    private const float ImpairedSightThresh = 0.85f;

    private const float accuracyPenaltyBlock = 0.7f;

    private const float movementCapacityBlock = 0.5f;

    private const int distFar = 35;

    private const int distVeryFar = 150;

    private const int distFallout = 50;

    private const string shootingAccuracy = "ShootingAccuracy";

    private const string TerribleSightMsg = "CantHuntTerribleSight";

    private const string ToxicFalloutMsg = "CantHuntToxicBuildup";

    private const string NeedsMsg = "CantHuntNeedsLow";

    private const string SizeMsg = "CantHuntTargetTooSmall";

    private const string SlowMsg = "CantHuntTooSlow";

    private const string TemperatureMsg = "CantHuntTooWarmCold";

    private const string SleepingPredator = "CantHuntDangerousTarget";

    private const string PredatorsNotEnabled = "CantHuntPredatorsNotEnabled";

    private const string MeleeHuntNotEnabled = "CantHuntMeleeNotEnabled";

    private const string MeleeExplodingPrey = "CantHuntMeleeExplodingPrey";

    public static Trait GetNamedTrait(Pawn pawn, string TraitName)
    {
        var allTraits = pawn.story.traits.allTraits;
        foreach (var getNamedTrait in allTraits)
        {
            if (getNamedTrait.def.defName == TraitName)
            {
                return getNamedTrait;
            }
        }

        return null;
    }

    public static void PostFix(ref bool __result, Pawn pawn)
    {
        if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < TerribleSightThresh)
        {
            __result = true;
        }
        else
        {
            var primary = pawn.equipment.Primary;
            if (primary != null && primary.def.IsMeleeWeapon && (Hunting_Loader.settings.shouldMeleeHuntBigGame ||
                                                                 Hunting_Loader.settings
                                                                     .shouldMeleeHuntMediumGame ||
                                                                 Hunting_Loader.settings.shouldMeleeHuntSmallGame))
            {
                __result = false;
            }
        }
    }

    public static void PostFix2(ref bool __result, Pawn pawn, Thing t)
    {
        var primary = pawn.equipment.Primary;
        if (primary.def.IsMeleeWeapon)
        {
            if (Hunting_Loader.settings.shouldMeleeHuntBigGame ||
                Hunting_Loader.settings.shouldMeleeHuntMediumGame ||
                Hunting_Loader.settings.shouldMeleeHuntSmallGame)
            {
                __result = t is Pawn && pawn.CanReserve(t) &&
                           pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Hunt) != null &&
                           HasJobOnThing(pawn, t);
            }
        }
        else
        {
            __result = __result && HasJobOnThing(pawn, t);
        }
    }

    private static bool HasJobOnThing(Pawn pawn, Thing t)
    {
        var imparedSight = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < ImpairedSightThresh;
        if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < TerribleSightThresh)
        {
            JobFailReason.Is(TerribleSightMsg.Translate());
            return false;
        }

        if (t is not Pawn pawn2 || !pawn2.AnimalOrWildMan())
        {
            return false;
        }

        var primary = pawn.equipment.Primary;
        var isMeleeWeapon = primary.def.IsMeleeWeapon;
        if (isMeleeWeapon && pawn2.RaceProps.DeathActionWorker != null)
        {
            JobFailReason.Is(MeleeExplodingPrey.Translate());
            return false;
        }

        var rightBodySize = !(pawn2.BodySize < 0.65) && !(pawn2.BodySize > 1.3);
        if (isMeleeWeapon && !Hunting_Loader.settings.shouldMeleeHuntBigGame && pawn2.BodySize > 1.3)
        {
            JobFailReason.Is(MeleeHuntNotEnabled.Translate());
            return false;
        }

        if (isMeleeWeapon && !Hunting_Loader.settings.shouldMeleeHuntMediumGame && rightBodySize)
        {
            JobFailReason.Is(MeleeHuntNotEnabled.Translate());
            return false;
        }

        if (isMeleeWeapon && !Hunting_Loader.settings.shouldMeleeHuntSmallGame && pawn2.BodySize < 0.65)
        {
            JobFailReason.Is(MeleeHuntNotEnabled.Translate());
            return false;
        }

        if (!pawn2.Downed && pawn2.def.race.predator && !Hunting_Loader.settings.shouldHuntPredators)
        {
            JobFailReason.Is(PredatorsNotEnabled.Translate());
            return false;
        }

        if (!pawn2.Awake() && !pawn2.Downed && pawn2.def.race.predator && !isMeleeWeapon &&
            Hunting_Loader.settings.shouldHuntPredators)
        {
            JobFailReason.Is(SleepingPredator.Translate());
            return false;
        }

        var shootingAccuracyTrait = false;
        var namedTrait = GetNamedTrait(pawn, shootingAccuracy);
        if (namedTrait != null)
        {
            shootingAccuracyTrait = namedTrait.Degree == -1;
            _ = namedTrait.Degree == 1;
        }

        if (shootingAccuracyTrait && !isMeleeWeapon && pawn2.BodySize < 0.65)
        {
            JobFailReason.Is(SizeMsg.Translate());
            return false;
        }

        if (imparedSight && !(pawn2.BodySize > 1.3))
        {
            JobFailReason.Is(TerribleSightMsg.Translate());
            return false;
        }

        var intVec = pawn.Position - t.Position;
        var distanceCheck = intVec.LengthManhattan > distFar;
        _ = intVec.LengthManhattan > distVeryFar;
        var pawnHungry = pawn.needs.food != null &&
                         pawn.needs.food.CurLevelPercentage < Hunting_Loader.settings.minimumFoodLevel;
        var pawnTired = pawn.needs.rest != null &&
                        pawn.needs.rest.CurLevelPercentage < Hunting_Loader.settings.minimumSleepLevel;
        var pawnSlowed = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) < movementCapacityBlock;
        var num = 0f;
        foreach (var keyValuePair in pawn.Map.resourceCounter.AllCountedAmounts)
        {
            if (keyValuePair.Key.IsNutritionGivingIngestible && keyValuePair.Key.ingestible.HumanEdible &&
                keyValuePair.Key.ingestible.preferability > FoodPreferability.RawBad)
            {
                num += keyValuePair.Key.ingestible.CachedNutrition * keyValuePair.Value;
            }
        }

        if (!(num < pawn.Map.mapPawns.FreeColonistsCount * 2) && (pawnHungry || pawnTired) && distanceCheck)
        {
            JobFailReason.Is(NeedsMsg.Translate());
            return false;
        }

        if (pawnSlowed && distanceCheck)
        {
            JobFailReason.Is(SlowMsg.Translate());
            return false;
        }

        if (!Hunting_Loader.settings.ignoreTemperature &&
            (pawn.Map.mapTemperature.OutdoorTemp > pawn.SafeTemperatureRange().max ||
             pawn.Map.mapTemperature.OutdoorTemp < pawn.SafeTemperatureRange().min) &&
            (distanceCheck || pawnSlowed) &&
            !(num < pawn.Map.mapPawns.FreeColonistsCount * 2))
        {
            JobFailReason.Is(TemperatureMsg.Translate());
            return false;
        }

        var statValue = pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance);
        if (!pawn.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) ||
            (!(statValue > 0f) || !(intVec.LengthManhattan > distFallout * statValue)) && !pawnSlowed)
        {
            return true;
        }

        JobFailReason.Is(ToxicFalloutMsg.Translate());
        return false;
    }
}