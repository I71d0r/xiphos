using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xiphos.Areas.Administration.Controllers;
using Xiphos.Areas.Administration.Models;
using Xiphos.Data.Models;
using Xiphos.Data.ProductDatabase;
using Xiphos.Shared.Authentication;
using Xunit;

namespace Xiphos.Tests.Areas.Administration.Controllers
{
    // --Notable--
    // An example how to orchestrate the test environment in order to test what model
    // will controller actions provide assuming parameters and database data.
    // Database tests are based on in-memory database.
    // Tests utilize asynchronous workflow.
    public class MelodyControllerTests : IAsyncLifetime
    {
        private ProductDbContext _dbContext;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: "Xiphos_Tests")
                .Options;

            _dbContext = new ProductDbContext(options);
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Melodies.AddRangeAsync(Data.AllMelodies);
            await _dbContext.SaveChangesAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
            => await _dbContext.DisposeAsync();

        [Theory]
        [InlineData(null, new[] { 10, 15, 1 })]
        [InlineData(Sort.Property.Name + "_" + Sort.Direction.Ascending, new[] { 10, 15, 1 })]
        [InlineData(Sort.Property.Name + "_" + Sort.Direction.Descending, new[] { 1, 15, 10 })]
        [InlineData(Sort.Property.Id + "_" + Sort.Direction.Ascending, new[] { 1, 10, 15 })]
        [InlineData(Sort.Property.Id + "_" + Sort.Direction.Descending, new[] { 15, 10, 1 })]
        public async Task Index_SortingTest(string sort, int[] expectedMelodyIds)
        {
            var testData = Data.AllMelodies;

            using var controller = MakeAdminController();

            var view = (ViewResult)await controller.Index(sort, null, 0, testData.Length);
            var model = view.Model as MelodyListModel;

            Assert.NotNull(model);
            Assert.Equal(testData.Length, model.TotalCount);
            Assert.Equal(expectedMelodyIds.Length, model.Melodies.Count);
            AssertMelodies(expectedMelodyIds, testData, model);
        }

        [Theory]
        [InlineData(null, 3, new[] { 10, 15, 1 })]
        [InlineData("AA", 1, new[] { 10 })]
        public async Task Index_FilteringTest(string filter, int expectedTotalCount, int[] expectedMelodyIds)
        {
            var testData = Data.AllMelodies;

            using var controller = MakeAdminController();

            var view = (ViewResult)await controller.Index(null, filter, 0, testData.Length);
            var model = view.Model as MelodyListModel;

            Assert.NotNull(model);
            Assert.Equal(expectedTotalCount, model.TotalCount);
            Assert.Equal(expectedMelodyIds.Length, model.Melodies.Count);
            AssertMelodies(expectedMelodyIds, testData, model);
        }

        [Theory]
        [InlineData(0, 2, new[] { 10, 15 })]
        [InlineData(1, 2, new[] { 1 })]
        public async Task Index_PagingTest(int pageIndex, int pageSize, int[] expectedMelodyIds)
        {
            var testData = Data.AllMelodies;

            using var controller = MakeAdminController();

            var view = (ViewResult)await controller.Index(null, null, pageIndex, pageSize);
            var model = view.Model as MelodyListModel;

            Assert.NotNull(model);
            Assert.Equal(testData.Length, model.TotalCount);
            Assert.Equal(expectedMelodyIds.Length, model.Melodies.Count);
            AssertMelodies(expectedMelodyIds, testData, model);
        }

        private static void AssertMelodyEqual(MelodyModel expected, MelodyModel got)
        {
            Assert.Equal(expected.Id, got.Id);
            Assert.Equal(expected.Name, got.Name);
            Assert.Equal(expected.Data, got.Data);
        }

        private static void AssertMelodies(
            IReadOnlyList<int> expectedMelodyIds,
            MelodyModel[] testData,
            MelodyListModel model)
        {
            var i = 0;

            foreach (var melody in model.Melodies)
            {
                var expected = testData.Single(m => m.Id == expectedMelodyIds[i]);
                AssertMelodyEqual(expected, melody);
                i++;
            }
        }

        // Adds claims and context to deal with non-attribute authorization permission checks
        private MelodyController MakeAdminController()
            => new MelodyController(_dbContext, new Mock<ILogger<MelodyController>>().Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new[]
                                {
                                    new Claim(ClaimTypes.Role, UserRoles.Administrator)
                                }))
                    }
                }
            };

        private static class Data
        {
            public static MelodyModel[] AllMelodies => new[]
            {
                new MelodyModel { Id = 1, Name = "xXx", Data = "C# Gb B" },
                new MelodyModel { Id = 10, Name = "AAA", Data = "C D E" },
                new MelodyModel { Id = 15, Name = "bbb", Data = "A A A" }
            };
        }
    }
}
