using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using thrifty.common.utils;

namespace thrifty.common.config;

internal record SmithingWasteRecoveryConfig(
  ImmutableHashSet<string> disallowedRecipes
) {
  internal const string ConfigKeyDisallowedRecipes = "disallowedRecipes";

  internal static SmithingWasteRecoveryConfig defaultInstance() => new(ImmutableHashSet<string>.Empty);

  internal void toYAML(StringWriter stream, Indent indent) {
    indent++;

    stream.WriteLine();
    stream.WriteLine();

    stream.Write(indent);
    stream.WriteLine("# Recipes that should not reward scrap bits on crafting.");

    stream.Write(indent);
    stream.WriteLine("#");

    stream.Write(indent);
    stream.WriteLine("# For example, adding the recipe game:knifeblade-copper would mean that no scrap would be");
    stream.Write(indent);
    stream.WriteLine("# rewarded on completion of the recipe.");

    stream.Write(indent);
    stream.Write(ConfigKeyDisallowedRecipes);

    if (disallowedRecipes.Count == 0) {
      stream.WriteLine(": []");
      indent--;
      return;
    }

    stream.WriteLine(":");

    foreach (var recipe in disallowedRecipes) {
      stream.Write(indent);
      stream.Write("- ");
      stream.WriteLine(recipe);
    }

    indent--;
  }
}
