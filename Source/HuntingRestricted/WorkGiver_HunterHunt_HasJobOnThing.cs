using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HuntingRestricted;

[HarmonyPatch(typeof(WorkGiver_HunterHunt), nameof(WorkGiver_HunterHunt.HasJobOnThing))]
public static class WorkGiver_HunterHunt_HasJobOnThing
{
    public static void Postfix(ref bool __result, Pawn pawn, Thing t)
    {
        var primary = pawn.equipment.Primary;
        if (primary.def.IsMeleeWeapon)
        {
            if (Hunting_Loader.Settings.ShouldMeleeHuntBigGame ||
                Hunting_Loader.Settings.ShouldMeleeHuntMediumGame ||
                Hunting_Loader.Settings.ShouldMeleeHuntSmallGame)
            {
                __result = t is Pawn && pawn.CanReserve(t) &&
                           pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Hunt) != null &&
                           MarvsHuntWhenSane.HasJobOnThing(pawn, t);
            }
        }
        else
        {
            __result = __result && MarvsHuntWhenSane.HasJobOnThing(pawn, t);
        }
    }
}