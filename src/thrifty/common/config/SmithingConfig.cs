using System.Collections.Generic;
using System.IO;
using thrifty.common.utils;

namespace thrifty.common.config;

internal record SmithingConfig(
  MaterialValues defaultMaterialValues,
  ISet<string>   disallowedScrapRecipes
) {
  internal const string ConfigKeyDefaultMaterialValues = "defaultMaterialValues";
  internal const string ConfigKeyDisallowedRecipes     = "disallowedScrapRecipes";

  internal void toYAML(StringWriter stream, Indent indent) {
    indent++;

    stream.WriteLine();
    stream.writeKey(indent, ConfigKeyDefaultMaterialValues);
    defaultMaterialValues.toYAML(stream, indent);

    stream.WriteLine();
    stream.writeKey(indent, ConfigKeyDisallowedRecipes);



    stream.WriteLine("#");
    stream.WriteLine("# Thrifty Smithing Configuration");
    stream.WriteLine("#");
    stream.WriteLine("");
    stream.Write(defaultMaterialValues);
    stream.Write(":");

  }

}
