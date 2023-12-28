using Models.Entities;
using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.Data.FakeManagersData
{
    public static class FakeUsersData
    {
        public static List<User> Users = new List<User>
        {
            new User
            {
                Id = new Guid("e0cbb7d6-556d-43e7-82da-6e2b9938e359"),
                Email = "user@example.com",
                UserName = "user",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[0]
                }
            },

            new User
            {
                Id = new Guid("7645b7b5-2d7b-4122-8e16-cef9df13bb4c"),
                Email = "user1@example.com",
                UserName = "user1",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[1]
                }
            },

            new User
            {
                Id = new Guid("e0cbb7d6-556d-43e7-82da-6e2b9938e359"),
                Email = "user2@example.com",
                UserName = "user2",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[2]
                }
            },

            new User
            {
                Id = new Guid("37320745-d376-4bde-9346-07870ea926a5"),
                Email = "user3@example.com",
                UserName = "user3",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[3]
                }
            },

            new User
            {
                Id = new Guid("60942901-7fed-4bf6-8871-4946e95e6695"),
                Email = "mati@gmail.com",
                UserName = "mateusz_g",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[4]
                }
            },

            new User
            {
                Id = new Guid("e449ef7c-665e-4502-8347-4b33a1b715a9"),
                Email = "kasia@gmail.com",
                UserName = "katarzyna_g",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[5]
                }
            },

            new User
            {
                Id = new Guid("169115ae-0921-4f8d-ad3b-cd3b72c1a943"),
                Email = "mati@o2.pl",
                UserName = "mateusz_o",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[6]
                }
            },

            new User
            {
                Id = new Guid("c6b2e9c7-fa88-43c5-887d-bf79eeab8cda"),
                Email = "kasia@o2.pl",
                UserName = "katarzyna_o",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[7]
                }
            },

            new User
            {
                Id = new Guid("3da39e43-2299-445a-a9d1-83bbc0671afa"),
                Email = "mati1@hotmail.com",
                UserName = "mateusz_h",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[8]
                }
            },

            new User
            {
                Id = new Guid("efa593b4-2e79-4e24-a97a-6b40a7443ce5"),
                Email = "kasia1@hotmail.com",
                UserName = "katarzyna_h",
                Budgets = new List<Budget>
                {
                    FakeBudgetsData.Budgets[9]
                }
            },
        };

    }
}
