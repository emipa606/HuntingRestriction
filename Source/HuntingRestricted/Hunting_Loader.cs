using System.Reflection;
using HarmonyLib;
using Mlie;
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

    private const string SettingMinimumFoodLevel = "HRSettingMinimumFoodLevel";

    private const string SettingMinimumSleepLevel = "HRSettingMinimumSleepLevel";

    private const string SettingIgnoreTemperature = "HRSettingIgnoreTemperature";

    private const string SettingNeedsTitle = "HRSettingNeedsTitle";

    public static HR_Settings Settings;

    private static string currentVersion;

    public Hunting_Loader(ModContentPack content) : base(content)
    {
        new Harmony("net.marvinkosh.rimworld.mod.huntingrestriction").PatchAll(Assembly.GetExecutingAssembly());
        Settings = GetSettings<HR_Settings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return ModName.Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard
        {
            ColumnWidth = inRect.width / 1.8f
        };
        listingStandard.Begin(inRect);
        listingStandard.CheckboxLabeled(SettingCollectExplodables.Translate(),
            ref Settings.ShouldCollectExplodables, TooltipCollectExplodables.Translate());
        listingStandard.Gap();
        listingStandard.CheckboxLabeled(SettingMeleeHuntSmallGame.Translate(),
            ref Settings.ShouldMeleeHuntSmallGame);
        listingStandard.Gap();
        listingStandard.CheckboxLabeled(SettingMeleeHuntMediumGame.Translate(),
            ref Settings.ShouldMeleeHuntMediumGame);
        listingStandard.Gap();
        listingStandard.CheckboxLabeled(SettingMeleeHuntBigGame.Translate(), ref Settings.ShouldMeleeHuntBigGame);
        listingStandard.Gap();
        listingStandard.CheckboxLabeled(SettingHuntPredators.Translate(), ref Settings.ShouldHuntPredators);
        listingStandard.Gap();
        listingStandard.CheckboxLabeled(SettingShouldApprochSleepers.Translate(),
            ref Settings.ShouldApproachSleepers);

        listingStandard.Gap();
        listingStandard.CheckboxLabeled(SettingIgnoreTemperature.Translate(),
            ref Settings.IgnoreTemperature);

        listingStandard.Gap();
        listingStandard.Label(SettingNeedsTitle.Translate());
        Settings.MinimumFoodLevel = listingStandard.SliderLabeled(
            SettingMinimumFoodLevel.Translate(Settings.MinimumFoodLevel.ToStringPercent()), Settings.MinimumFoodLevel,
            0f, 1f);
        listingStandard.Gap();
        Settings.MinimumSleepLevel = listingStandard.SliderLabeled(
            SettingMinimumSleepLevel.Translate(Settings.MinimumSleepLevel.ToStringPercent()),
            Settings.MinimumSleepLevel,
            0f, 1f);
        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label(SettingModVersion.Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }

    public class HR_Settings : ModSettings
    {
        public bool IgnoreTemperature;
        public float MinimumFoodLevel = 0.4f;

        public float MinimumSleepLevel = 0.3f;

        public bool ShouldApproachSleepers;

        public bool ShouldCollectExplodables;

        public bool ShouldHuntPredators;

        public bool ShouldMeleeHuntBigGame;

        public bool ShouldMeleeHuntMediumGame;

        public bool ShouldMeleeHuntSmallGame;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MinimumFoodLevel, "b_MinimumFoodLevel", 0.4f, true);
            Scribe_Values.Look(ref MinimumSleepLevel, "b_MinimumSleepLevel", 0.3f, true);
            Scribe_Values.Look(ref IgnoreTemperature, "b_IgnoreTemperature", false, true);
            Scribe_Values.Look(ref ShouldCollectExplodables, "b_ShouldCollectExplodables", false, true);
            Scribe_Values.Look(ref ShouldMeleeHuntSmallGame, "b_ShouldMeleeHuntSmallGame", false, true);
            Scribe_Values.Look(ref ShouldMeleeHuntMediumGame, "b_ShouldMeleeHuntMediumGame", false, true);
            Scribe_Values.Look(ref ShouldMeleeHuntBigGame, "b_ShouldMeleeHuntBigGame", false, true);
            Scribe_Values.Look(ref ShouldHuntPredators, "b_ShouldHuntPredators", false, true);
            Scribe_Values.Look(ref ShouldApproachSleepers, "b_ShouldApprochSleepers", false, true);
        }
    }
}