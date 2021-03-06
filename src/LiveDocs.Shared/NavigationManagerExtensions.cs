﻿// SOURCE: https://chrissainty.com/working-with-query-strings-in-blazor/
// AUTHOR: Chris Sainty
// DATE: 2020-07-01

using Microsoft.AspNetCore.Components;
using System.Web;

namespace LiveDocs.Shared
{
    public static class NavigationManagerExtensions
    {
        public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T value)
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);
            var valueFromQueryString = HttpUtility.ParseQueryString(uri.Query).Get(key);
            if (valueFromQueryString != null)
            {
                if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
                {
                    value = (T)(object)valueAsInt;
                    return true;
                }

                if (typeof(T) == typeof(string))
                {
                    value = (T)(object)valueFromQueryString.ToString();
                    return true;
                }

                if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
                {
                    value = (T)(object)valueAsDecimal;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
