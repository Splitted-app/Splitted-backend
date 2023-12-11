using Microsoft.AspNetCore.Identity;
using Models.Entities;
using Splitted_backend.Extensions;
using Splitted_backend.Models.Entities;

namespace Splitted_backend.EntitiesFilters
{
    public class TransactionsFilter
    {
        private (DateTime dateFrom, DateTime dateTo) dates { get; set; }

        private (decimal minAmount, decimal maxAmount) amounts { get; set; }

        private string? category { get; set; }

        private string? userName { get; set; }


        public TransactionsFilter((DateTime? dateFrom, DateTime? dateTo) dates, (decimal? minAmount, decimal? maxAmount) amounts,
            string? category = null, string? userName = null)
        {
            this.dates = (dates.dateFrom is null ? DateTime.MinValue : (DateTime)dates.dateFrom,
                dates.dateTo is null ? DateTime.MaxValue : (DateTime)dates.dateTo);
            this.amounts = (amounts.minAmount is null ? decimal.MinValue : (decimal)amounts.minAmount,
                amounts.maxAmount is null ? decimal.MaxValue : (decimal)amounts.maxAmount);
            this.category = category is null ? null : category.ToLower();
            this.userName = userName is null ? null : userName.ToLower();
        }

        
        public List<Transaction> GetFilteredTransactions(List<Transaction> transactions)
            => FilterByDates(FilterByAmounts(FilterByCategory(FilterByUserName(transactions))));

        private List<Transaction> FilterByDates(IEnumerable<Transaction> transactions) 
            => transactions.Where(t => t.Date <= dates.dateTo && t.Date >= dates.dateFrom)
                .OrderBy(t => t.Date)
                .ToList();

        private IEnumerable<Transaction> FilterByAmounts(IEnumerable<Transaction> transactions)
            => transactions.Where(t => t.Amount <= amounts.maxAmount && t.Amount >= amounts.minAmount);

        private IEnumerable<Transaction> FilterByCategory(IEnumerable<Transaction> transactions)
            => transactions.Where(t =>
                {
                    string? userCategory = t.UserCategory is null ? null : t.UserCategory.ToLower();
                    return category is null || 
                        (category.Equals("uncategorized") && string.IsNullOrWhiteSpace(userCategory)) ||
                        (userCategory is not null && userCategory.Contains(category));
                })
                .ToList();

        private IEnumerable<Transaction> FilterByUserName(IEnumerable<Transaction> transactions)
            => transactions.Where(t => userName is null || t.User.UserName.Equals(userName))
                .ToList();
    }
}
