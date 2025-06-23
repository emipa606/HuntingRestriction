using RimWorld;
using Verse;
using Verse.AI;

namespace HuntingRestricted;

public class MarvsHuntWhenSane
{
    public const float TerribleSightThresh = 0.55f;

    private const float ImpairedSightThresh = 0.85f;

    private const float MovementCapacityBlock = 0.5f;

    private const int DistFar = 35;

    private const int DistVeryFar = 150;

    private const int DistFallout = 50;

    private const string ShootingAccuracy = "ShootingAccuracy";

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

    public static Trait GetNamedTrait(Pawn pawn, string traitName)
    {
        var allTraits = pawn.story.traits.allTraits;
        foreach (var getNamedTrait in allTraits)
        {
            if (getNamedTrait.def.defName == traitName)
            {
                return getNamedTrait;
            }
        }

        return null;
    }

    public static bool HasJobOnThing(Pawn pawn, Thing t)
    {
        var impairedSight = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < ImpairedSightThresh;
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
        if (isMeleeWeapon && !Hunting_Loader.Settings.ShouldMeleeHuntBigGame && pawn2.BodySize > 1.3 ||
            isMeleeWeapon && !Hunting_Loader.Settings.ShouldMeleeHuntMediumGame && rightBodySize || isMeleeWeapon &&
            !Hunting_Loader.Settings.ShouldMeleeHuntSmallGame && pawn2.BodySize < 0.65)
        {
            JobFailReason.Is(MeleeHuntNotEnabled.Translate());
            return false;
        }

        if (!pawn2.Downed && pawn2.def.race.predator && !Hunting_Loader.Settings.ShouldHuntPredators)
        {
            JobFailReason.Is(PredatorsNotEnabled.Translate());
            return false;
        }

        if (!pawn2.Awake() && !pawn2.Downed && pawn2.def.race.predator && !isMeleeWeapon &&
            Hunting_Loader.Settings.ShouldHuntPredators)
        {
            JobFailReason.Is(SleepingPredator.Translate());
            return false;
        }

        var shootingAccuracyTrait = false;
        var namedTrait = GetNamedTrait(pawn, ShootingAccuracy);
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

        if (impairedSight && !(pawn2.BodySize > 1.3))
        {
            JobFailReason.Is(TerribleSightMsg.Translate());
            return false;
        }

        var intVec = pawn.Position - t.Position;
        var distanceCheck = intVec.LengthManhattan > DistFar;
        _ = intVec.LengthManhattan > DistVeryFar;
        var pawnHungry = pawn.needs.food != null &&
                         pawn.needs.food.CurLevelPercentage < Hunting_Loader.Settings.MinimumFoodLevel;
        var pawnTired = pawn.needs.rest != null &&
                        pawn.needs.rest.CurLevelPercentage < Hunting_Loader.Settings.MinimumSleepLevel;
        var pawnSlowed = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) < MovementCapacityBlock;
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

        if (!Hunting_Loader.Settings.IgnoreTemperature &&
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
            (!(statValue > 0f) || !(intVec.LengthManhattan > DistFallout * statValue)) && !pawnSlowed)
        {
            return true;
        }

        JobFailReason.Is(ToxicFalloutMsg.Translate());
        return false;
    }
}