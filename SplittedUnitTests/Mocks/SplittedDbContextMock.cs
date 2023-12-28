using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Models.Entities;
using Models.Interfaces;
using Moq;
using Splitted_backend.DbContexts;
using SplittedUnitTests.Data;
using SplittedUnitTests.Data.FakeRepositoriesData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.Mocks
{
    public static class SplittedDbContextMock
    {
        public static SplittedDbContext GetMockedDbContext(List<Budget> budgets, List<Transaction> transactions, 
            List<Goal> goals)
        {
            Mock<DbSet<Budget>> budgetSetMock = MockDbSet(budgets.ConvertAll(b => (Budget)b.Clone()));
            Mock<DbSet<Transaction>> transactionSetMock = MockDbSet(transactions
                .ConvertAll(t => (Transaction)t.Clone()));
            Mock<DbSet<Goal>> goalSetMock = MockDbSet(goals.ConvertAll(g => (Goal)g.Clone()));


            Mock<SplittedDbContext> splittedDbContextMock = new Mock<SplittedDbContext>();
            MockSetOnDbContext(splittedDbContextMock, budgetSetMock);
            MockSetOnDbContext(splittedDbContextMock, transactionSetMock);
            MockSetOnDbContext(splittedDbContextMock, goalSetMock);

            return splittedDbContextMock.Object;
        }

        private static Mock<DbSet<T>> MockDbSet<T>(List<T> entitiesList) where T : class, IEntity
        {
            Mock<DbSet<T>> entitySetMock = entitiesList.AsQueryable().BuildMockDbSet();

            entitySetMock.Setup(c => c.Add(It.IsAny<T>()))
                .Callback((T entity) => entitiesList.Add(entity));
            entitySetMock.Setup(c => c.AddRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
                .Callback((IEnumerable<T> entities, CancellationToken _) => entitiesList.AddRange(entities))
                .Returns(Task.CompletedTask);
            entitySetMock.Setup(c => c.Remove(It.IsAny<T>()))
                .Callback((T entity) => entitiesList
                .RemoveAll(e => e.Id.Equals(entity.Id)));
            entitySetMock.Setup(c => c.RemoveRange(It.IsAny<IEnumerable<T>>()))
                .Callback((IEnumerable<T> entities) => entitiesList
                .RemoveAll(e => entities.Any(en => en.Id.Equals(e.Id))));
            entitySetMock.Setup(c => c.Update(It.IsAny<T>()))
                .Callback((T entity) =>
            {
                int index = entitiesList.FindIndex(e => e.Id.Equals(entity.Id)); 
                if (index != -1)
                    entitiesList[index] = entity;
            });

            return entitySetMock;
        }

        private static void MockSetOnDbContext<T>(Mock<SplittedDbContext> splittedDbContextMock, 
            Mock<DbSet<T>> dbSetMock) where T : class
        {
            splittedDbContextMock.Setup(c => c.Set<T>()).Returns(dbSetMock.Object);
        }
    }
}
