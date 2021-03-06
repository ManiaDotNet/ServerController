﻿using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains utitility (extension) methods.
/// </summary>
public static class Util
{
    private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);

    /// <summary>
    /// Appends the item to the end of the source IEnumerable.
    /// </summary>
    /// <typeparam name="TSource">The type of the items in the IEnumerable.</typeparam>
    /// <param name="source">The main Sequence of items.</param>
    /// <param name="item">The item to append to the main Sequence.</param>
    /// <returns>The source Sequence with the item appended to it.</returns>
    public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource item)
    {
        foreach (var sourceItem in source)
            yield return sourceItem;

        yield return item;
    }

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
    /// Removes and returns the last element of a List.
    /// </summary>
    /// <typeparam name="T">List type.</typeparam>
    /// <param name="list">The List.</param>
    /// <returns>The last element of the list.</returns>
    public static T Pop<T>(this List<T> list)
    {
        var last = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return last;
    }

    /// <summary>
    /// Appends an element to a list.
    /// </summary>
    /// <typeparam name="T">List type.</typeparam>
    /// <param name="list">The list.</param>
    /// <param name="item">The item to add.</param>
    public static void Push<T>(this List<T> list, T item)
    {
        list.Add(item);
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