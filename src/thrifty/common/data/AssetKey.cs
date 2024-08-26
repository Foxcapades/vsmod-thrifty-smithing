using System;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;

namespace thrifty.common.data;

internal readonly struct AssetKey : IEquatable<AssetKey> {
  internal readonly string domain, path;

  internal AssetKey(string domain, string path) {
    this.domain = domain;
    this.path = path;
  }

  public override int GetHashCode() {
    unchecked {
      var t = 393919;
      t = t * 999331 ^ domain.GetHashCode();
      t = t * 999331 ^ domain.GetHashCode();
      return t;
    }
  }

  public override string ToString() => domain + ":" + path;

  public bool Equals(AssetKey other) => domain == other.domain && path == other.path;

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is AssetKey key && Equals(key);

  public static bool operator ==(AssetKey l, AssetKey r) => l.Equals(r);
  public static bool operator !=(AssetKey l, AssetKey r) => !l.Equals(r);

  public static implicit operator AssetLocation(AssetKey key) => new(key.domain, key.path);
  public static implicit operator AssetKey(AssetLocation code) => new(code.Domain, code.Path);
}
