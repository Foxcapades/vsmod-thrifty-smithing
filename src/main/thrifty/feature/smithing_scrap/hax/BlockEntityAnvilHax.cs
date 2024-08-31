using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using thrifty.common.data;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using thrifty.common.util;
using thrifty.feature.smithing_scrap.data;
using thrifty.feature.smithing_scrap.ext;
using Vintagestory.API.Client;

namespace thrifty.feature.smithing_scrap.hax;

/// <summary>
/// Patches the <c>BlockEntityAnvil</c> class to record 'waste' voxels split
/// from smithing recipes and print out metal bits when a smithing recipe is
/// completed or aborted.
/// </summary>
internal class BlockEntityAnvilHax {
  #region Patching
  //////////////////////////////////////////////////////////////////////////////
  //                                                                          //
  //   Patching                                                               //
  //                                                                          //
  //////////////////////////////////////////////////////////////////////////////

  private static (MethodInfo, MethodInfo)[] patchedMethods = Array.Empty<(MethodInfo, MethodInfo)>();

  internal static void patch(Harmony harmony, EnumAppSide side) {
    var type = typeof(BlockEntityAnvil);

    if (side.IsServer()) {
      var onSplit = type.GetMethod(nameof(BlockEntityAnvil.OnSplit))!;
      var checkIfFinished = type.GetMethod(nameof(BlockEntityAnvil.CheckIfFinished))!;

      patchedMethods = new[] {
        (onSplit, harmony.Patch(onSplit, new(onSplitPrefix), new(onSplitPostfix))),
        (checkIfFinished, harmony.Patch(checkIfFinished, new(checkIfFinishedPrefix))),
      };
    } else {
      var openDialog = type.GetMethod("OpenDialog", BindingFlags.NonPublic | BindingFlags.Instance);

      patchedMethods = openDialog == null
        ? Array.Empty<(MethodInfo, MethodInfo)>()
        : new[] { (openDialog, harmony.Patch(openDialog, postfix: new(openDialog))) };
    }
  }

  internal static void unpatch(Harmony harmony) {
    foreach (var (og, patch) in patchedMethods) {
      harmony.Unpatch(og, patch);
    }
  }

  #endregion Patching

  #region OnSplit
  //////////////////////////////////////////////////////////////////////////////
  //                                                                          //
  //   OnSplit Patch                                                          //
  //                                                                          //
  //////////////////////////////////////////////////////////////////////////////

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
  private static void onSplitPrefix(Vec3i voxelPos, BlockEntityAnvil __instance) {
    // . Only operate if the target voxel is metal
    // . Only operate if the recipe was not disallowed in the config
    if (isMetalVoxel(voxelPos, __instance) && Smithy.recipeIsAllowed(__instance.SelectedRecipe)) {
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
  private static void onSplitPostfix(BlockEntityAnvil __instance) {
    // . Only operate for allowed recipes
    // . Only operate if there are no voxels left
    if (Smithy.recipeIsAllowed(__instance.SelectedRecipe) && !hasRemainingVoxels(__instance)) {
      // If all the voxels were split off of the work item, then the recipe has
      // been aborted.  Spit out bits equivalent to the input materials used for
      // the recipe.
      var data = __instance.getWorkData();

      if (!data.HasValue)
        return;

      if (data.Value.hasInputs && shouldPrintBits(__instance))
        BitGen.issueBits(Smithy.calculateTotalInputBits(data.Value), __instance);

      __instance.clearWorkData();
    }
  }

  //
  //
  // Internals
  //
  //

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

  #endregion OnSplit

  #region CheckIfFinished
  //////////////////////////////////////////////////////////////////////////////
  //                                                                          //
  //   CheckIfFinished Patch                                                  //
  //                                                                          //
  //////////////////////////////////////////////////////////////////////////////

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
  private static void checkIfFinishedPrefix(IPlayer? byPlayer, BlockEntityAnvil __instance) {
    // 1. Only run on the server side
    // 2. Only run if there is a recipe
    // 3. Only run if the work is completed
    if (__instance.Api.World.Side.IsClient() || __instance.SelectedRecipe == null || !workCompleted(__instance))
      return;

    var data = __instance.getWorkData();

    if (!data.HasValue || !shouldPrintBits(__instance))
      return;

    if (byPlayer is null)
      BitGen.issueBits(Smithy.calculateWasteReturnBits(data.Value, __instance.SelectedRecipe), __instance);
    else
      BitGen.issueBits(Smithy.calculateWasteReturnBits(data.Value, __instance.SelectedRecipe), byPlayer, __instance);

    __instance.clearWorkData();
  }

  //
  //
  // Internals
  //
  //

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

  #endregion CheckIfFinished

  #region OpenDialog
  //////////////////////////////////////////////////////////////////////////////
  //                                                                          //
  //   OpenDialog Patch                                                       //
  //                                                                          //
  //////////////////////////////////////////////////////////////////////////////

  private static void openDialog(ItemStack? ingredient, GuiDialog? ___dlg) {
    if (ingredient == null || ___dlg is not GuiDialogBlockEntityRecipeSelector)
      return;

    var grantedVoxels = ingredient.grantedVoxelCount();

    if (grantedVoxels < 1)
      return;

    var field = typeof(GuiDialogBlockEntityRecipeSelector)
      .GetField("skillItems", BindingFlags.NonPublic | BindingFlags.Instance);

    if (field?.GetValue(___dlg) is not List<SkillItem> items)
      return;

    foreach (var item in items) {
      var data = RecipeDataCache.lookup(item.Code);

      if (!data.HasValue)
        continue;

      string message;

      if (data.Value.voxels > grantedVoxels)
        message = "Requires additional material."; // TODO: i8n
      else {
        var (salvaged, lost) = Smithy.calculateWaste(grantedVoxels - data.Value.voxels);
        // TODO: i8n
        message = lost > 0
          ? string.Format("{0} salvageable bits, {1} units of material lost", salvaged, lost.toUserString())
          : string.Format("{0} salvageable bits", salvaged);
      }

      if (item.Description.Length == 0)
        item.Description = message;
      else
        item.Description += Environment.NewLine + message;
    }
  }

  #endregion OpenDialog

  #region Shared Logic
  //////////////////////////////////////////////////////////////////////////////
  //                                                                          //
  //   Shared Internal Methods                                                //
  //                                                                          //
  //////////////////////////////////////////////////////////////////////////////

  private static bool shouldPrintBits(BlockEntityAnvil anvil) =>
    anvil.SelectedRecipe?.Output != null
    && !ThriftySmithing.Config.disallowedRecipes.Contains(anvil.SelectedRecipe.Output.Code.ToString());

  #endregion Shared Logic
}
