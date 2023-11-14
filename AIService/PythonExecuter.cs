using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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


        public PythonExecuter()
        {
            // ../usr/lib/x86_64-linux-gnu/libpython3.9.so.1.0
            pythonDllPath = Path.Combine(Directory.GetCurrentDirectory(), "../AIService/PythonDll/python310.dll");
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
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDllPath);
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
    }
}
