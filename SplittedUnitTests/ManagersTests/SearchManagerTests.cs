using FluentAssertions;
using Splitted_backend.Managers;
using Splitted_backend.Models.Entities;
using SplittedUnitTests.Data.FakeManagersData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.ManagersTests
{
    public class SearchManagerTests
    {
        [Theory]
        [InlineData("robert", "7645b7b5-2d7b-4122-8e16-cef9df13bb4c", 0)]
        [InlineData("mateusz", "37320745-d376-4bde-9346-07870ea926a5", 3)]
        [InlineData("mateusz", "3da39e43-2299-445a-a9d1-83bbc0671afa", 2)]
        [InlineData("kasia", "37320745-d376-4bde-9346-07870ea926a5", 3)]
        [InlineData("kasia", "e449ef7c-665e-4502-8347-4b33a1b715a9", 2)]
        [InlineData("use", "169115ae-0921-4f8d-ad3b-cd3b72c1a943", 4)]
        public void Test_SearchUsers(string query, string userId, int expectedCount)
        {
            IQueryable<User> users = FakeUsersData.Users.AsQueryable();

            List<User> foundUsers = SearchManager.SearchUsers(users, query, new Guid(userId));

            foundUsers.Should().NotBeNull();
            foundUsers.Should().HaveCount(expectedCount);
        }
    }
}
