using thrifty.debug;
using Vintagestory.GameContent;
using thrifty.feat.smithing.data;

namespace thrifty.common.x;

internal static class XAnvil {
  internal static WorkData? getWorkData(this BlockEntityAnvil anvil) {
    var ret = (WorkData?) anvil.WorkItemStack?.Attributes?[ThriftySmithing.WorkDataKey];
    Logs.debug("fetched work-data state {0}", ret.HasValue ? ret.Value : "null");
    return ret;
  }

  internal static void setWorkData(this BlockEntityAnvil anvil, WorkData data) {
    Logs.debug("saving work-data state {0}", data);
    anvil.WorkItemStack!.Attributes![ThriftySmithing.WorkDataKey] = data;
  }

  internal static void clearWorkData(this BlockEntityAnvil anvil) {
    Logs.debug("clearing work-data state");
    anvil.WorkItemStack?.Attributes?.RemoveAttribute(ThriftySmithing.WorkDataKey);
  }
}
