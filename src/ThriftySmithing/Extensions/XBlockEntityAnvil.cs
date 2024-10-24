using System.Collections.Generic;
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

  public static List<(string, ITreeAttribute)> findExtensionData(this BlockEntityAnvil anvil) {
    var attrs = anvil.WorkItemStack?.Attributes;

    if (attrs == null || attrs.Count == 0)
      return new List<(string, ITreeAttribute)>(0);

    var results = new List<(string, ITreeAttribute)>(attrs.Count);

    foreach (var kv in attrs) {
      if (kv.Key.StartsWith(ThriftySmithing.WorkDataModifierPrefix)) {
        if (kv.Value is ITreeAttribute val) {
          results.Add((kv.Key[ThriftySmithing.WorkDataModifierPrefix.Length..], val));
        }
      }
    }

    return results;
  }

  public static void addExtensionData(this BlockEntityAnvil anvil, string key, WorkDataModifiers modifier) =>
    anvil.WorkItemStack!.Attributes![ThriftySmithing.WorkDataModifierPrefix + key] = modifier.toAttribute();
}
