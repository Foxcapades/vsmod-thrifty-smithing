using System;
using System.Diagnostics.Contracts;
using System.IO;
using thrifty.debug;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace thrifty.common.config;

internal static class ConfigLoader {
  internal static (Config loadedConfig, bool modified) loadConfig(string path) {
    if (!File.Exists(path)) {
      Logs.debug("no config file found at path {0}, using default config", path);
      return (Config.defaultInstance(), true);
    }

    Stream configContents;

    try {
      configContents = File.OpenRead(path);
    } catch (FileNotFoundException) {
      Logs.error("config file {0} appears to have been deleted mid process, using default config", path);
      return (Config.defaultInstance(), true);
    } catch (Exception e) {
      Logs.error("encountered exception while attempting to load config file {0}, using default config", path);
      Logs.error(e);
      return (Config.defaultInstance(), true);
    }

    YamlDocument rawDocument;

    try {
      rawDocument = new Deserializer().Deserialize<YamlDocument>(new StreamReader(configContents));
    } catch (Exception e) {
      Logs.error("encountered exception while attempting to parse config file {0}, using default config", path);
      Logs.error(e);
      return (Config.defaultInstance(), true);
    }

    if (rawDocument.RootNode.NodeType != YamlNodeType.Mapping) {
      Logs.warn("config file {0} document root node was not a mapping, using default config", path);
      return (Config.defaultInstance(), true);
    }

    var root = (YamlMappingNode) rawDocument.RootNode;

    MaterialValues? defaultMaterials = null;
    bool needsWrite = false, tmp;

    foreach (var (key, value) in root.Children) {
      if (key.NodeType != YamlNodeType.Scalar) {
        Logs.warn("config contained complex key on root node, ignoring it");
        continue;
      }

      switch (((YamlScalarNode) key).Value) {
        case Config.ConfigKeyDefaultMaterialValues:
          (defaultMaterials, tmp) = tryParseMaterialValues(value);
          needsWrite = needsWrite || tmp;
          continue;
      }
    }

    return defaultMaterials.HasValue
      ? (new(defaultMaterials.Value), needsWrite)
      : (new(MaterialValues.defaultInstance()), true);
  }

  private static (MaterialValues value, bool needsWrite) tryParseMaterialValues(YamlNode node) {
    if (node is not YamlMappingNode mapping) {
      Logs.warn("config contained invalid or blank {0} value", Config.ConfigKeyDefaultMaterialValues);
      return (MaterialValues.defaultInstance(), true);
    }

    // Tracks whether the target fields were valid.
    //
    // If any of the fields are missed, the correlating boolean value in the
    // array will remain false.
    //
    // If any of the fields are hit, but invalid, the correlating boolean value
    // in the array will be explicitly set to false.
    //
    // If any of the fields are hit and are actually valid, the correlating
    // boolean value in the array will be set to true.
    //
    // After iterating through all the fields in the input yaml, if all of these
    // are set to true, then the YAML block was complete and valid.
    var hits = new[] {false, false, false, false};

    var vpi = MaterialValues.DefaultVoxelsPerIngot;
    var vpp = MaterialValues.DefaultVoxelsPerPlate;
    var mpb = MaterialValues.DefaultMaterialUnitsPerBit;
    var mpi = MaterialValues.DefaultMaterialUnitsPerIngot;

    foreach (var (key, value) in mapping) {
      if (key is not YamlScalarNode validKey) {
        Logs.info("config contained complex key under " + Config.ConfigKeyDefaultMaterialValues + ", ignoring it");
        continue;
      }

      switch (validKey.Value) {
        case MaterialValues.ConfigKeyVoxelsPerIngot:
          (vpi, hits[0]) = value.tryParseU8(() => Config.ConfigKeyDefaultMaterialValues + "." + MaterialValues.ConfigKeyVoxelsPerIngot);
          break;
        case MaterialValues.ConfigKeyVoxelsPerPlate:
          (vpp, hits[1]) = value.tryParseU8(() => Config.ConfigKeyDefaultMaterialValues + "." + MaterialValues.ConfigKeyVoxelsPerPlate);
          break;
        case MaterialValues.ConfigKeyMaterialUnitsPerIngot:
          (mpi, hits[2]) = value.tryParseU16(() => Config.ConfigKeyDefaultMaterialValues + "." + MaterialValues.ConfigKeyMaterialUnitsPerIngot);
          break;
        case MaterialValues.ConfigKeyMaterialUnitsPerBit:
          (mpb, hits[3]) = value.tryParseU8(() => Config.ConfigKeyDefaultMaterialValues + "." + MaterialValues.ConfigKeyMaterialUnitsPerBit);
          break;
        default:
          Logs.warn("unrecognized config key {0}", Config.ConfigKeyDefaultMaterialValues + "." + validKey.Value);
          break;
      }
    }

    return (new(vpi, vpp, mpi, mpb), !(hits[0] && hits[1] && hits[2] && hits[3]));
  }

  private static (byte, bool) tryParseU8(this YamlNode node, Func<string> path) {
    if (node is not YamlScalarNode scalar) {
      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 255 (inclusive); using default value", path());
      return (0, false);
    } else if (scalar.Value is null) {
      Logs.warn("config value {0} was null, using default value", path());
      return (0, false);
    }

    try {
      var tmp = byte.Parse(scalar.Value);

      if (tmp > 0)
        return (tmp, true);

      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 255 (inclusive); using default value", path());
      return (0, false);
    } catch (OverflowException e) {
      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 255 (inclusive); using default value", path());
      return (0, false);
    } catch (FormatException) {
      // do nothing, fallthrough
    } catch (Exception e) {
      Logs.error("encountered unexpected exception while attempting to parse byte for config key {0}; using default value", path());
      Logs.error(e);
      return (0, false);
    }

    try {
      var f = float.Parse(scalar.Value);

      if (f is > 0 and < 256)
        return ((byte) f, true);

      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 255 (inclusive); using default value", path());
      return (0, false);
    } catch (FormatException) {
      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 255 (inclusive); using default value", path());
      return (0, false);
    } catch (Exception e) {
      Logs.error("encountered unexpected exception while attempting to parse byte for config key {0}; using default value", path());
      Logs.error(e);
      return (0, false);
    }
  }

  private static (ushort, bool) tryParseU16(this YamlNode node, Func<string> path) {
    if (node is not YamlScalarNode scalar) {
      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 65535 (inclusive); using default value", path());
      return (0, false);
    } else if (scalar.Value is null) {
      Logs.warn("config value {0} was null, using default value", path());
      return (0, false);
    }

    try {
      var tmp = ushort.Parse(scalar.Value);

      if (tmp > 0)
        return (tmp, true);

      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 65535 (inclusive); using default value", path());
      return (0, false);
    } catch (OverflowException e) {
      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 65535 (inclusive); using default value", path());
      return (0, false);
    } catch (FormatException) {
      // do nothing, fallthrough
    } catch (Exception e) {
      Logs.error("encountered unexpected exception while attempting to parse byte for config key {0}; using default value", path());
      Logs.error(e);
      return (0, false);
    }

    try {
      var f = float.Parse(scalar.Value);

      if (f is > 0 and < 65536)
        return ((ushort) f, true);

      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 65535 (inclusive); using default value", path());
      return (0, false);
    } catch (FormatException) {
      Logs.warn("invalid config value for {0}, must be a whole number from 1 to 65535 (inclusive); using default value", path());
      return (0, false);
    } catch (Exception e) {
      Logs.error("encountered unexpected exception while attempting to parse byte for config key {0}; using default value", path());
      Logs.error(e);
      return (0, false);
    }
  }

}
