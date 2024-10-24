using System.Collections.Generic;
using ThriftySmithing.Utils;
using Vintagestory.API.Datastructures;

namespace ThriftySmithing.Data;

public readonly record struct WorkDataModifiers(sbyte voxels, sbyte ingots, sbyte plates) {
  public const string VoxelKey = "voxels";
  public const string IngotKey = "ingots";
  public const string PlateKey = "plates";

  public bool isBlank => voxels == 0 && ingots == 0 && plates == 0;

  public ITreeAttribute toAttribute() {
    return new TreeAttribute {
      [VoxelKey] = new IntAttribute(voxels),
      [IngotKey] = new IntAttribute(ingots),
      [PlateKey] = new IntAttribute(plates),
    };
  }

  public static WorkDataModifiers[] fromDataTree(List<(string, ITreeAttribute)> data) {
    var hopeful = new WorkDataModifiers[data.Count];
    var i = 0;

    foreach (var (key, value) in data) {
      var mod = fromAttribute(key, value);

      if (mod.isBlank)
        Logs.info("skipping extension point key {0} as it is blank (all zero values)", key);
      else
        hopeful[i++] = mod;
    }

    return hopeful[..i];
  }

  private static WorkDataModifiers fromAttribute(string key, ITreeAttribute data) {
    return new WorkDataModifiers(
      validatedByte(key, VoxelKey, data.TryGetInt(VoxelKey)),
      validatedByte(key, IngotKey, data.TryGetInt(IngotKey)),
      validatedByte(key, PlateKey, data.TryGetInt(PlateKey))
    );
  }

  private static sbyte validatedByte(string mod, string key, int? value) {
    switch (value) {
      case null:
        return 0;
      case > 127:
        Logs.error("extension point {0} contains a value for key {1} that is greater than 127: {2}", mod, key, value);
        return 0;
      case < -128:
        Logs.error("extension point {0} contains a value for key {1} that is less than -128: {2}", mod, key, value);
        return 0;
      default:
        return (sbyte) value;
    }
  }
}
