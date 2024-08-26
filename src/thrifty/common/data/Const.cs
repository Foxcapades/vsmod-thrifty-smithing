namespace thrifty.common.data;

internal static class Const {
  internal static class Harmony {
    internal static class Category {
      internal const string Server = "server";
    }
  }

  internal static class Attribute {
    /// <summary>
    /// Used by:
    /// <list>
    ///   <item>
    ///     <term><c>BlockSmeltedContainer.SetContents</c></term>
    ///     <description>
    ///       Sets the contents of the smelted container <c>ItemStack</c> to an
    ///       <c>ItemStack</c> for the smelted item type (likely an ingot).
    ///     </description>
    ///   </item>
    /// </list>
    /// </summary>
    internal const string Output = "output";

    /// <summary>
    /// Used by:
    /// <list>
    ///   <item>
    ///     <term><c>BlockSmeltedContainer.SetContents</c></term>
    ///     <description>
    ///       Sets the number of units of the smelted material on the container
    ///       <c>ItemStack</c>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </summary>
    internal const string Units = "units";

    internal const string Temperature = "temperature";
  }

  internal static class Code {

    internal static class Default {
      internal const string Domain = "game";

      internal const string MetalBitPathPrefix   = "metalbit";
      internal const string MetalPlatePathPrefix = "metalplate";
      internal const string IngotPathPrefix      = "ingot";
    }
  }
}
