using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Goal
{
    public class GoalCreatedDTO
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public string Name { get; set; }

        public string? Category { get; set; }

        public GoalTypeEnum GoalType { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime Deadline { get; set; }

        public bool IsMain { get; set; }
    }
}
