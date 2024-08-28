using System;
using Vintagestory.API.Datastructures;

namespace thrifty.common.ext;

internal static class XTreeAttribute {
  internal static byte registerType(Type type) {
    var id = (byte) (TreeAttribute.AttributeIdMapping.Count + 1);
    TreeAttribute.RegisterAttribute(id, type);
    return id;
  }
}
