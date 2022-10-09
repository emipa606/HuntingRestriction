using System.Reflection;
using HarmonyLib;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace HuntingRestricted;

internal class Hunting_Loader : Mod
{
    private const string ModName = "HRSettingsModName";

    private const string SettingCollectExplodables = "HRSettingShouldCollectExplodables";

    private const string TooltipCollectExplodables = "HRTooltipShouldCollectExplodables";

    private const string SettingMeleeHuntBigGame = "HRSettingShouldMeleeHuntBigGame";

    private const string SettingMeleeHuntMediumGame = "HRSettingShouldMeleeHuntMediumGame";

    private const string SettingMeleeHuntSmallGame = "HRSettingShouldMeleeHuntSmallGame";

    private const string SettingModVersion = "HRSettingModVersion";

    private const string SettingHuntPredators = "HRSettingShouldHuntPredators";

    private const string SettingShouldApprochSleepers = "HRSettingShouldApprochSleepers";

    public static HR_Settings settings;

    private static Harmony harmony;

    private static string currentVersion;

    public Hunting_Loader(ModContentPack content) : base(content)
    {
        PatchHH();
        settings = GetSettings<HR_Settings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(
                ModLister.GetActiveModWithIdentifier("Mlie.HuntingRestriction"));
    }

    public override string SettingsCategory()
    {
        return ModName.Translate();
    }

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
        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label(SettingModVersion.Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }

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

    public class HR_Settings : ModSettings
    {
        public bool shouldApprochSleepers;

        public bool shouldCollectExplodables;

        public bool shouldHuntPredators;

        public bool shouldMeleeHuntBigGame;

        public bool shouldMeleeHuntMediumGame;

        public bool shouldMeleeHuntSmallGame;

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