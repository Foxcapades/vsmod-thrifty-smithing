using ThriftySmithing.Data;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ThriftySmithing.Utils;

public static class BitGen {

  internal static void issueBits(int count, BlockEntityAnvil anvil) {
    if (count < 1)
      return;

    var world = anvil.Api.World;
    var item  = world.GetItem(getBitCode(anvil));

    var stack = new ItemStack(item, count);

    applyTemperature(stack, anvil);

    if (item != null)
      world.SpawnItemEntity(stack, anvil.Pos.ToVec3d());
  }

  internal static void issueBits(int count, IPlayer player, BlockEntityAnvil anvil) {
    if (count < 1)
      return;

    var world = player.Entity.World;
    var item = world.GetItem(getBitCode(anvil));

    if (item == null)
      return;

    var stack = new ItemStack(item, count);

    applyTemperature(stack, anvil);

    if (!player.InventoryManager.TryGiveItemstack(stack, true))
      world.SpawnItemEntity(stack, player.Entity.Pos.XYZ);
  }

  private static AssetLocation getBitCode(BlockEntityAnvil anvil) {
    var input = anvil.SelectedRecipe.Ingredient.Code!;
    return new(input.Domain, Paths.makePathOf(Const.DefaultMetalBitPathPrefix, Paths.lastPathEntry(input)));
  }

  private static void applyTemperature(ItemStack stack, BlockEntityAnvil anvil) {
    var temp = anvil.WorkItemStack.Attributes.TryGetFloat(Const.TemperatureAttributeKey);

    if (temp.HasValue)
      stack.Attributes[Const.TemperatureAttributeKey] = new FloatAttribute(temp.Value);
  }
}
