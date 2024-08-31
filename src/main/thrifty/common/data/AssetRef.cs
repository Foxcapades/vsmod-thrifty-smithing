using Vintagestory.API.Common;

namespace thrifty.common.data;

public readonly record struct AssetRef(string domain, string path) {
  public override string ToString() => domain + ":" + path;

  public static implicit operator AssetLocation(AssetRef asset) => new(asset.domain, asset.path);

  public static implicit operator AssetRef(AssetLocation asset) => new(asset.Domain, asset.Path);

  public static implicit operator string(AssetRef asset) => asset.ToString();
}
