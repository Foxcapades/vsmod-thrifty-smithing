using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace thrifty.common.utils;

internal static class Recipes {
  internal static bool isValid(this SmithingRecipe recipe) {
    return recipe.Ingredient?.Code != null
      && recipe.Ingredient.Quantity > 0
      && recipe.Ingredient.Type == EnumItemClass.Item
      && recipe.Output?.Code != null
      && recipe.Output.Quantity > 0;
  }

  internal static ushort voxelCount(this SmithingRecipe recipe) {
    ushort count = 0;

    foreach (var voxel in recipe.Voxels)
      if (voxel)
        count++;

    return count;
  }
}
