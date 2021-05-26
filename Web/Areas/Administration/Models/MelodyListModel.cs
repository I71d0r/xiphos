using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xiphos.Areas.Administration.Models
{
    /// <summary>
    /// Paged melody set model
    /// </summary>
    public class MelodyListModel
    {
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

        // --Notable--
        // The purpose of this object is to expose enumerated object annotations.
        //  E.g. @Html.DisplayNameFor(m => m.Default.Id)
        public MelodyModel Default => new MelodyModel();

        public static async Task<MelodyListModel> FetchAsync(IQueryable<MelodyModel> melodies, int pageIndex, int pageSize)
        {
            var count = await (melodies ?? throw new ArgumentNullException(nameof(melodies))).CountAsync();

            return new MelodyListModel
            {
                TotalCount = count,
                PageIndex = pageIndex,
                PageCount = (int)Math.Ceiling(1D * count / pageSize),
                PageSize = pageSize,

                // --Notable--
                // AsNoTracking is recommended for any read-only scenarios as it saves construction of the tracking information.
                Melodies = await melodies
                    .AsNoTracking()
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync()
            };
        }
    }
}
