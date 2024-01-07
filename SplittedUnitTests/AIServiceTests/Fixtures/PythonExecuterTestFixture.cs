using AIService;
using Splitted_backend.Interfaces;
using Splitted_backend.Repositories;
using SplittedUnitTests.Data.FakeRepositoriesData;
using SplittedUnitTests.RepositoriesTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedUnitTests.AIServiceTests.Fixtures
{
    public class PythonExecuterTestFixture
    {
        public PythonExecuter pythonExecuter { get; private set; }


        public PythonExecuterTestFixture()
        {
            string pythonDllPath = "../../../Data/FakeAIServiceData/PythonEnv/python310.dll";
            string aiCatalogPath = "../../../Data/FakeAIServiceData/AITestFiles";
            string mainAIModule = "test_main.py";

            pythonExecuter = new PythonExecuter(pythonDllPath, aiCatalogPath, mainAIModule);
        }
    }
}
