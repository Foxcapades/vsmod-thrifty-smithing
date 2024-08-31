namespace thrifty.common.util;

internal static class Formatting {
  internal static string toUserString(this float value, byte n = 2) {
    if (n == 0) {
      return ((int) value).ToString();
    }

    var original = string.Format("N{0}", n);

    for (var i = original.Length - 1; i > -1; i--) {
      if (original[i] == '.')
        return original[..i];

      if (original[i] != '0')
        return original[..(i + 1)];
    }

    return original;
  }
}
