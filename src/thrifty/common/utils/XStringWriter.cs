using System.IO;

namespace thrifty.common.utils;

internal static class XStringWriter {
  internal static void write(this StringWriter stream, Indent indent, string text) {
    stream.Write(indent);
    stream.Write(text);
  }

  internal static void writeKey(this StringWriter stream, string text) {
    stream.Write(text);
    stream.Write(":");
  }

  internal static void writeKey(this StringWriter stream, Indent indent, string text) {
    stream.Write(indent);
    stream.Write(text);
    stream.Write(":");
  }

  internal static void writeLine(this StringWriter stream, Indent indent, string text) {
    stream.Write(indent);
    stream.WriteLine(text);
  }

  internal static void writeLine(this StringWriter stream, Indent indent, string text, int value) {
    stream.Write(indent);
    stream.Write(text);
    stream.Write(": ");
    stream.WriteLine(value);
  }
}
