using System.Collections.Generic;

namespace thrifty.common.data;

internal static class RecipeDataCache {
  private static readonly Dictionary<AssetRef, RecipeData> Cache = new();

  public static RecipeData? lookup(AssetRef key) => Cache.TryGetValue(key, out var value) ? value : null;

  public static void register(AssetRef key, RecipeData data) => Cache[key] = data;
}
