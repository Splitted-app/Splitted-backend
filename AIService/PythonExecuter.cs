using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Models.DTOs.Outgoing.Transaction;
using Models.Entities;
using Python.Runtime;

namespace AIService
{
    public class PythonExecuter
    {
        private string pythonDllPath { get; }

        private string aiCatalogPath { get; }

        private string mainAIModule { get; }


        public PythonExecuter(IConfiguration configuration)
        {
            pythonDllPath = Path.Combine(Directory.GetCurrentDirectory(), configuration["DllPath"]);
            aiCatalogPath = Path.Combine(Directory.GetCurrentDirectory(), "../AIService/AIFiles");
            mainAIModule = Path.GetFileNameWithoutExtension(Path.Combine(aiCatalogPath, "main.py"));
            InitializeExecuter();
        }


        public void TrainAIModel(List<TransactionAITrainDTO> userTransactions)
        {
            using (Py.GIL())
            {
                using (PyModule scope = Py.CreateScope())
                {
                    PyObject aiModule = LoadMainModule();

                    PyObject pyUserTransactions = userTransactions.ToPython();
                    Console.WriteLine(aiModule.InvokeMethod("train_model", pyUserTransactions));
                }
            }
        }

        private PyObject LoadMainModule()
        {
            dynamic sys = Py.Import("sys");
            sys.path.append(aiCatalogPath);
            return Py.Import(mainAIModule);
        }

        private void InitializeExecuter()
        {
            Runtime.PythonDLL = pythonDllPath;
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }
}
