using Models.Enums;
using Models.Interfaces;
using Splitted_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entities
{
    [Table("Goals")]
    public class Goal : IEntity, ICloneable
    {
        public Guid Id { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        public string Name { get; set; }

        public string? Category { get; set; }

        public GoalTypeEnum GoalType { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime Deadline { get; set; }

        public bool IsMain { get; set; }


        public Guid UserId { get; set; }

        public User User { get; set; } = null!;


        public object Clone()
        {
            return new Goal
            {
                Id = Id,
                Amount = Amount,
                Name = Name,
                Category = Category,
                GoalType = GoalType,
                CreationDate = CreationDate,
                Deadline = Deadline,
                IsMain = IsMain,
                UserId = UserId,
            };
        }
    }
}
