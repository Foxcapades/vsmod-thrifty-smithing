using Vintagestory.API.Common;

namespace thrifty.feat.casting;

internal static class Casting {
  internal static AssetLocation[] moldLocations() =>
    new[] { new AssetLocation("*", "toolmold-burned") };
}
