using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace thrifty.debug.commands;

internal abstract class DumbCommand {

  protected abstract string name { get; }

  protected string? description;

  protected abstract ICommandArgumentParser[] arguments { get; }

  protected abstract TextCommandResult run(TextCommandCallingArgs args);

  internal void register(ICoreServerAPI api) {
    var com = api.ChatCommands.Create(name)
      .WithArgs(arguments)
      .HandleWith(run);

    if (description != null)
      com.WithDescription(description);
  }
}
