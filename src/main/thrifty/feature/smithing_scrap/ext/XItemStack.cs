using System;
using thrifty.common;
using thrifty.common.util;
using thrifty.feature.smithing_scrap.data;
using thrifty.feature.smithing_scrap.hax;
using Vintagestory.API.Common;

namespace thrifty.feature.smithing_scrap.ext;

internal static class XItemStack {
  internal static IngredientType typeOf(this ItemStack item) =>
    item.Item?.typeOf() ?? IngredientType.Irrelevant;
  
  internal static int grantedVoxelCount(this ItemStack item) =>
    (item.Item?.grantedVoxelCount() ?? 0) * item.StackSize;
}

internal static class XItem {
  internal static IngredientType typeOf(this Item item) =>
    Paths.firstPathEntry(item.Code) switch {
      Const.DefaultIngotPathPrefix => IngredientType.Ingot,
      Const.DefaultMetalPlatePathPrefix => IngredientType.Plate,
      _ => IngredientType.Irrelevant,
    };

  internal static int grantedVoxelCount(this Item item) =>
    typeOf(item) switch {
      IngredientType.Ingot => Smithy.VoxelsPerIngot,
      IngredientType.Plate => Smithy.VoxelsPerPlate,
      _ => 0,
    };
}
