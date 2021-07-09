using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HuntingRestricted
{
    // Token: 0x02000002 RID: 2
    internal class Hunting_Loader : Mod
    {
        // Token: 0x04000001 RID: 1
        private const string ModName = "HRSettingsModName";

        // Token: 0x04000002 RID: 2
        private const string SettingCollectExplodables = "HRSettingShouldCollectExplodables";

        // Token: 0x04000003 RID: 3
        private const string TooltipCollectExplodables = "HRTooltipShouldCollectExplodables";

        // Token: 0x04000004 RID: 4
        private const string SettingMeleeHuntBigGame = "HRSettingShouldMeleeHuntBigGame";

        // Token: 0x04000005 RID: 5
        private const string SettingMeleeHuntMediumGame = "HRSettingShouldMeleeHuntMediumGame";

        // Token: 0x04000006 RID: 6
        private const string SettingMeleeHuntSmallGame = "HRSettingShouldMeleeHuntSmallGame";

        // Token: 0x04000007 RID: 7
        private const string SettingHuntPredators = "HRSettingShouldHuntPredators";

        private const string SettingShouldApprochSleepers = "HRSettingShouldApprochSleepers";

        // Token: 0x04000008 RID: 8
        public static HR_Settings settings;

        // Token: 0x04000009 RID: 9
        private static Harmony harmony;

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public Hunting_Loader(ModContentPack content) : base(content)
        {
            PatchHH();
            settings = GetSettings<HR_Settings>();
        }

        // Token: 0x06000002 RID: 2 RVA: 0x0000206C File Offset: 0x0000026C
        public override string SettingsCategory()
        {
            return ModName.Translate();
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002078 File Offset: 0x00000278
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing_Standard = new Listing_Standard
            {
                ColumnWidth = inRect.width / 1.8f
            };
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled(SettingCollectExplodables.Translate(),
                ref settings.shouldCollectExplodables, TooltipCollectExplodables.Translate());
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled(SettingMeleeHuntSmallGame.Translate(),
                ref settings.shouldMeleeHuntSmallGame);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled(SettingMeleeHuntMediumGame.Translate(),
                ref settings.shouldMeleeHuntMediumGame);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled(SettingMeleeHuntBigGame.Translate(), ref settings.shouldMeleeHuntBigGame);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled(SettingHuntPredators.Translate(), ref settings.shouldHuntPredators);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled(SettingShouldApprochSleepers.Translate(),
                ref settings.shouldApprochSleepers);
            listing_Standard.End();
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002174 File Offset: 0x00000374
        private static void PatchHH()
        {
            harmony = new Harmony("net.marvinkosh.rimworld.mod.huntingrestriction");
            Log.Message("Hunting Restriction: Trying to patch WorkGiver_HunterHunt.ShouldSkip.");
            var method =
                typeof(WorkGiver_HunterHunt).GetMethod("ShouldSkip", BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
            {
                Log.Warning(
                    "Hunting Restriction: Got null original method when attempting to find original WorkGiver_HunterHunt.ShouldSkip.");
            }
            else
            {
                var method2 = typeof(MarvsHuntWhenSane).GetMethod("PostFix");
                if (method2 == null)
                {
                    Log.Warning("Hunting Restriction: Got null method when attempting to load postfix.");
                }
                else
                {
                    harmony.Patch(method, null, new HarmonyMethod(method2));
                    Log.Message("Hunting Restriction: Patched WorkGiver_HunterHunt.ShouldSkip.");
                    Log.Message("Hunting Restriction: Trying to patch WorkGiver_HunterHunt.HasJobOnThing.");
                    method = typeof(WorkGiver_HunterHunt).GetMethod("HasJobOnThing",
                        BindingFlags.Instance | BindingFlags.Public);
                    if (method == null)
                    {
                        Log.Warning(
                            "Hunting Restriction: Got null original method when attempting to find original WorkGiver_HunterHunt.HasJobOnThing.");
                    }
                    else
                    {
                        method2 = typeof(MarvsHuntWhenSane).GetMethod("PostFix2");
                        if (method2 == null)
                        {
                            Log.Warning("Hunting Restriction: Got null method when attempting to load second postfix.");
                        }
                        else
                        {
                            harmony.Patch(method, null, new HarmonyMethod(method2));
                            Log.Message("Hunting Restriction: Patched WorkGiver_HunterHunt.HasJobOnThing.");
                        }
                    }
                }
            }
        }

        // Token: 0x02000005 RID: 5
        public class HR_Settings : ModSettings
        {
            public bool shouldApprochSleepers;

            // Token: 0x04000021 RID: 33
            public bool shouldCollectExplodables;

            // Token: 0x04000025 RID: 37
            public bool shouldHuntPredators;

            // Token: 0x04000024 RID: 36
            public bool shouldMeleeHuntBigGame;

            // Token: 0x04000023 RID: 35
            public bool shouldMeleeHuntMediumGame;

            // Token: 0x04000022 RID: 34
            public bool shouldMeleeHuntSmallGame;

            // Token: 0x06000018 RID: 24 RVA: 0x00002E6C File Offset: 0x0000106C
            public override void ExposeData()
            {
                Scribe_Values.Look(ref shouldCollectExplodables, "b_ShouldCollectExplodables", false, true);
                Scribe_Values.Look(ref shouldMeleeHuntSmallGame, "b_ShouldMeleeHuntSmallGame", false, true);
                Scribe_Values.Look(ref shouldMeleeHuntMediumGame, "b_ShouldMeleeHuntMediumGame", false, true);
                Scribe_Values.Look(ref shouldMeleeHuntBigGame, "b_ShouldMeleeHuntBigGame", false, true);
                Scribe_Values.Look(ref shouldHuntPredators, "b_ShouldHuntPredators", false, true);
                Scribe_Values.Look(ref shouldApprochSleepers, "b_ShouldApprochSleepers", false, true);
            }
        }
    }
}