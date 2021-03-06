﻿using System;

namespace LiveDocs.Shared
{
    public static class StringHelper
    {
        /// <summary>
        /// Get the Damerau-Levenshtein distance between two string of text.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <remarks>Source: https://www.csharpstar.com/csharp-string-distance-algorithm/ </remarks>
        public static int DamerauLevenshteinDistance(string source, string target, int maxDistance = int.MaxValue)
        {
            if (Math.Abs(source.Length - target.Length) > maxDistance)
                return -1;

            var bounds = new { Height = source.Length + 1, Width = target.Length + 1 };

            int[,] matrix = new int[bounds.Height, bounds.Width];

            for (int height = 0; height < bounds.Height; height++) { matrix[height, 0] = height; };
            for (int width = 0; width < bounds.Width; width++) { matrix[0, width] = width; };

            for (int height = 1; height < bounds.Height; height++)
            {
                for (int width = 1; width < bounds.Width; width++)
                {
                    int cost = (source[height - 1] == target[width - 1]) ? 0 : 1;
                    int insertion = matrix[height, width - 1] + 1;
                    int deletion = matrix[height - 1, width] + 1;
                    int substitution = matrix[height - 1, width - 1] + cost;

                    int distance = Math.Min(insertion, Math.Min(deletion, substitution));

                    if (height > 1 && width > 1 && source[height - 1] == target[width - 2] && source[height - 2] == target[width - 1])
                    {
                        distance = Math.Min(distance, matrix[height - 2, width - 2] + cost);
                    }

                    matrix[height, width] = distance;
                }
            }

            return matrix[bounds.Height - 1, bounds.Width - 1];
        }
    }
}