using thrifty.common.data;
using Vintagestory.API.Common;

namespace thrifty.common.x;

internal static class XAPI {
  internal static Item? getItem(this ICoreAPI api, string path) =>
    api.getItem(new AssetLocation(Const.Code.Default.Domain, path));

  internal static Item? getItem(this ICoreAPI api, AssetLocation location) =>
    api.World.GetItem(location);

  internal static Block? getBlock(this ICoreAPI api, string path) =>
    api.getBlock(new AssetLocation(Const.Code.Default.Domain, path));

  internal static Block? getBlock(this ICoreAPI api, AssetLocation location) =>
    api.World.GetBlock(location);
}
