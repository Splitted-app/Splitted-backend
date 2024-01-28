using AIService;
using Models.CsvModels;
using Models.DTOs.Outgoing.Transaction;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittedIntegrationTests.Mocks
{
    public static class PythonExecuterMock
    {
        public static PythonExecuter GetMockedPythonExecuter()
        {
            Mock<PythonExecuter> pythonExecuterMock = new Mock<PythonExecuter>("path", "path", "module");

            pythonExecuterMock.Protected()
                .Setup("InitializeExecuter");

            pythonExecuterMock.Setup(pe => pe.TrainModel(It.IsAny<List<TransactionAITrainDTO>>(), It.IsAny<string>()));
            pythonExecuterMock.Setup(pe => pe.CategorizeTransactions(It.IsAny<List<TransactionCsv>>(),
                It.IsAny<List<TransactionAITrainDTO>>(), It.IsAny<string>()));

            return pythonExecuterMock.Object;
        }
    }
}
