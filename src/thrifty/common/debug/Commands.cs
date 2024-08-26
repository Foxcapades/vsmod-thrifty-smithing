using thrifty.debug.commands;
using Vintagestory.API.Server;

namespace thrifty.debug;

internal static class Commands {
  internal static void register(ICoreServerAPI api) {
    #if ENABLE_DEBUG_FEATURES
    foreach (var command in new DumbCommand[]{ new HotLava() }) {
      command.register(api);
    }
    #endif
  }
}
