using System;
using Newtonsoft.Json.Linq;

namespace thrifty.common.util;

/// <summary>
/// Serialization Utilities
/// </summary>
internal static class Cereal {

  /// <summary>
  /// JSON Serialization Utilities
  /// </summary>
  internal static class JSON {

    /// <summary>
    /// Tries to parse the value stored in the given <paramref name="json"/>
    /// object under the given <paramref name="key"/> as a <c>Half</c> value.
    /// <para />
    /// If for some reason that is not possible, the given default value will be
    /// returned instead.
    /// <para />
    /// Additionally, returns a boolean indicator whether the target value could
    /// be parsed.
    /// </summary>
    ///
    /// <remarks>
    /// Conditions under which the default value would be returned include:
    /// <list type="bullet">
    ///   <item><description>
    ///   The input <paramref name="json"/> value did not represent a JSON
    ///   object.
    ///   </description></item>
    ///   <item><description>
    ///   The target key was not present on the input <paramref name="json"/>
    ///   object.
    ///   </description></item>
    ///   <item><description>
    ///   The value stored under the target key could not be parsed as a
    ///   <c>Half</c> value due to type incompatibility or overflow.
    ///   </description></item>
    /// </list>
    /// </remarks>
    ///
    /// <param name="json">
    /// Supposed JSON object on which <paramref name="key"/> should be
    /// available.
    /// </param>
    ///
    /// <param name="key">
    /// Key in the JSON object of the target value to try and parse.
    /// </param>
    ///
    /// <param name="defaultValue">
    /// Fallback value to use if a <c>Half</c> value could not be parsed from
    /// the target JSON for any reason.
    /// </param>
    ///
    /// <returns>
    /// A tuple of a <c>Half</c> value and a <c>bool</c> value indicating
    /// whether the method was able to successfully parse the target key from
    /// the target JSON.
    /// <para />
    /// A boolean return value of <c>false</c> indicates that the parsing failed
    /// and the given default value was returned.
    /// </returns>
    internal static (Half value, bool wasValid) tryParseHalf(JToken json, string key, Half defaultValue) {
      var (f, b) = tryParseFloat(json, key, 0);
      return b ? ((Half) f, true) : (defaultValue, false);
    }

    /// <summary>
    /// Tries to parse the value stored in the given <paramref name="json"/>
    /// object under the given <paramref name="key"/> as a <c>float</c> value.
    /// <para />
    /// If for some reason that is not possible, the given default value will be
    /// returned instead.
    /// <para />
    /// Additionally, returns a boolean indicator whether the target value could
    /// be parsed.
    /// </summary>
    ///
    /// <remarks>
    /// Conditions under which the default value would be returned include:
    /// <list type="bullet">
    ///   <item><description>
    ///   The input <paramref name="json"/> value did not represent a JSON
    ///   object.
    ///   </description></item>
    ///   <item><description>
    ///   The target key was not present on the input <paramref name="json"/>
    ///   object.
    ///   </description></item>
    ///   <item><description>
    ///   The value stored under the target key could not be parsed as a
    ///   <c>float</c> value due to type incompatibility or overflow.
    ///   </description></item>
    /// </list>
    /// </remarks>
    ///
    /// <param name="json">
    /// Supposed JSON object on which <paramref name="key"/> should be
    /// available.
    /// </param>
    ///
    /// <param name="key">
    /// Key in the JSON object of the target value to try and parse.
    /// </param>
    ///
    /// <param name="defaultValue">
    /// Fallback value to use if a <c>float</c> value could not be parsed from
    /// the target JSON for any reason.
    /// </param>
    ///
    /// <returns>
    /// A tuple of a <c>float</c> value and a <c>bool</c> value indicating
    /// whether the method was able to successfully parse the target key from
    /// the target JSON.
    /// <para />
    /// A boolean return value of <c>false</c> indicates that the parsing failed
    /// and the given default value was returned.
    /// </returns>
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
