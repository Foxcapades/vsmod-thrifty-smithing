using thrifty.common.data;
using thrifty.common.x;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace thrifty.debug.commands;

internal class HotLava : DumbCommand {

  protected override string name => "hot-lava";

  protected override ICommandArgumentParser[] arguments => new ICommandArgumentParser[] {
    new StringArgParser("metal", true),
    new IntArgParser("units", 100, false),
    new FloatArgParser("temp", 5000, false)
  };

  internal HotLava() {
    description = "Gives a crucible of something hot.";
  }

  protected override TextCommandResult run(TextCommandCallingArgs args) {
    if (args.Caller.Player == null)
      return TextCommandResult.Error("this command can only be used by players");

    var world = args.Caller.Entity.World;

    var ingot = world.GetItem(new AssetLocation(
      Const.Code.Default.Domain,
      $"{Const.Code.Default.IngotPathPrefix}-{args[0]}"
    ));

    if (ingot == null)
      return TextCommandResult.Error($"could not find an ingot for material \"{args[0]}\"");

    args.Caller.Player.give(new(world.GetBlock(new AssetLocation(Const.Code.Default.Domain, "crucible-smelted"))) {
      Attributes = {
        [Const.Attribute.Output] = new ItemstackAttribute(new ItemStack(ingot)),
        [Const.Attribute.Units] = new IntAttribute((int) args[1]),
        [Const.Attribute.Temperature] = new TreeAttribute {
          [Const.Attribute.Temperature] = new FloatAttribute((float) args[2]),
        },
      },
    });

    return TextCommandResult.Success();
  }
}
