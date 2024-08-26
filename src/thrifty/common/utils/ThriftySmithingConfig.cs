using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using thrifty.debug;
using Vintagestory.API.Datastructures;

namespace thrifty.common.utils;

/**
 * <summary>
 * ThriftySmithing mod configuration.
 * </summary>
 */
internal readonly struct ThriftySmithingConfig {

  #region Constants & Defaults

  /**
   * <summary>
   * Defines the default or fallback value to use when configuring the number of
   * voxels a single ingot represents when placed on an anvil.
   * </summary>
   *
   * <remarks>
   * This value is based on standard Vintage Story behavior and should only be
   * updated if there is a change to this behavior in the base game.
   * </remarks>
   */
  private const byte DefaultVoxelsPerIngot = 42;
  private const string ConfigKeyVoxelsPerIngot = "voxelsPerIngot";

  /**
   * <summary>
   * Defines the default or fallback value to use when configuring the number of
   * voxels a single metal plate represents when placed on an anvil.
   * </summary>
   *
   * <remarks>
   * This value is based on standard Vintage Story behavior and should only be
   * updated if there is a change to this behavior in the base game.
   * </remarks>
   */
  private const byte DefaultVoxelsPerPlate = 81;
  private const string ConfigKeyVoxelsPerPlate = "voxelsPerPlate";

  /**
   * <summary>
   * Defines the default or fallback value to use when configuring the amount or
   * number of units make up a single ingot of a given material.
   * </summary>
   *
   * <remarks>
   * This value is based on standard Vintage Story behavior and should only be
   * updated if there is a change to this behavior in the base game.
   * </remarks>
   */
  private const ushort DefaultMaterialUnitsPerIngot = 100;
  private const string ConfigKeyMaterialUnitsPerIngot = "materialUnitsPerIngot";

  /**
   * <summary>
   * Defines the default or fallback value to use when configuring the amount or
   * number of units are represented by a single 'bit' of a given material.
   * </summary>
   *
   * <remarks>
   * This value is based on standard Vintage Story behavior and should only be
   * updated if there is a change to this behavior in the base game.
   * </remarks>
   */
  private const byte DefaultMaterialUnitsPerBit = 5;
  private const string ConfigKeyMaterialUnitsPerBit = "materialUnitsPerBit";

  /**
   * <summary>
   * Provides the default set of recipes for which ThriftySmithing mod behavior
   * is disallowed.
   * </summary>
   */
  private static IReadOnlySet<string> defaultDisallowedRecipes() {
    return new HashSet<string>(0);
  }

  private const string ConfigKeyDisallowedRecipes = "disallowedRecipes";

  #endregion Constants & Defaults

  #region Configurable Values

  /**
   * <summary>
   * The number of voxels granted when placing a single ingot on an anvil as
   * part of the smithing process.
   * </summary>
   *
   * <value>Must be in the range <c>[0, 255]</c>.</value>
   *
   * <remarks>
   * This value should not be changed unless another mod or customization alters
   * the number of voxels granted when placing an ingot on an anvil.
   *
   * <para />
   *
   * Changing this value to something that does not align with game behavior
   * will cause weird amounts of materials to be returned from smithing recipes.
   * </remarks>
   */
  public readonly byte voxelsPerIngot;

  /**
   * <summary>
   * The number of voxels granted when placing a single metal plate on an anvil
   * as part of the smithing process.
   * </summary>
   *
   * <value>Must be in the range <c>[0, 255]</c>.</value>
   *
   * <remarks>
   * This value should not be changed unless another mod or customization alters
   * the number of voxels granted when placing an ingot on an anvil.
   *
   * <para />
   *
   * Changing this value to something that does not align with game behavior
   * will cause weird amounts of materials to be returned from smithing recipes.
   * </remarks>
   */
  public readonly byte voxelsPerPlate;

  /**
   * <summary>
   * The amount or number of units of material that make up a single ingot.
   * </summary>
   *
   * <value>
   * Must be in the range <c>[1, 65535]</c>.
   * </value>
   *
   * <remarks>
   * This value should not be changed unless another mod or customization alters
   * the amount of material that is required to create a single ingot.
   *
   * <para />
   *
   * Changing this value to something that does not align with game behavior
   * will cause weird amounts of materials to be returned from smithing recipes.
   * </remarks>
   */
  public readonly ushort materialUnitsPerIngot;

  /**
   * <summary>
   * The amount or number of units are represented by a single 'bit' of a given
   * material.
   * </summary>
   *
   * <value>
   * Must be in the range <c>[1, 255]</c>.
   * </value>
   *
   * <remarks>
   * This value should not be changed unless another mod or customization alters
   * the amount of material that is rewarded for smelting or otherwise breaking
   * down a single bit of a given material.
   *
   * <para />
   *
   * Changing this value to something that does not align with game behavior
   * will cause weird amounts of materials to be returned from smithing recipes.
   * </remarks>
   */
  public readonly byte materialUnitsPerBit;

  /**
   * <summary>
   * Recipes (item locations) for which ThriftySmithing mod behaviors are
   * disallowed.
   * </summary>
   */
  public readonly IReadOnlySet<string> disallowedRecipes;

  #endregion Configurable Values

  private ThriftySmithingConfig(
    byte voxelsPerIngot,
    byte voxelsPerPlate,
    ushort materialUnitsPerIngot,
    byte materialUnitsPerBit,
    IReadOnlySet<string> disallowedRecipes
  ) {
    this.voxelsPerIngot = voxelsPerIngot;
    this.voxelsPerPlate = voxelsPerPlate;
    this.materialUnitsPerIngot = materialUnitsPerIngot;
    this.materialUnitsPerBit = materialUnitsPerBit;
    this.disallowedRecipes = disallowedRecipes;
  }

  #region Instance Methods

  internal JsonObject toJSON() {
    var json = new JObject();

    json[ConfigKeyVoxelsPerIngot] = voxelsPerIngot;
    json[ConfigKeyVoxelsPerPlate] = voxelsPerPlate;
    json[ConfigKeyMaterialUnitsPerIngot] = materialUnitsPerIngot;
    json[ConfigKeyMaterialUnitsPerBit] = materialUnitsPerBit;
    json[ConfigKeyDisallowedRecipes] = new JArray(disallowedRecipes);

    return new(json);
  }

  #endregion Instance Methods

  #region "Public" Constructors

  /**
   * <summary>
   * Attempts to use the given JSON value to construct a new
   * <c>ThriftySmithingConfig</c> instance, providing an indicator whether the
   * JSON was valid.
   * </summary>
   *
   * <remarks>
   * This method validates the input JSON, logging any issues and filling in
   * defaults as necessary.  If there were any issues found in the input JSON,
   * this method will return a <c>wasValid</c> value of <c>false</c>.
   * </remarks>
   *
   * <param name="json">
   * Input JSON.  This value will be parsed in an attempt to construct a new
   * <c>ThriftySmithingConfig</c> instance.
   * </param>
   *
   * <returns>
   * A tuple containing a new <c>ThriftySmithingConfig</c> instance and a
   * boolean value indicating whether the input JSON was valid.
   * </returns>
   */
  internal static (ThriftySmithingConfig config, bool wasValid) parseFromJSON(JsonObject json) {
    if (json.Token.Type != JTokenType.Object) {
      Logs.warn("configuration JSON was not an object; using default config");
      return (defaultConfig(), false);
    }

    bool wasValid = true, twv;
    byte vpi, vpp, mupb;
    ushort mupi;
    IReadOnlySet<string> dr;

    (vpi, twv) = parseByte(json.Token, ConfigKeyVoxelsPerIngot, DefaultVoxelsPerIngot);
    wasValid = wasValid && twv;

    (vpp, twv) = parseByte(json.Token, ConfigKeyVoxelsPerPlate, DefaultVoxelsPerPlate);
    wasValid = wasValid && twv;

    (mupi, twv) = parseUShort(json.Token, ConfigKeyMaterialUnitsPerIngot, DefaultMaterialUnitsPerIngot);
    wasValid = wasValid && twv;

    (mupb, twv) = parseByte(json.Token, ConfigKeyMaterialUnitsPerBit, DefaultMaterialUnitsPerBit);
    wasValid = wasValid && twv;

    (dr, twv) = parseDisallowedRecipes(json.Token);
    wasValid = wasValid && twv;

    return (new ThriftySmithingConfig(vpi, vpp, mupi, mupb, dr), wasValid);
  }

  /**
   * <summary>
   * Creates a new <c>ThriftySmithingConfig</c> instance using default values
   * for all config options.
   * </summary>
   *
   * <returns>
   * A new, defaulted <c>ThriftySmithingConfig</c> instance.
   * </returns>
   */
  internal static ThriftySmithingConfig defaultConfig() =>
    new(
      DefaultVoxelsPerIngot,
      DefaultVoxelsPerPlate,
      DefaultMaterialUnitsPerIngot,
      DefaultMaterialUnitsPerBit,
      defaultDisallowedRecipes()
    );

  #endregion "Public" Constructors

  #region JSON Parsing Helpers

  private static (byte value, bool wasValid) parseByte(JToken json, string key, byte fallback) {
    var (parsedValue, usedDefault) = tryInt(json, key, fallback);

    if (!usedDefault && parsedValue is < 1 or > 255) {
      Logs.warn(
        "configuration JSON had an invalid value for key \"{0}\", must be in "
        + "the range [0, 255]; using default value \"{1}\"",
        key,
        fallback
      );

      return (fallback, false);
    }

    return ((byte) parsedValue, !usedDefault);
  }

  private static (ushort value, bool wasValid) parseUShort(JToken json, string key, ushort fallback) {
    var (parsedValue, usedDefault) = tryInt(json, key, fallback);

    if (!usedDefault && parsedValue is < 1 or > 65535) {
      Logs.warn(
        "configuration JSON had an invalid value for key \"{0}\", must be in "
        + "the range [1, 65535]; using default value \"{1}\"",
        key,
        fallback
      );

      return (fallback, false);
    }

    return ((ushort) parsedValue, !usedDefault);
  }

  private static (IReadOnlySet<string>, bool wasValid) parseDisallowedRecipes(JToken json) {
    if (json[ConfigKeyDisallowedRecipes] == null) {
      Logs.info("configuration JSON had no value \"{0}\", using default value", ConfigKeyDisallowedRecipes);
      return (defaultDisallowedRecipes(), false);
    }

    var recipeArray = json[ConfigKeyDisallowedRecipes]!;

    if (recipeArray.Type != JTokenType.Array) {
      Logs.warn(@"configuration JSON ""{0}"" value was set to a non-array value, using default value", ConfigKeyDisallowedRecipes);
      return (defaultDisallowedRecipes(), false);
    }

    var disallowedRecipes = new List<string>(((JArray) recipeArray).Count);
    var wasValid = true;

    foreach (var token in (JArray) recipeArray) {
      if (token.Type != JTokenType.String) {
        Logs.warn("configuration JSON \"{0}\" array contained a non-string value, ignoring that value", ConfigKeyDisallowedRecipes);
        wasValid = false;
        continue;
      }

      disallowedRecipes.Add(token.Value<string>()!);
    }

    return (new HashSet<string>(disallowedRecipes), wasValid);
  }

  /**
   * <summary>
   * Tries to parse a target <paramref name="key"/> from the given JSON object
   * (<paramref name="json"/>) as an integer value, falling back to the given
   * <paramref name="defaultValue"/> if necessary due to the target value being
   * absent or invalid.
   * </summary>
   *
   * <param name="json">
   * Configuration JSON expected to contain the target <paramref name="key"/>.
   * </param>
   *
   * <param name="key">
   * Key string for the target value.
   * </param>
   *
   * <param name="defaultValue">
   * Fallback value to use if the target <paramref name="key"/> is absent or
   * points to a non-integer value.
   * </param>
   *
   * <returns>
   * A tuple containing a usable int value and a boolean flag indicating whether
   * the provided <paramref name="defaultValue"/> was used.
   * </returns>
   */
  private static (int value, bool usedDefault) tryInt(JToken json, string key, int defaultValue) {
    if (json[key] == null) {
      Logs.info("configuration JSON had no value for key \"{0}\", using default value \"{1}\"", key, defaultValue);
      return (defaultValue, true);
    }

    var value = json[key]!;

    switch (value.Type) {
      case JTokenType.Integer: {
        return (value.Value<int>(), false);
      }

      case JTokenType.Float: {
        var floatVal = value.Value<float>();
        var intVal = (int) floatVal;
        Logs.warn("configuration JSON \"{0}\" value was set to the float value \"{1}\" instead of an int; trimming value to \"{2}\"", key, floatVal, intVal);
        return (intVal, true);
      }

      default:
        Logs.warn("configuration JSON \"{0}\" value was set to a non-integer value, falling back to default value \"{1}\"", key, defaultValue);
        return (defaultValue, true);
    }
  }

  #endregion JSON Parsing Helpers
}
