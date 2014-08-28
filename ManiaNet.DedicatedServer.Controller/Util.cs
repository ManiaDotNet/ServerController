using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains utitility (extension) methods.
/// </summary>
public static class Util
{
    private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);

    /// <summary>
    /// Converts a long Unix TimeStamp into a DateTime.
    /// </summary>
    /// <param name="long">The Unix TimeStamp.</param>
    /// <returns>The DateTime representing the same TimeStamp.</returns>
    public static DateTime FromUnixTimeStampToDateTime(this long @long)
    {
        return unixEpoch.AddSeconds(@long);
    }

    /// <summary>
    /// Converts a DateTime into a long Unix TimeStamp.
    /// </summary>
    /// <param name="dateTime">The DateTime.</param>
    /// <returns>The long Unix TimeStamp representing the same TimeStamp.</returns>
    public static long ToUnixTimeStamp(this DateTime dateTime)
    {
        return (long)(dateTime - unixEpoch).TotalSeconds;
    }
}