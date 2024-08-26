using System.IO;
using thrifty.common.utils;
using thrifty.debug;
using YamlDotNet.RepresentationModel;

namespace thrifty.common.config;


internal record Config(
  MaterialValues defaultMaterialValues,
  SmithingWasteRecoveryConfig smithingWasteConfig
) {
  internal const string ConfigKeyDefaultMaterialValues = "defaultMaterialValues";

  internal const string ConfigKeySmithingWaste = "smithingWasteConfig";

  internal static Config defaultInstance() => new(
    MaterialValues.defaultInstance(),
    SmithingWasteRecoveryConfig.defaultInstance()
  );

  internal void toYAML(StringWriter stream, Indent indent) {
    stream.WriteLine("#");
    stream.WriteLine("# Thrifty Smithing Configuration");
    stream.WriteLine("#");

    stream.WriteLine();

    stream.WriteLine("# Default material values used by various features.");
    stream.writeKey(ConfigKeyDefaultMaterialValues);
    defaultMaterialValues.toYAML(stream, indent);

    stream.WriteLine();

    stream.WriteLine("# Settings for the smithing waste recovery feature.");
    stream.writeKey(ConfigKeySmithingWaste);
    smithingWasteConfig.toYAML(stream, indent);
  }

  internal static (Config config, bool needsWrite) fromYAML(YamlDocument document) {
    if (document.RootNode is not YamlMappingNode mapping) {
      Logs.warn("config file root node was not a mapping, using default config");
      return (defaultInstance(), true);
    }

    MaterialValues? defaultMaterials = null;
    bool needsWrite = false, tmp;


    foreach (var (key, value) in mapping) {
      if (key.NodeType != YamlNodeType.Scalar) {
        Logs.warn("config contained complex key on root node, ignoring it");
        continue;
      }

      switch (((YamlScalarNode) key).Value) {
        case ConfigKeyDefaultMaterialValues:
          (defaultMaterials, tmp) = tryParseMaterialValues(value);
          needsWrite = needsWrite || tmp;
          continue;
      }
    }
  }
};
