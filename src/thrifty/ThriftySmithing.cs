using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using thrifty.common;
using thrifty.common.data;
using thrifty.common.utils;
using thrifty.common.x;
using thrifty.debug;
using thrifty.feat.smithing.data;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

#nullable enable
namespace thrifty;

public class ThriftySmithing : ModSystem {
  private const string ConfigFileName = "thrifty-smithing.json";

  private static ThriftySmithingConfig? loadedConfig;

  #region Mod Internal

  internal const string WorkDataKey = "ef:ts:workData";

  internal static readonly byte InternalAttributeID = (byte) (TreeAttribute.AttributeIdMapping.Count + 1);

  internal static ThriftySmithingConfig Config =>
    loadedConfig ??
    throw new InvalidOperationException("attempted to get the ThriftySmithing mod configuration before it was loaded");

  #endregion Mod Internal

  private ICoreServerAPI? serverAPI;

  public override void Start(ICoreAPI api) {
    Logs.init(Mod.Logger);

    registerTypes();
    loadConfig(api);
    applyHarmonyPatches(Mod.Info.ModID, api.Side);

    base.Start(api);
  }

  public override void StartServerSide(ICoreServerAPI api) {
    serverAPI = api;

    api.Event.ServerRunPhase(EnumServerRunPhase.GameReady, findSmithableItems);

    #if ENABLE_DEBUG_FEATURES
    Commands.register(api);
    #endif
  }

  private void findSmithableItems() {
    var gridIndex = new Dictionary<AssetKey, HashSet<AssetKey>>(1024);

    foreach (var recipe in serverAPI!.World.GridRecipes) {
      if (!recipe.Enabled || recipe.Output is null || recipe.Output.Type != EnumItemClass.Item)
        continue;

      // TODO: wildcards??? if wildcards are allowed in ingredients, then will
      //       this result in missing recipes that we should hit?
      foreach (var ingredient in recipe.resolvedIngredients) {
        var key = (AssetKey) ingredient.Code;

        if (gridIndex.TryGetValue(key, out var value)) {
          value.Add(recipe.Output.Code);
        } else {
          gridIndex[key] = new HashSet<AssetKey> { recipe.Output.Code };
        }
      }
    }

    var hits = 0;

    foreach (var recipe in serverAPI!.getValidSmithingRecipes()) {
      var ingredient = serverAPI!.getItem(recipe.Ingredient.Code);

      if (ingredient?.CombustibleProps is not { SmeltingType: EnumSmeltType.Smelt, SmeltedRatio: > 0 }) // TODO: does this work?
        continue;

      var result = serverAPI!.getItem(recipe.Output.Code);

      if (result is null)
        continue;

      // TODO: result could be a usable item directly or a tool head or
      //       something, see if it is usable itself (has a durability) or if it
      //       is part of a crafting grid recipe.  That may mean getting an
      //       index of all grid crafting recipes?

      var voxelValue = (ushort) (recipe.voxelCount() / recipe.Output.Quantity);

      // is a tool already (chisel or mod item)
      if (result.Tool.HasValue || result.Durability > 1) {
        Cache.registerVoxelValue(result, voxelValue);
        hits++;
        continue;
      }

      var resultKey = (AssetKey) result.Code;

      // is an ingredient for a tool
      if (gridIndex.TryGetValue(resultKey, out var outputs)) {
        foreach (var trueForm in outputs) {
          var resolvedTrueForm = serverAPI.getItem(trueForm);

          if (resolvedTrueForm == null)
            continue;

          // TODO: Wildcard?
          if (resolvedTrueForm.Tool != null || resolvedTrueForm.Durability > 1) {
            Cache.registerVoxelValue(trueForm, voxelValue);
            hits++;
          }
        }
      }
    }

    Logs.debug("found {0} item candidates for scrap recovery on break", hits);
  }

  private static void loadConfig(ICoreAPI api) {
    bool writeConfig;

    try {
      var configJSON = api.LoadModConfig(ConfigFileName);

      if (configJSON == null) {
        Logs.debug("no config file found, one will be generated");
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
        api.StoreModConfig(loadedConfig.Value.toJSON(), ConfigFileName);
      } catch (Exception e) {
        Console.WriteLine(e);
        throw;
      }
    }
  }

  private static void registerTypes() {
    TreeAttribute.RegisterAttribute(InternalAttributeID, typeof(WorkData));
  }

  private static void applyHarmonyPatches(string id, EnumAppSide side) {
    if (Harmony.HasAnyPatches(id))
      return;

    if (side == EnumAppSide.Server)
      new Harmony(id).PatchCategory(Assembly.GetExecutingAssembly(), Const.Harmony.Category.Server);
  }
}
