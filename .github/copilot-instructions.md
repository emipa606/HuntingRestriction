# GitHub Copilot Instructions for Hunting Restriction (Continued) Mod

## Mod Overview and Purpose

The Hunting Restriction (Continued) mod is an update to MarvinKoshs original Hunting Restriction mod. It introduces additional settings and behaviors for hunters in the RimWorld game. The mod aims to make hunting more realistic by considering various factors such as toxicity, distance, target size, and environmental conditions. This allows hunters to make more informed decisions about whether to hunt a particular target or not.

## Key Features and Systems

### Decision Making for Hunters
- **Toxic Fallout**: Hunters will consider toxic fallout before and during hunting.
- **Distance to Target**: Hunters will choose a shooting range based on the weapon's maximum range and other factors.
- **Target Size**: Hunters will consider whether the target is big, medium, or small game.
- **Target Behavior**: Hunters will consider whether the target is a predator and whether they are sleeping.
- **Hunter's Needs**: Hunters will prioritize hunting if stocks of food are low, despite their own needs.
- **Sight and Movement Efficiency**: Hunters with poor sight (worse than 55%) will not hunt.
- **Outdoor Temperature**: Hunters will consider the temperature when deciding to hunt.
- **Current Stocks of Edible Nutrition**: Hunters will prioritize hunting if their food stock levels are low.

### Hunting Range Adjustments
- **Weapon Range Selection**: Hunters will choose a shooting range suited to the weapon they have in hand.
- **Safety Range for Explosive Targets**: Hunters will add extra safety range if the target is likely to explode on death.
- **Weather Effects**: Weather (especially fog) will reduce the distance hunters will try to maintain.
- **Trigger Happy Trait**: The Trigger Happy trait will reduce the distance hunters will try to maintain.
- **Asleep Targets**: Hunters will move to close range if the target is asleep.

### Additional Settings
- **Haul Hunting Targets That Explode**: An option to enable or disable hunters hauling explosive targets.
- **Melee Hunting Options**: Options to enable melee hunting for various sizes of game.
- **Hunt Predators**: An option to enable hunting predators (turned off by default).

## Coding Patterns and Conventions

- **Modularity**: The mod is structured into multiple C# files, each handling specific aspects of the mod's functionality.
- **Harmony Patching**: The mod uses Harmony patching to modify existing RimWorld code.
- **Settings Management**: The mod includes a settings loader (`Hunting_Loader`) to manage user-defined settings.
- **Decision Logic**: Decision-making logic for hunters is encapsulated in classes like `WorkGiver_HunterHunt_ShouldSkip` and `WorkGiver_HunterHunt_HasJobOnThing`.
- **Range Calculation**: Range calculation logic is encapsulated in the `RangeSelection` class.

## XML Integration

The mod includes an XML file (`Patched_JobDriver_Hunt.xml`) that contains Harmony patches for the `JobDriver_Hunt` class. These patches modify the behavior of the hunting job in RimWorld to incorporate the additional decision-making logic.

### Example XML Patch
xml
<Patch>
  <Operation Class="HarmonyLib.CodeInstructionTranspiler">
    <Method>
      <ClassType>Verse.JobDriver_Hunt</ClassType>
      <MethodName>GetFinalTarget</MethodName>
    </Method>
    <Transpiler>
      <ClassType>HuntingRestricted.RangeSelection</ClassType>
      <MethodName>getGoodWeaponHuntingRange</MethodName>
    </Transpiler>
  </Operation>
</Patch>


## Harmony Patching

The mod uses Harmony patching to modify the behavior of RimWorld's existing classes. Here are the key harmony patches used in the mod:

### Example Harmony Patch
csharp
[HarmonyPatch(typeof(JobDriver_Hunt), "GetFinalTarget")]
public static class Patch_JobDriver_Hunt_GetFinalTarget
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        // Modify the instructions as needed
        return codes;
    }
}


## Suggestions for Copilot

1. **XML Patches**: Use Copilot to generate XML patches for Harmony, ensuring that the patches are correctly structured and target the right methods.
2. **Decision Logic**: Use Copilot to generate decision-making logic for hunters, considering various factors such as toxicity, distance, target size, and environmental conditions.
3. **Range Calculation**: Use Copilot to generate range calculation logic, taking into account factors such as weapon range, safety distance, and weather conditions.
4. **Settings Management**: Use Copilot to generate code for managing user-defined settings, ensuring that the settings are correctly loaded and saved.
5. **Harmony Patching**: Use Copilot to generate Harmony patches, ensuring that the patches are correctly applied and do not interfere with the original code.

## Additional Resources

- **Ludeon Forums**: For more information and direct download links: [Ludeon Forums](https://ludeon.com/forums/index.php/topic,34618.0.html)
- **Discord Channel**: For error-reporting and support: [Discord Channel](https://discord.gg/rimworld)

By following these instructions and using Copilot to assist with coding tasks, you can efficiently develop and maintain the Hunting Restriction (Continued) mod for RimWorld.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

