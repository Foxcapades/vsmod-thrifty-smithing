using System;
using System.Reflection;
using HarmonyLib;
using ThriftySmithing.Data;
using ThriftySmithing.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace ThriftySmithing;

public class ThriftySmithing : ModSystem {
  private const string ConfigFileName = "thrifty-smithing.json";

  private static ThriftySmithingConfig? loadedConfig;

  #region Mod Internal

  internal const string WorkDataKey = "ef:ts:workData";

  internal const string WorkDataModifierKey = "ef:ts:workData:modifiers";

  internal static readonly byte InternalAttributeID = (byte) (TreeAttribute.AttributeIdMapping.Count + 1);

  internal static ThriftySmithingConfig Config =>
    loadedConfig ??
    throw new InvalidOperationException("attempted to get the ThriftySmithing mod configuration before it was loaded");

  #endregion Mod Internal

  public override void Start(ICoreAPI api) {
    Logs.init(Mod.Logger);

    registerTypes();
    loadConfig(api);
    applyHarmonyPatches(Mod.Info.ModID);

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

  private static void registerTypes() {
    TreeAttribute.RegisterAttribute(InternalAttributeID, typeof(WorkData));
  }

  private static void applyHarmonyPatches(string id) {
    if (!Harmony.HasAnyPatches(id))
      new Harmony(id).PatchAll(Assembly.GetExecutingAssembly());
  }
}
