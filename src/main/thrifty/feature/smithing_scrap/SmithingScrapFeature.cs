using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

using thrifty.common.ext;
using thrifty.feature.smithing_scrap.data;
using thrifty.feature.smithing_scrap.hax;

namespace thrifty.feature.smithing_scrap;

internal static class SmithingScrapFeature {
  /// <summary>
  /// Key that configuration specific to this feature will be kept under in the
  /// mod config file.
  /// </summary>
  internal const string FeatureConfigKey = "smithing-scrap-recovery";

  /// <summary>
  /// Indicates whether this feature's patch has been applied.
  /// </summary>
  private static bool patched;

  /// <summary>
  /// The ID of the <code>WorkData</code> attribute registered by this feature.
  /// </summary>
  internal static byte attributeID;

  /// <summary>
  /// Registers this feature and its components.
  /// </summary>
  ///
  /// <param name="harmony">
  /// Harmony instance that will be used to patch core game behaviors.
  /// </param>
  ///
  /// <param name="api">
  /// VS API instance.
  /// </param>
  internal static void register(Harmony harmony, ICoreAPI api) {
    if (patched)
      return;

    // Register the attribute type regardless of whether we are server or client
    // side as the data will unfortunately be synchronized between the two.
    attributeID = XTreeAttribute.registerType(typeof(WorkData));

    // Only register the harmony patches on the server side.
    if (api is ICoreServerAPI) {
      BlockAnvilHax.patch(harmony);
    }

    BlockEntityAnvilHax.patch(harmony, api.Side);

    patched = true;
  }

  /// <summary>
  /// Deregisters this feature and its components.
  /// </summary>
  internal static void deregister(Harmony harmony, ICoreAPI api) {
    // Harmony patches should only be registered on the server side.
    if (patched) {
      if (api is ICoreServerAPI) {
        BlockEntityAnvilHax.unpatch(harmony);
      }

      BlockAnvilHax.unpatch(harmony);

      patched = false;
    }
  }
}
