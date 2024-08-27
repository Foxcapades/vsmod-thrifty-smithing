using System;
using Newtonsoft.Json.Linq;

namespace ThriftySmithing.Utils;

internal static class Cereal {
  internal static class JSON {
    internal static (Half value, bool wasValid) tryParseHalf(JToken json, string key, Half defaultValue) {
      var (f, b) = tryParseFloat(json, key, 0);
      return b ? ((Half) f, true) : (defaultValue, false);
    }

    internal static (float value, bool wasValid) tryParseFloat(JToken json, string key, float defaultValue) {
      if (json[key] == null)
        return (defaultValue, false);

      var value = json[key]!;

      switch (value.Type) {
        case JTokenType.Float:
          return tryUnpackFloat(value, defaultValue);

        case JTokenType.Integer:
          var (i, b) = tryUnpackInt(value, 0);

          if (!b)
            return (defaultValue, false);

          try {
            return (i, true);
          } catch (OverflowException) {
            return (defaultValue, false);
          }

        default:
          return (defaultValue, false);
      }
    }

    private static (float value, bool wasValid) tryUnpackFloat(JToken raw, float defaultValue) {
      try {
        return (raw.Value<float>(), true);
      } catch (OverflowException) {
        return (defaultValue, false);
      }
    }

    private static (int value, bool wasValid) tryUnpackInt(JToken raw, int defaultValue) {
      try {
        return (raw.Value<int>(), true);
      } catch (OverflowException) {
        return (defaultValue, false);
      }
    }
  }
}
