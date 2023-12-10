using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Outgoing.Goal
{
    public class GoalGetDTO : GoalCreatedDTO
    {
        public double Percentage { get; set; }
    }
}
