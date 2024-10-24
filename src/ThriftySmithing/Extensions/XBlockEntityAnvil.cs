using ThriftySmithing.Data;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ThriftySmithing.Extensions;

public static class XBlockEntityAnvil {
   public static WorkData? getWorkData(this BlockEntityAnvil anvil) =>
    (WorkData?) anvil.WorkItemStack?.Attributes?[ThriftySmithing.WorkDataKey];

  public static void setWorkData(this BlockEntityAnvil anvil, WorkData data) =>
    anvil.WorkItemStack!.Attributes![ThriftySmithing.WorkDataKey] = data;

  public static void clearWorkData(this BlockEntityAnvil anvil) =>
    anvil.WorkItemStack?.Attributes?.RemoveAttribute(ThriftySmithing.WorkDataKey);

  public static ITreeAttribute? getExtensionData(this BlockEntityAnvil anvil) =>
    (ITreeAttribute?) anvil.WorkItemStack?.Attributes?[ThriftySmithing.WorkDataModifierKey];

  public static void setExtensionData(this BlockEntityAnvil anvil, ITreeAttribute data) =>
    anvil.WorkItemStack!.Attributes![ThriftySmithing.WorkDataModifierKey] = data;

  public static void setExtensionData(this BlockEntityAnvil anvil, WorkDataModifiers modifier) =>
    anvil.WorkItemStack!.Attributes![ThriftySmithing.WorkDataModifierKey] = modifier.toAttribute();

  public static void clearExtensionData(this BlockEntityAnvil anvil) =>
    anvil.WorkItemStack?.Attributes?.RemoveAttribute(ThriftySmithing.WorkDataModifierKey);
}
