using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    [Table("UserBudgets")]
    public class UserBudget
    {
        public Guid UserId { get; set; }

        public Guid BudgetId { get; set; }


        public User User { get; set; } = new();

        public Budget Budget { get; set; } = new();
    }
}
