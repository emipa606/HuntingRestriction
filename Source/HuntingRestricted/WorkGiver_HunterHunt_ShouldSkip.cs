using HarmonyLib;
using RimWorld;
using Verse;

namespace HuntingRestricted;

[HarmonyPatch(typeof(WorkGiver_HunterHunt), nameof(WorkGiver_HunterHunt.ShouldSkip))]
public static class WorkGiver_HunterHunt_ShouldSkip
{
    public static void Postfix(ref bool __result, Pawn pawn)
    {
        if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < MarvsHuntWhenSane.TerribleSightThresh)
        {
            __result = true;
        }
        else
        {
            var primary = pawn.equipment.Primary;
            if (primary != null && primary.def.IsMeleeWeapon && (Hunting_Loader.Settings.ShouldMeleeHuntBigGame ||
                                                                 Hunting_Loader.Settings
                                                                     .ShouldMeleeHuntMediumGame ||
                                                                 Hunting_Loader.Settings.ShouldMeleeHuntSmallGame))
            {
                __result = false;
            }
        }
    }
}