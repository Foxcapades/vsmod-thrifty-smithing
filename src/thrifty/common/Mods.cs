using System.Collections.Generic;
using System.Linq;
using thrifty.common.utils;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace thrifty.common;

internal static class Mods {
  internal static IEnumerable<SmithingRecipe> getSmithingRecipes(this ICoreAPI api) =>
    api.ModLoader.GetModSystem<RecipeRegistrySystem>()?.SmithingRecipes ?? Enumerable.Empty<SmithingRecipe>();

  internal static IEnumerable<SmithingRecipe> getValidSmithingRecipes(this ICoreAPI api) {
    foreach (var recipe in api.getSmithingRecipes())
      if (recipe.isValid())
        yield return recipe;
      else
        yield break;
  }
}
