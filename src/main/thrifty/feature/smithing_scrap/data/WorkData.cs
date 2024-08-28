using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace thrifty.feature.smithing_scrap.data;

internal struct WorkData : IAttribute {
  internal const string AttributeKey = "ef.ts.workData";

  internal byte ingotCount = 0;

  internal byte plateCount = 0;

  internal bool hasInputs => ingotCount + plateCount > 0;

  public WorkData() { }

  internal WorkData(byte ingotCount, byte plateCount) {
    this.ingotCount = ingotCount;
    this.plateCount = plateCount;
  }

  public void ToBytes(BinaryWriter stream) {
    stream.Write(ingotCount);
    stream.Write(plateCount);
  }

  public void FromBytes(BinaryReader stream) {
    ingotCount = stream.ReadByte();
    plateCount = stream.ReadByte();
  }

  public int GetAttributeId() => SmithingScrapFeature.attributeID;

  public object GetValue() => this;

  public string ToJsonToken() => $"[{ingotCount}, {plateCount}]";

  public bool Equals(IWorldAccessor worldForResolve, IAttribute attr) =>
    attr is WorkData tmp
    && ingotCount == tmp.ingotCount
    && plateCount == tmp.plateCount;

  public IAttribute Clone() => new WorkData(ingotCount, plateCount);

  public override string ToString() => $"WorkData(ingots={ingotCount}, plates={plateCount})";
}
