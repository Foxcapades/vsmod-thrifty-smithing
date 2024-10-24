using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using ThriftySmithing.Data;
using ThriftySmithing.Extensions;
using ThriftySmithing.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace ThriftySmithing.Hax;

/**
 * <summary>
 * Patches the <c>BlockEntityAnvil</c> class to record 'waste' voxels split from
 * smithing recipes and print out metal bits when a smithing recipe is completed
 * or aborted.
 * </summary>
 */
[HarmonyPatch(typeof(BlockEntityAnvil))]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class BlockEntityAnvilHax {

  /**
   * <summary>
   * Before a voxel is split from a smithing recipe, if it is metal, add it to
   * the work-in-progress' waste voxel counter.
   * </summary>
   *
   * <param name="voxelPos">
   * Position of the target voxel to be split from the recipe.
   * </param>
   *
   * <param name="__instance">
   * Entity instance the patched method is being called on, provided by Harmony.
   * </param>
   */
  [HarmonyPrefix]
  [HarmonyPatch(nameof(BlockEntityAnvil.OnSplit))]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  private static void onSplitPrefix(Vec3i voxelPos, BlockEntityAnvil __instance) {
    // 1. Only operate on the server side
    // 2. Only operate if the target voxel is metal
    // 3. Only operate if the recipe was not disallowed in the config
    if (
      __instance.Api.World.Side.IsServer()
      && isMetalVoxel(voxelPos, __instance)
      && Smithy.recipeIsAllowed(__instance.SelectedRecipe)
    ) {
      var workData = __instance.getWorkData() ?? new();
      __instance.setWorkData(workData);
    }
  }

  /**
   * <summary>
   * After a voxel is split from a smithing recipe, if it was the last voxel
   * (meaning the recipe was aborted) try and spit out a number of metal bits
   * equivalent to the amount of material put into the recipe in the form of
   * ingots or plates.
   * </summary>
   *
   * <param name="__instance">
   * Entity instance the patched method is being called on, provided by Harmony.
   * </param>
   */
  [HarmonyPostfix]
  [HarmonyPatch(nameof(BlockEntityAnvil.OnSplit))]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  private static void onSplitPostfix(BlockEntityAnvil __instance) {
    // 1. Only operate on the server side
    // 2. Only operate for allowed recipes
    // 3. Only operate if there are no voxels left
    if (
      __instance.Api.World.Side.IsServer()
      && Smithy.recipeIsAllowed(__instance.SelectedRecipe)
      && !hasRemainingVoxels(__instance)
    ) {
      Logs.trace("paying attention to BlockEntityAnvil.OnSplit postfix");

      // If all the voxels were split off of the work item, then the recipe has
      // been aborted.  Spit out bits equivalent to the input materials used for
      // the recipe.
      var data = __instance.getWorkData();

      var extensionData = __instance.findExtensionData();

      var voxels = 0;

      if (!data.HasValue) {
        if (extensionData.Count == 0)
          return;

        var (v, i, p) = sumExtensionData(WorkDataModifiers.fromDataTree(extensionData));

        var tmp = new WorkData {
          ingotCount = (byte) Math.Max(0, i),
          plateCount = (byte) Math.Max(0, p),
        };

        data = tmp;
        voxels = v;
      } else if (extensionData.Count > 0) {
        var tmp = data.Value;

        var (v, i, p) = sumExtensionData(WorkDataModifiers.fromDataTree(extensionData));

        tmp.ingotCount += (byte) Math.Max(0, i);
        tmp.plateCount += (byte) Math.Max(0, p);

        data = tmp;
        voxels = v;
      }

      if (data.Value.hasInputs && shouldPrintBits(__instance))
        BitGen.issueBits(Smithy.calculateWasteReturnBits(
          Math.Max(0, Smithy.calculateWasteVoxels(data.Value, __instance.SelectedRecipe) + voxels)),
          __instance
        );

      __instance.clearWorkData();
    }
  }

  /**
   * <summary>
   * If a player has completed their work-in-progress, spit out metal bits for
   * waste voxels if possible.
   * </summary>
   *
   * <param name="byPlayer">
   * The player currently interacting with the anvil entity.
   * </param>
   *
   * <param name="__instance">
   * Entity instance the patched method is being called on, provided by Harmony.
   * </param>
   */
  [HarmonyPrefix]
  [HarmonyPatch(nameof(BlockEntityAnvil.CheckIfFinished))]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  // TODO: byPlayer can be null!!!
  private static void checkIfFinishedPrefix(IPlayer? byPlayer, BlockEntityAnvil __instance) {
    // 1. Only run on the server side
    // 2. Only run if there is a recipe
    // 3. Only run if the work is completed
    if (__instance.Api.World.Side.IsClient() || __instance.SelectedRecipe == null || !workCompleted(__instance))
      return;

    Logs.trace("paying attention to BlockEntityAnvil.CheckIfFinished prefix");

    var data = __instance.getWorkData();

    if (!data.HasValue || !shouldPrintBits(__instance)) {
      Logs.trace("skipping early: data.HasValue={0}, shouldPrintBits={1}", data.HasValue, shouldPrintBits(__instance));
      return;
    }

    var voxels = calculateVoxelsForWork(__instance);
    var bits = Smithy.calculateWasteReturnBits(voxels);
    Logs.trace("waste voxels: {0}", voxels);
    Logs.trace("bits to return: {0}", bits);

    if (byPlayer is null)
      BitGen.issueBits(bits, __instance);
    else
      BitGen.issueBits(bits, byPlayer, __instance);

    __instance.clearWorkData();
  }

  private static int calculateVoxelsForWork(BlockEntityAnvil anvil) {
    var data = anvil.getWorkData();
    var extensionData = anvil.findExtensionData();

    // If there is no work data assigned by ThriftySmithing itself.
    if (!data.HasValue) {
      Logs.trace("no work data");

      // AND there is no data wired in from another plugin
      if (extensionData.Count == 0) {
        Logs.trace("no extension data");
        return 0;
      }

      var (v, i, p) = sumExtensionData(WorkDataModifiers.fromDataTree(extensionData));

      var tmp = new WorkData {
        ingotCount = (byte) Math.Max(0, i),
        plateCount = (byte) Math.Max(0, p),
      };

      Logs.trace("with extension data total sums to: Voxels({0}), {1}", v, tmp);

      return Math.Max(0, Smithy.calculateWasteVoxels(tmp, anvil.SelectedRecipe) + v);
    }

    if (extensionData.Count == 0) {
      Logs.trace("no extension data");
      return Smithy.calculateWasteVoxels(data.Value, anvil.SelectedRecipe);
    }

    {
      var tmp = data.Value;

      var (v, i, p) = sumExtensionData(WorkDataModifiers.fromDataTree(extensionData));

      tmp.ingotCount += (byte) Math.Max(0, i);
      tmp.plateCount += (byte) Math.Max(0, p);

      Logs.trace("with extension data total sums to: Voxels({0}), {1}", v, tmp);

      return Math.Max(0, Smithy.calculateWasteVoxels(tmp, anvil.SelectedRecipe) + v);
    }
  }

  private static (int, int, int) sumExtensionData(WorkDataModifiers[] mods) {
    var voxels = 0;
    var ingots = 0;
    var plates = 0;

    foreach (var mod in mods) {
      voxels += mod.voxels;
      ingots += mod.ingots;
      plates += mod.plates;
    }

    return (voxels, ingots, plates);
  }

  /**
   * <summary>
   * Tests whether a work-in-progress on the given anvil is completed.
   * </summary>
   *
   * <remarks>
   * Sadly this method basically just duplicates the content of the
   * <c>BlockEntityAnvil.WorkMatchesRecipe</c> method.  If it were possible to
   * hook into the existing call to that method inside
   * <c>BlockEntityAnvil.CheckIfFinished</c> this method and the extra work it
   * does could be removed.
   * </remarks>
   *
   * <param name="anvil">
   * Anvil entity on which the work-in-progress should be tested.
   * </param>
   *
   * <returns>
   * <c>true</c> if the work-in-progress on the given anvil matches the target
   * smithing recipe, otherwise <c>false</c>.
   * </returns>
   */
  private static bool workCompleted(BlockEntityAnvil anvil) {
    var xLim = anvil.Voxels.GetLength(0);
    var yLim = Math.Min(anvil.SelectedRecipe.QuantityLayers, anvil.Voxels.GetLength(1));
    var zLim = anvil.Voxels.GetLength(2);

    var rotatedVoxels = anvil.recipeVoxels!;

    for (var x = 0; x < xLim; x++) {
      for (var y = 0; y < yLim; y++) {
        for (var z = 0; z < zLim; z++) {
          var expectedType = (byte) (rotatedVoxels[x, y, z]
            ? EnumVoxelMaterial.Metal
            : EnumVoxelMaterial.Empty);

          if (anvil.Voxels[x, y, z] != expectedType)
            return false;
        }
      }
    }

    return true;
  }

  private static bool shouldPrintBits(BlockEntityAnvil anvil) =>
    anvil.SelectedRecipe?.Output != null
    && !ThriftySmithing.Config.disallowedRecipes.Contains(anvil.SelectedRecipe.Output.Code.ToString());

  /**
   * <summary>
   * Tests if there are any non-empty voxels remaining for the work-in-progress
   * item on the given anvil.
   * </summary>
   *
   * <param name="anvil">
   * Anvil entity whose work-in-progress item should be tested for non-empty
   * voxels.
   * </param>
   *
   * <returns>
   * <c>true</c> if there is at least one non-empty voxel remaining on the given
   * anvil, otherwise <c>false</c>.
   * </returns>
   */
  private static bool hasRemainingVoxels(BlockEntityAnvil anvil) {
    foreach (var voxel in anvil.Voxels)
      if ((EnumVoxelMaterial) voxel is EnumVoxelMaterial.Metal or EnumVoxelMaterial.Slag)
        return true;

    return false;
  }

  /**
   * <summary>
   * Tests if the voxel at the given position for the given anvil's
   * work-in-progress item is metal.
   * </summary>
   *
   * <param name="pos">
   * Position of the voxel to test.
   * </param>
   *
   * <param name="anvil">
   * Anvil on which the target voxel will be tested.
   * </param>
   *
   * <returns>
   * <c>true</c> if the target voxel is metal, otherwise <c>false</c>.
   * </returns>
   */
  private static bool isMetalVoxel(Vec3i pos, BlockEntityAnvil anvil) =>
    (EnumVoxelMaterial) anvil.Voxels[pos.X, pos.Y, pos.Z] == EnumVoxelMaterial.Metal;
}
