using Vintagestory.GameContent;

using thrifty.feature.smithing_scrap.data;

namespace thrifty.feature.smithing_scrap.ext;

internal static class XBlockEntityAnvil {
  internal static WorkData? getWorkData(this BlockEntityAnvil anvil) =>
    (WorkData?) anvil.WorkItemStack?.Attributes?[WorkData.AttributeKey];

  internal static void setWorkData(this BlockEntityAnvil anvil, WorkData data) =>
    anvil.WorkItemStack!.Attributes![WorkData.AttributeKey] = data;

  internal static void clearWorkData(this BlockEntityAnvil anvil) =>
    anvil.WorkItemStack?.Attributes?.RemoveAttribute(WorkData.AttributeKey);
}
