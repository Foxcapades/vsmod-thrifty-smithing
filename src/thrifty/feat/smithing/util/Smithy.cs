using System.Collections.Generic;
using Vintagestory.GameContent;
using thrifty.feat.smithing.data;

namespace thrifty.feat.smithing.util;

/**
 * <summary>
 * Data access facade and utility methods.
 * </summary>
 */
internal static class Smithy {

  #region Config-Based Values
  // Config-based values are values that are from or are derived solely from the
  // mod's configuration.

  public static int VoxelsPerIngot => ThriftySmithing.Config.voxelsPerIngot;

  public static int VoxelsPerPlate => ThriftySmithing.Config.voxelsPerPlate;

  public static int MaterialUnitsPerIngot => ThriftySmithing.Config.materialUnitsPerIngot;

  public static float MaterialUnitsPerVoxel => MaterialUnitsPerIngot / (float) VoxelsPerIngot;

  public static float MaterialUnitsPerPlate => VoxelsPerPlate * MaterialUnitsPerVoxel;

  public static int MaterialUnitsPerBit => ThriftySmithing.Config.materialUnitsPerBit;

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

  internal static int calculateScrap(ushort voxelCount) =>
    (int) (voxelCount * MaterialUnitsPerVoxel / MaterialUnitsPerBit);

  internal static int calculateWasteReturnBits(WorkData data, SmithingRecipe recipe) =>
    (int) (calculateWasteMaterial(data, recipe) / MaterialUnitsPerBit);

  internal static int calculateTotalInputBits(WorkData data) =>
    (int) (calculateTotalInputMaterial(data) / MaterialUnitsPerBit);

  private static int calculateInputVoxels(WorkData data) =>
    data.ingotCount * VoxelsPerIngot + data.plateCount * VoxelsPerPlate;

  private static int calculateWasteVoxels(WorkData data, SmithingRecipe recipe) =>
    calculateInputVoxels(data) - getVoxelCount(recipe);

  private static float calculateWasteMaterial(WorkData data, SmithingRecipe recipe) =>
    calculateWasteVoxels(data, recipe) * MaterialUnitsPerVoxel;

  private static float calculateTotalInputMaterial(WorkData data) =>
    data.ingotCount * MaterialUnitsPerIngot + data.plateCount * MaterialUnitsPerPlate;

  #endregion Calculations
}