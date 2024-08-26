using System;
using thrifty.debug;
using YamlDotNet.RepresentationModel;

namespace thrifty.common.utils;

internal static class XYAML {
  internal static (byte, bool) tryParseU8(this YamlNode node, Func<string> path) {
    if (node is not YamlScalarNode scalar) {
      Logs.warn("invalid config value for {0}, must be a whole number from 0 to 255 (inclusive)", path());
      return (0, false);
    } else if (scalar.Value is null) {
      Logs.warn("config value {0} was null", path());
      return (0, false);
    }

    try {
      return (byte.Parse(scalar.Value), true);
    } catch (OverflowException e) {
      Logs.warn("invalid config value for {0}, must be a whole number from 0 to 255 (inclusive)", path());
      return (0, false);
    } catch (FormatException) {
      // do nothing, fallthrough
    } catch (Exception e) {
      Logs.error("encountered unexpected exception while attempting to parse byte for config key {0}", path());
      Logs.error(e);
      return (0, false);
    }

    try {
      var f = float.Parse(scalar.Value);

      if (f is > 0 and < 256) {
        Logs.warn("");
        return ((byte) f, true);
      }

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

}
