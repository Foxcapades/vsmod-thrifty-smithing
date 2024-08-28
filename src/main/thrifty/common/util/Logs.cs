using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;

namespace thrifty.common.util;

/// <summary>
/// Logging facade.
/// </summary>
internal static class Logs {
  private static ILogger? logger;

  #if ENABLE_DEBUG_FEATURES
  private const string TracePattern = "{0}: {1}";
  #endif

  internal static void init(ILogger logger) {
    Logs.logger = logger;
  }

  /// <summary>
  /// Log a message at trace (verbose debug) level.
  /// </summary>
  ///
  /// <remarks>
  /// In production builds this method should be empty and hopefully inlined
  /// away entirely.
  /// </remarks>
  ///
  /// <param name="message">
  /// Message to log.
  /// </param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void trace(string message) {
    #if ENABLE_DEBUG_FEATURES
    var frame = new StackFrame(1);
    logger!.VerboseDebug(TracePattern, frame.GetMethod()!.Name, message);
    #endif
  }

  /// <summary>
  /// Log a message at trace (verbose debug) level.
  /// </summary>
  ///
  /// <remarks>
  /// In production builds this method should be empty and hopefully inlined
  /// away entirely.
  /// </remarks>
  ///
  /// <param name="format">
  /// String format for the message.  This format should follow the rules of the
  /// standard library <c>string.Format</c> method.
  /// </param>
  ///
  /// <param name="p1">
  /// Object to inject into the format string for logging.
  /// </param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void trace(string format, object p1) {
    #if ENABLE_DEBUG_FEATURES
    var frame = new StackFrame(1);
    logger!.VerboseDebug(TracePattern, frame.GetMethod()!.Name, string.Format(format, p1));
    #endif
  }

  /// <summary>
  /// Log a message at trace (verbose debug) level.
  /// </summary>
  ///
  /// <remarks>
  /// In production builds this method should be empty and hopefully inlined
  /// away entirely.
  /// </remarks>
  ///
  /// <param name="format">
  /// String format for the message.  This format should follow the rules of the
  /// standard library <c>string.Format</c> method.
  /// </param>
  ///
  /// <param name="p1">
  /// First object to inject into the format string for logging.
  /// </param>
  ///
  /// <param name="p2">
  /// Second object to inject into the format string for logging.
  /// </param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void trace(string format, object p1, object p2) {
    #if ENABLE_DEBUG_FEATURES
    var frame = new StackFrame(1);
    logger!.VerboseDebug(TracePattern, frame.GetMethod()!.Name, string.Format(format, p1, p2));
    #endif
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void debug(string message) {
    #if ENABLE_DEBUG_FEATURES
    logger!.Debug(message);
    #endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void debug(string format, object p1) {
    #if ENABLE_DEBUG_FEATURES
    logger!.Debug(format, p1);
    #endif
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void debug(string format, object p1, object p2) {
    #if ENABLE_DEBUG_FEATURES
    logger!.Debug(format, p1, p2);
    #endif
  }

  internal static void info(string format, params object[] things) => logger!.Notification(format, things);

  internal static void warn(string message) => logger!.Warning(message);
  internal static void warn(string format, params object[] things) => logger!.Warning(format, things);

  internal static void error(string format, params object[] things) => logger!.Error(format, things);
  internal static void error(Exception err) => logger!.Error(err);
}
