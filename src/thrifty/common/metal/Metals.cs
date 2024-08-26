using System.Collections.Generic;
using thrifty.Data;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace thrifty.metal;

internal static class Metals {
  private static readonly AssetLocation MetalBitWildcard = new("*", Const.DefaultMetalBitPathPrefix + "-*");

  private static ISet<string> knownMetals = new HashSet<string>();

  internal static bool isMetal(this ItemStack stack) =>
    stack.Item.Variant;

  internal static void thunk(IWorldAccessor world) {
    foreach (var item in world.Items) {
      item.Tool
    }

    if (world is IServerWorldAccessor)

    world.SearchItems()
  }
}
