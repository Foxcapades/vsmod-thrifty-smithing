using System.Collections.Generic;
using thrifty.debug;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace thrifty.common.data;

internal static class Cache {
  private static readonly Dictionary<AssetKey, ushort> itemToVoxels = new();

  // TODO: instead of registering the voxel value, register the material value?
  //       This would need to be calculated based on bit value for the material
  //       and ingot size and such.
  internal static void registerVoxelValue(Item item, ushort voxels) {
    Logs.trace("registering item voxel count {0} = {1}", item.Code, voxels);
    itemToVoxels[item.Code] = voxels;
  }

  internal static void registerVoxelValue(AssetKey key, ushort voxels) {
    Logs.trace("registering asset voxel count {0} = {1}", key, voxels);
    itemToVoxels[key] = voxels;
  }

  internal static bool hasKnownVoxelValue(this CollectibleObject obj) => itemToVoxels.ContainsKey(obj.Code);

  internal static void registerSmithingRecipe(Item result, SmithingRecipe recipe, string material, ushort voxels) {

  }
}
