using System;

namespace Xiphos.Areas.Administration.Controllers
{
    /*
     * --Notable--
     * To get the sorting more organized the text processing logic is extracted to this class.
     * It could be enum based but in this rather internal string to string scenario it would be
     * unnecessary complicated with conversions.
     *
     * For more complicated scenarios or in order to reuse such code this logic can
     * form an encapsulating component. This solution is somewhere in the middle.
     */

    /// <summary>
    /// Sorting helper
    /// </summary>
    internal static class Sort
    {
        /// <summary>
        /// Sorting properties
        /// </summary>
        public static class Property
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string Default = Name;
        }

        /// <summary>
        /// Sorting directions
        /// </summary>
        public static class Direction
        {
            public const string Ascending = "asc";
            public const string Descending = "desc";
            public const string Default = Ascending;
        }

        /// <summary>
        /// Parses the sorting property and direction.
        /// </summary>
        /// <param name="sort">Srt to parse</param>
        /// <param name="property">Output property</param>
        /// <param name="direction">Output direction</param>
        public static void ParseOrDefault(string sort, out string property, out string direction)
        {
            var sortProp = string.IsNullOrWhiteSpace(sort) ? GetSort(Property.Default, Direction.Default) : sort;
            var parts = sortProp.Split('_');

            if (parts.Length != 2)
            {
                parts = new[] { Property.Default, Direction.Default };
            }

            property = parts[0].ToLowerInvariant() switch
            {
                Property.Id => Property.Id,
                Property.Name => Property.Name,
                _ => Property.Default,
            };

            direction = parts[1].ToLowerInvariant() switch
            {
                Direction.Ascending => Direction.Ascending,
                Direction.Descending => Direction.Descending,
                _ => Direction.Default,
            };
        }

        /// <summary>
        /// Renders the sort string
        /// </summary>
        /// <param name="property">Sort property</param>
        /// <param name="direction">Sort direction</param>
        /// <returns>Sort string</returns>
        public static string GetSort(string property, string direction)
            => $"{property}_{direction}";

        /// <summary>
        /// Flips given direction to the opposite one
        /// </summary>
        /// <param name="direction">Direction to flip</param>
        /// <returns>Opposite direction</returns>
        public static string FlipDirection(string direction)
            => direction == Direction.Ascending ? Direction.Descending : Direction.Ascending;

        /// <summary>
        /// Gets a HTML sorting arrow for a property based on whether and how is currently used for sorting.
        /// </summary>
        /// <param name="decoratedProperty">Property in question</param>
        /// <param name="sortingProperty">Actual sorting property</param>
        /// <param name="direction">Actual sorting direction</param>
        /// <returns></returns>
        public static string GetSortingArrow(string decoratedProperty, string sortingProperty, string direction)
            => decoratedProperty.Equals(sortingProperty, StringComparison.OrdinalIgnoreCase) == false ?
                string.Empty : direction switch
                {
                    Sort.Direction.Ascending => "&#8679;",  // ⇧
                    Sort.Direction.Descending => "&#8681;", // ⇩
                    _ => string.Empty
                };
    }
}
