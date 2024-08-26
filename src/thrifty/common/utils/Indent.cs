using System.Collections.Generic;

namespace thrifty.common.utils;

internal class Indent {
  private readonly List<string> cache = new(8) {
    "",
    "  ",
    "    ",
    "      ",
    "        ",
    "          ",
    "            ",
    "              ",
  };

  private byte currentIndent = 0;

  public static implicit operator string(Indent indent) => indent.cache[indent.currentIndent];

  public static Indent operator ++(Indent self) {
    self.currentIndent++;
    self.populateCache();
    return self;
  }

  public static Indent operator --(Indent self) {
    self.currentIndent--;
    return self;
  }

  private void populateCache() {
    var count = currentIndent - (cache.Count - 1);

    if (count < 1)
      return;

    var add = cache[1];
    var biggest = cache[^1];

    for (var i = 0; i < count; i++) {
      biggest += add;
      cache.Add(biggest);
    }
  }
}
