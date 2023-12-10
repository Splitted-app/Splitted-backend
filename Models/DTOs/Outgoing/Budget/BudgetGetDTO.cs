using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.DTOs.Outgoing.User;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Budget
{
    public class BudgetGetDTO : UserBudgetGetDTO
    {
        public List<UserGetDTO> Users { get; set; }
    }
}
