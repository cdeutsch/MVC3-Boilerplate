using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// This code came from Phil Haack:
/// http://haacked.com/archive/2010/06/10/checking-for-empty-enumerations.aspx
/// Improves the null coalescing operator by allowing us to do this:
///     var theArgument = argument.AsNullIfWhiteSpace() ?? "defaultValue";
/// </summary>
namespace System
{
    public static class EnumerationExtensions
    {
        public static string AsNullIfEmpty(this string items)
        {
            if (String.IsNullOrEmpty(items))
            {
                return null;
            }
            return items;
        }

        public static string AsNullIfWhiteSpace(this string items)
        {
            if (String.IsNullOrWhiteSpace(items))
            {
                return null;
            }
            return items;
        }

        public static IEnumerable<T> AsNullIfEmpty<T>(this IEnumerable<T> items)
        {
            if (items == null || !items.Any())
            {
                return null;
            }
            return items;
        }
    }
}