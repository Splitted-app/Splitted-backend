﻿using Models.Entities;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.RepositoriesTests.Fixtures
{
    public class BudgetRepositoryGetFixture
    {
        public IRepositoryWrapper repositoryWrapper { get; private set; }


        public BudgetRepositoryGetFixture()
        {
            repositoryWrapper = new RepositoryWrapper(SplittedDbContextMock.GetMockedDbContext());
        }

    }
}
