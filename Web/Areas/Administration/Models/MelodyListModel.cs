using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiphos.Areas.Administration.Controllers;
using Xiphos.Data.Models;

namespace Xiphos.Areas.Administration.Models
{
    /// <summary>
    /// Paged melody set model
    /// </summary>
    public class MelodyListModel
    {
        private const int DefaultPageSize = 5;

        /// <summary>
        /// Total count of melodies fetched
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Current page index
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// Total count of pages
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        /// Page item count
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Items data
        /// </summary>
        public IList<MelodyModel> Melodies { get; private set; }

        /// <summary>
        /// Previous page button is disabled
        /// </summary>
        public bool PreviousDisabled => PageIndex <= 0;

        /// <summary>
        /// Next page button is disabled
        /// </summary>
        public bool NextDisabled => PageIndex >= PageCount - 1;

        /// <summary>
        /// Current sorting
        /// </summary>
        public string CurrentSort { get; private set; }

        /// <summary>
        /// Sorting applied when you click on Id column header
        /// </summary>
        public string IdSort { get; private set; }

        /// <summary>
        /// Sorting applied when you click on Name column header
        /// </summary>
        public string NameSort { get; private set; }

        /// <summary>
        /// Curently applied filter string
        /// </summary>
        public string CurrentFilter { get; private set; }

        /// <summary>
        /// Id sorting direction string
        /// </summary>
        public string IdArrow { get; private set; }

        /// <summary>
        /// Name sorting direction string
        /// </summary>
        public string NameArrow { get; private set; }

        // --Notable--
        // The purpose of this object is to expose enumerated object annotations.
        //  E.g. @Html.DisplayNameFor(m => m.Default.Id)
        public MelodyModel Default => new MelodyModel();

        public static async Task<MelodyListModel> FetchAsync(
            IQueryable<MelodyModel> melodies,
            string sort,
            string filter,
            int? pageIndex,
            int? pageSize)
        {
            if (melodies == null) throw new ArgumentNullException(nameof(melodies));

            var result = new MelodyListModel()
            {
                PageIndex = pageIndex.HasValue && pageIndex > 0 ? pageIndex.Value : 0,
                PageSize = pageSize.HasValue && pageSize > 0 ? pageSize.Value : DefaultPageSize
            };

            Sort.ParseOrDefault(sort, out string property, out string direction);

            // These properties will be rendered on column headers, they represent what should
            // be the sort when you click on the header, not what is the sort right now.
            // Either we are sorting by {propertyX} then flip direction,
            // or we are not, then set sort to {propertyX} with default direction.
            result.IdSort = Sort.GetSort(
                property: Sort.Property.Id,
                direction: property == Sort.Property.Id ? Sort.FlipDirection(direction) : Sort.Direction.Default);

            result.NameSort = Sort.GetSort(
                property: Sort.Property.Name,
                direction: property == Sort.Property.Name ? Sort.FlipDirection(direction) : Sort.Direction.Default);

            result.CurrentSort = Sort.GetSort(property, direction);
            result.IdArrow = Sort.GetSortingArrow(Sort.Property.Id, property, direction);
            result.NameArrow = Sort.GetSortingArrow(Sort.Property.Name, property, direction);
            result.CurrentFilter = filter ?? string.Empty;

            // Sorting
            melodies = property switch
            {
                Sort.Property.Id => direction switch
                {
                    Sort.Direction.Ascending => melodies.OrderBy(m => m.Id),
                    Sort.Direction.Descending => melodies.OrderByDescending(m => m.Id),
                    _ => melodies
                },
                Sort.Property.Name => direction switch
                {
                    Sort.Direction.Ascending => melodies.OrderBy(m => m.Name),
                    Sort.Direction.Descending => melodies.OrderByDescending(m => m.Name),
                    _ => melodies
                },
                _ => melodies
            };

            // Search filter
            if (!string.IsNullOrEmpty(filter))
            {
                // Consider that case sensitivity of contains depends on data source and/or used collation.
                melodies = melodies.Where(m => m.Name.Contains(filter));
            }

            result.TotalCount = await (melodies ?? throw new ArgumentNullException(nameof(melodies))).CountAsync();
            result.PageCount = Math.Max(1, (int)Math.Ceiling(1D * result.TotalCount / result.PageSize));

            // --Notable--
            // AsNoTracking is recommended for any read-only scenarios as it saves construction of the tracking information.
            result.Melodies = await melodies
                    .AsNoTracking()
                    .Skip(result.PageIndex * result.PageSize)
                    .Take(result.PageSize)
                    .ToListAsync();

            return result;
        }
    }
}
