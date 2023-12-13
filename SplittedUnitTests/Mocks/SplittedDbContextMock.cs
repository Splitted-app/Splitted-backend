using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Models.Entities;
using Moq;
using Splitted_backend.DbContexts;
using SplittedUnitTests.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.Mocks
{
    public static class SplittedDbContextMock
    {
        public static SplittedDbContext GetMockedDbContext()
        {
            Mock<DbSet<Budget>> budgetSetMock = FakeBudgetsData.Budgets.BuildMockDbSet();

            Mock<SplittedDbContext> splittedDbContextMock = new Mock<SplittedDbContext>();
            MockSetOnDbContext(splittedDbContextMock, budgetSetMock);

            return splittedDbContextMock.Object;
        }

        private static void MockSetOnDbContext<T>(Mock<SplittedDbContext> splittedDbContextMock, Mock<DbSet<T>> dbSetMock)
            where T : class
        {
            splittedDbContextMock.Setup(c => c.Set<T>()).Returns(dbSetMock.Object);
        }
    }
}
