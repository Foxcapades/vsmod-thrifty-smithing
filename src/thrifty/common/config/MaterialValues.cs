using System.IO;
using thrifty.common.utils;
using YamlDotNet.RepresentationModel;

namespace thrifty.common.config;

internal readonly record struct MaterialValues(
  byte voxelsPerIngot,
  byte voxelsPerPlate,
  ushort materialUnitsPerIngot,
  byte materialUnitsPerBit
) {
  /// <summary>
  /// Defines the default or fallback value to use when configuring the number of
  /// voxels a single ingot represents when placed on an anvil.
  /// </summary>
  ///
  /// <remarks>
  /// This value is based on standard Vintage Story behavior and should only be
  /// updated if there is a change to this behavior in the base game.
  /// </remarks>
  internal const byte DefaultVoxelsPerIngot = 42;
  internal const string ConfigKeyVoxelsPerIngot = "voxelsPerIngot";

  /// <summary>
  /// Defines the default or fallback value to use when configuring the number of
  /// voxels a single metal plate represents when placed on an anvil.
  /// </summary>
  ///
  /// <remarks>
  /// This value is based on standard Vintage Story behavior and should only be
  /// updated if there is a change to this behavior in the base game.
  /// </remarks>
  internal const byte DefaultVoxelsPerPlate = 81;
  internal const string ConfigKeyVoxelsPerPlate = "voxelsPerPlate";

  /// <summary>
  /// Defines the default or fallback value to use when configuring the amount or
  /// number of units make up a single ingot of a given material.
  /// </summary>
  ///
  /// <remarks>
  /// This value is based on standard Vintage Story behavior and should only be
  /// updated if there is a change to this behavior in the base game.
  /// </remarks>
  internal const ushort DefaultMaterialUnitsPerIngot = 100;
  internal const string ConfigKeyMaterialUnitsPerIngot = "materialUnitsPerIngot";

  /// <summary>
  /// Defines the default or fallback value to use when configuring the amount or
  /// number of units are represented by a single 'bit' of a given material.
  /// </summary>
  ///
  /// <remarks>
  /// This value is based on standard Vintage Story behavior and should only be
  /// updated if there is a change to this behavior in the base game.
  /// </remarks>
  internal const byte DefaultMaterialUnitsPerBit = 5;
  internal const string ConfigKeyMaterialUnitsPerBit = "materialUnitsPerBit";

  internal static MaterialValues defaultInstance() =>
    new(
      DefaultVoxelsPerIngot,
      DefaultVoxelsPerPlate,
      DefaultMaterialUnitsPerIngot,
      DefaultMaterialUnitsPerBit
    );

  internal void toYAML(StringWriter stream, Indent indent) {
    indent++;

    stream.WriteLine();

    stream.WriteLine();
    stream.writeLine(indent, "# Tells the mod how many voxels are added to a smithing work when an ingot");
    stream.writeLine(indent, "# is placed on an anvil.");
    stream.writeLine(indent, ConfigKeyVoxelsPerIngot, voxelsPerIngot);

    stream.WriteLine();
    stream.writeLine(indent, "# Tells the mod how many voxels are added to a smithing work when a metal");
    stream.writeLine(indent, "# plate is placed on an anvil.");
    stream.writeLine(indent, ConfigKeyVoxelsPerPlate, voxelsPerPlate);

    stream.WriteLine();
    stream.writeLine(indent, "# Tells the mod how many units of a metal it takes to form an ingot.");
    stream.writeLine(indent, ConfigKeyMaterialUnitsPerIngot, materialUnitsPerIngot);

    stream.WriteLine();
    stream.writeLine(indent, "# Tells the mod how many units of a metal are rewarded when melting down a");
    stream.writeLine(indent, "# single metal bit.");
    stream.writeLine(indent, ConfigKeyMaterialUnitsPerBit, materialUnitsPerBit);

    indent--;
  }

  internal static (MaterialValues value, bool needsWrite) fromYAML(YamlNode node) {

  }
};
