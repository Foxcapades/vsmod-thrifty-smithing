using ThriftySmithing.Utils;
using Vintagestory.API.Datastructures;

namespace ThriftySmithing.Data;

public record struct WorkDataModifiers(sbyte voxels, sbyte ingots, sbyte plates) {
  public const string VoxelKey = "voxels";
  public const string IngotKey = "ingots";
  public const string PlateKey = "plates";

  public bool isBlank => voxels == 0 && ingots == 0 && plates == 0;

  public ITreeAttribute toAttribute() {
    var attr = new TreeAttribute();
    attr[VoxelKey] = new IntAttribute(voxels);
    attr[IngotKey] = new IntAttribute(ingots);
    attr[PlateKey] = new IntAttribute(plates);
    return attr;
  }

  public static WorkDataModifiers[] fromDataTree(ITreeAttribute data) {
    var hopeful = new WorkDataModifiers[data.Count];
    var kvs = data.GetEnumerator();
    var i = 0;

    while (kvs.MoveNext()) {
      if (kvs.Current.Value == null)
        continue;

      if (kvs.Current.Value is not ITreeAttribute) {
        Logs.warn("work data extension point contained an invalid value under key {0}", kvs.Current.Key);
        continue;
      }

      var mod = fromAttribute(kvs.Current.Key, (ITreeAttribute) kvs.Current.Value);

      if (mod.isBlank)
        Logs.info("skipping extension point key {0} as it is blank (all zero values)", kvs.Current.Key);
      else
        hopeful[i++] = mod;
    }

    kvs.Dispose();

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
        Logs.error("extension point {0} contains a value for key {1} that is greater than 255: {2}", mod, key, value);
        return 0;
      case < -128:
        Logs.error("extension point {0} contains a value for key {1} that is less than 0: {2}", mod, key, value);
        return 0;
      default:
        return (sbyte) value;
    }
  }
}
