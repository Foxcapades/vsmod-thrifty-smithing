using System;
using HarmonyLib;
using Vintagestory.API.Common;

using thrifty.common.config;
using thrifty.common.util;
using thrifty.feature.smithing_scrap;

namespace thrifty;

public class ThriftySmithing : ModSystem {
  private const string ConfigFileName = "thrifty-smithing.json";

  private static ThriftySmithingConfig? loadedConfig;

  internal static ThriftySmithingConfig Config =>
    loadedConfig ??
    throw new InvalidOperationException("attempted to get the ThriftySmithing mod configuration before it was loaded");

  public override void Start(ICoreAPI api) {
    Logs.init(Mod.Logger);

    loadConfig(api);
    applyHarmonyPatches(Mod.Info.ModID, api);

    base.Start(api);
  }

  private static void loadConfig(ICoreAPI api) {
    bool writeConfig;

    try {
      var configJSON = api.LoadModConfig(ConfigFileName);

      if (configJSON == null) {
        Logs.info("no config file found, one will be generated");
        loadedConfig = ThriftySmithingConfig.defaultConfig();
        writeConfig = true;
      } else {
        Logs.debug("loaded mod configuration");
        (loadedConfig, writeConfig) = ThriftySmithingConfig.parseFromJSON(configJSON);
      }
    } catch (Exception e) {
      Logs.error("failed to load config JSON: {0}", e.Message);
      loadedConfig = ThriftySmithingConfig.defaultConfig();
      writeConfig = true;
    }

    if (writeConfig) {
      try {
        Logs.info("writing configuration to file: {0}", ConfigFileName);
        api.StoreModConfig(loadedConfig.toJSON(), ConfigFileName);
      } catch (Exception e) {
        Console.WriteLine(e);
        throw;
      }
    }
  }

  private static void applyHarmonyPatches(string id, ICoreAPI api) {
    if (!Harmony.HasAnyPatches(id)) {
      var harmony = new Harmony(id);

      SmithingScrapFeature.register(harmony, api);
    }
  }
}
