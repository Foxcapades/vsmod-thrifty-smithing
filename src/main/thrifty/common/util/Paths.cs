using Vintagestory.API.Common;

namespace thrifty.common.util;

internal static class Paths {
  internal static string lastPathEntry(this AssetLocation location) =>
    location.Path.Substring(location.Path.LastIndexOf('-') + 1);

  internal static string makePathOf(string prefix, string variant) =>
    prefix + "-" + variant;

  internal static string firstPathEntry(AssetLocation location) {
    var pos = location.Path.IndexOf('-');
    return pos < 1 ? location.Path : location.Path[..pos];
  }
}
