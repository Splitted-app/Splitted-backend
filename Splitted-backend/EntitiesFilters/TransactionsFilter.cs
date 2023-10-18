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

        private UserManager<User> userManager { get; }


        public TransactionsFilter((DateTime? dateFrom, DateTime? dateTo) dates, (decimal? minAmount, decimal? maxAmount) amounts,
            string? category, string? userName, UserManager<User> userManager)
        {
            this.dates = (dates.dateFrom is null ? DateTime.MinValue : (DateTime)dates.dateFrom,
                dates.dateTo is null ? DateTime.MaxValue : (DateTime)dates.dateTo);
            this.amounts = (amounts.minAmount is null ? decimal.MinValue : (decimal)amounts.minAmount,
                amounts.maxAmount is null ? decimal.MaxValue : (decimal)amounts.maxAmount);
            this.category = category is null ? null : category.ToLower();
            this.userName = userName is null ? null : userName.ToLower();
            this.userManager = userManager;
        }

        
        public async Task<List<Transaction>> GetFilteredTransactions(List<Transaction> transactions)
            => FilterByDates(FilterByAmounts(FilterByCategory(await FilterByUserName(transactions))));

        private List<Transaction> FilterByDates(IEnumerable<Transaction> transactions) 
            => transactions.Where(t => t.Date <= dates.dateTo && t.Date >= dates.dateFrom)
                .OrderBy(t => t.Date)
                .ToList();

        private IEnumerable<Transaction> FilterByAmounts(IEnumerable<Transaction> transactions)
            => transactions.Where(t => t.Amount <= amounts.maxAmount && t.Amount >= amounts.minAmount);

        private IEnumerable<Transaction> FilterByCategory(IEnumerable<Transaction> transactions)
            => transactions.Where(t =>
                {
                    List<string?> categories = new List<string?>
                    { 
                        t.AutoCategory is null ? null : t.AutoCategory.ToLower(), 
                        t.BankCategory is null ? null : t.BankCategory.ToLower(), 
                        t.UserCategory is null ? null : t.UserCategory.ToLower(),
                    };

                    return category is null || categories.Any(c => category.Equals(c));
                })
            .ToList();

        private async Task<IEnumerable<Transaction>> FilterByUserName(IEnumerable<Transaction> transactions)
        {
            List<User> transactionsUsers = await userManager.FindMultipleByIdsWithIncludesAsync(
                                transactions.Select(tf => tf.UserId));

            return transactions.Where(t => userName is null || 
                transactionsUsers.First(u => u.Id.Equals(t.UserId)).UserName.Equals(userName))
                .ToList();
        }
    }
}
