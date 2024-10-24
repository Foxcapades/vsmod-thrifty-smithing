using System.Collections.Generic;
using Vintagestory.GameContent;

namespace ThriftySmithing.Data;

/**
 * <summary>
 * Data access facade and utility methods.
 * </summary>
 */
public static class Smithy {

  #region Config-Based Values
  // Config-based values are values that are from or are derived solely from the
  // mod's configuration.

  public static int VoxelsPerIngot => ThriftySmithing.Config.voxelsPerIngot;

  public static int VoxelsPerPlate => ThriftySmithing.Config.voxelsPerPlate;

  public static int MaterialUnitsPerIngot => ThriftySmithing.Config.materialUnitsPerIngot;

  public static float MaterialUnitsPerVoxel => MaterialUnitsPerIngot / (float) VoxelsPerIngot;

  public static float MaterialUnitsPerPlate => VoxelsPerPlate * MaterialUnitsPerVoxel;

  public static int MaterialUnitsPerBit => ThriftySmithing.Config.materialUnitsPerBit;

  public static float MaterialUnitsRecoveredModifier => (float) ThriftySmithing.Config.materialUnitsRecoveredModifier;

  internal static bool recipeIsAllowed(SmithingRecipe recipe) =>
    recipe.Output != null
    && !ThriftySmithing.Config.disallowedRecipes.Contains(recipe.Output.Code.ToString());

  #endregion Config-Based Values

  #region Recipe Cache

  private static readonly Dictionary<string, ushort> voxelCounts = new();

  internal static int getVoxelCount(SmithingRecipe recipe) {
    if (recipe.Output == null)
      return 0;

    var key = recipe.Output.Code.ToString();

    if (voxelCounts.TryGetValue(key, out var count))
      return count;

    foreach (var voxel in recipe.Voxels)
      if (voxel)
        count++;

    voxelCounts[key] = count;

    return count;
  }

  #endregion Recipe Cache

  #region Calculations
  // Calculations used when determining the amount of material to return to the
  // player after smithing has ended.

  /// <summary>
  /// Calculates the amount of voxels that would be wasted for the given inputs.
  /// </summary>
  ///
  /// <param name="data">
  /// A <c>WorkData</c> instance containing details about the inputs that went
  /// onto the anvil.
  /// </param>
  ///
  /// <param name="recipe">
  /// Smithing recipe that is being crafted.  This value is used to determine
  /// the number of voxels actually used by the recipe.
  /// </param>
  ///
  /// <returns>
  /// The number of voxels that would be wasted on completion of the recipe.
  /// </returns>
  public static int calculateWasteVoxels(WorkData data, SmithingRecipe recipe) =>
    calculateInputVoxels(data) - getVoxelCount(recipe);

  public static int calculateWasteReturnBits(int voxels) =>
    (int) (calculateWasteMaterial(voxels) / MaterialUnitsPerBit);

  private static int calculateTotalInputBits(WorkData data) =>
    (int) (calculateTotalInputMaterial(data) / MaterialUnitsPerBit);

  private static int calculateInputVoxels(WorkData data) =>
    data.ingotCount * VoxelsPerIngot + data.plateCount * VoxelsPerPlate;

  private static float calculateWasteMaterial(int voxels) =>
    voxels * MaterialUnitsPerVoxel * MaterialUnitsRecoveredModifier;

  private static float calculateTotalInputMaterial(WorkData data) =>
    data.ingotCount * MaterialUnitsPerIngot + data.plateCount * MaterialUnitsPerPlate;

  #endregion Calculations
}
