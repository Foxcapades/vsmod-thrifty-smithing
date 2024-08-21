using ThriftySmithing.Data;
using Vintagestory.GameContent;

namespace ThriftySmithing.Extensions;

internal static class XBlockEntityAnvil {
  internal static WorkData? getWorkData(this BlockEntityAnvil anvil) =>
    (WorkData?) anvil.WorkItemStack?.Attributes?[ThriftySmithing.WorkDataKey];

  internal static void setWorkData(this BlockEntityAnvil anvil, WorkData data) =>
    anvil.WorkItemStack!.Attributes![ThriftySmithing.WorkDataKey] = data;

  internal static void clearWorkData(this BlockEntityAnvil anvil) =>
    anvil.WorkItemStack?.Attributes?.RemoveAttribute(ThriftySmithing.WorkDataKey);
}
