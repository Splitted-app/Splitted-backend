using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming.Transaction
{
    public class TransactionPutDTO
    {
        private decimal? amount;
        private string? currency;
        private DateTime? date;
        private string? description;
        private TransactionTypeEnum? transactionType;
        private string? userCategory;
        private readonly HashSet<string> setProperties = new HashSet<string>();


        public HashSet<string> SetProperties
        {
            get => new HashSet<string>(setProperties);
        }

        public decimal? Amount
        {
            get => amount;
            set
            {
                amount = value;
                setProperties.Add(nameof(Amount));
            }
        }

        public string? Currency
        {
            get => currency;
            set
            {
                currency = value;
                setProperties.Add(nameof(Currency));
            }
        }

        public DateTime? Date
        {
            get => date;
            set
            {
                date = value;
                setProperties.Add(nameof(Date));
            }
        }

        public string? Description
        {
            get => description;
            set
            {
                description = value;
                setProperties.Add(nameof(Description));
            }
        }

        public TransactionTypeEnum? TransactionType
        {
            get => transactionType;
            set
            {
                transactionType = value;
                setProperties.Add(nameof(TransactionType));
            }
        }
        public string? UserCategory
        {
            get => userCategory;
            set
            {
                userCategory = value;
                setProperties.Add(nameof(UserCategory));
            }
        }
    }
}
