# Copilot Instructions for Hunting Restriction (Continued) Mod

## Mod Overview and Purpose
The "Hunting Restriction (Continued)" mod is designed to enhance the decision-making process of hunters in RimWorld. Originally created by MarvinKosh and updated with permission, this mod aims to simulate realistic hunting behaviors by considering environmental and personal factors before engaging in hunting activities.

## Key Features and Systems
- **Environmental and Personal Factor Consideration**: Hunters evaluate various conditions such as toxic fallout, distance to target, prey size, and personal needs (like food and rest) before deciding to hunt.
- **Dynamic Shooting Distances**: Shooting ranges are adjusted based on weapon type, prey status (e.g., downed or sleeping), weather conditions, and character traits like "Trigger Happy".
- **Explosive Prey Handling**: The mod introduces an option for how hunters handle explosive prey like boomalope or boomrat, featuring settings to manage these scenarios and avoid unnecessary injuries.
- **Translatability**: The mod supports translation, making it available to a broader audience.
- **Enhanced Melee Hunting**: New settings allow for melee hunting based on prey size and preferencing non-predator targets unless specifically enabled.

## Coding Patterns and Conventions
- Maintain clean and consistent coding practices following C# conventions for readability and maintainability. 
- Use type-specific patterns where files are organized based on functionality such as hunting decisions, range selection, and job assignments.

## XML Integration
- XML files are utilized for patching game behaviors, such as `Patched_JobDriver_Hunt.xml`, to integrate mod functionalities seamlessly with the vanilla game.
- Ensure to clearly define XML nodes, attributes, and values for precise in-game behavior adjustments.

## Harmony Patching
- Utilize Harmony library (`brrainz.harmony`) to patch existing game methods without altering the core game files directly.
- Use postfixes to extend or modify behaviors efficiently as seen in `WorkGiver_HunterHunt_HasJobOnThing.cs` and `WorkGiver_HunterHunt_ShouldSkip.cs`.

## Suggestions for Copilot
1. **Code Completion Assistance**: Utilize Copilot for auto-completing repetitive code blocks, especially when dealing with XML file configurations and C# class definitions.
2. **Pattern Recognition**: Leverage Copilot's ability to recognize patterns in your mod structure and suggest consistent naming for classes and methods.
3. **Refactoring Aid**: Use Copilot to assist in refactoring complex methods for better performance and readability, particularly those involved in decision-making processes.
4. **Error Handling Suggestions**: Seek guidance from Copilot on implementing robust error handling to ensure the mod does not cause game instability.
5. **Documentation**: Use Copilot to help generate code comments and documentation for easier project maintenance and understanding by other contributors.

By following these detailed guidelines, developers and contributors can ensure that the "Hunting Restriction (Continued)" mod is developed with high-quality standards and integrates effectively with RimWorld's ecosystem.

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

