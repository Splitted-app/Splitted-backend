using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Models.CsvModels;
using Models.DTOs.Outgoing.Transaction;
using Models.Entities;
using Python.Runtime;
using Splitted_backend.Models.Entities;

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


        public void TrainModel(List<TransactionAITrainDTO> userTransactions, string userId)
        {
            using (Py.GIL())
            {
                using (PyModule scope = Py.CreateScope())
                {
                    dynamic aiModule = LoadMainModule();

                    PyObject pyUserTransactions = userTransactions.ToPython();
                    aiModule.train_model(pyUserTransactions, userId);
                }
            }
        }

        public void CategorizeTransactions(List<TransactionCsv> importedTransactions, 
            List<TransactionAITrainDTO> importedAiTransactions, string userId)
        {
            using (Py.GIL())
            {
                using (PyModule scope = Py.CreateScope())
                {
                    dynamic aiModule = LoadMainModule();

                    PyObject pyImportedTransactions = importedAiTransactions.ToPython();
                    string?[] autoCategories = aiModule.predict_categories(pyImportedTransactions, userId);

                    if (autoCategories is not null)
                        SetCategories(importedTransactions, autoCategories);
                }
            }
        }

        private void SetCategories(List<TransactionCsv> importedTransactions, string?[] autoCategories)
        {
            autoCategories = autoCategories
                .Select(ac => ac!.Equals("None") ? null : ac)
                .ToArray();

            importedTransactions = importedTransactions
                .Select((it, i) =>
                {
                    it.AutoCategory = autoCategories[i];
                    return it;
                })
                .ToList();

            foreach (TransactionCsv importedTransaction in importedTransactions)
            {
                importedTransaction.UserCategory = importedTransaction.AutoCategory is null ? 
                    importedTransaction.BankCategory : importedTransaction.AutoCategory;
            }
        }

        private dynamic LoadMainModule()
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
