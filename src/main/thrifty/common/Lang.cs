namespace thrifty.common;

internal static class Lang {
  internal static class Feat {
    internal static class SmithingScrap {
      internal static class RecipeList {
        internal static class HoverHint {
          internal static readonly Key NeedMore = new("thriftysmithing:ZTIc02-P");
          internal static readonly Key NoLoss   = new("thriftysmithing:xrs9F9Z8");
          internal static readonly Key SomeLoss = new("thriftysmithing:SgnVvN3r");
        }
      }
    }
  }

  internal readonly record struct Key(string key) {
    internal string resolve(params object[] formatValues) => Vintagestory.API.Config.Lang.Get(key, formatValues);
  }
}
