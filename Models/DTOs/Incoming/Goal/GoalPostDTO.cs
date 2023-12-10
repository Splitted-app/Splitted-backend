using Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming.Goal
{
    public class GoalPostDTO
    {
        [Required]
        public decimal? Amount { get; set; }

        public string? Category { get; set; }

        [Required]
        public GoalTypeEnum? GoalType { get; set; }

        [Required]
        public DateTime? Deadline { get; set; }

        public bool IsMain { get; set; }
    }
}
