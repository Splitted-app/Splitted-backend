using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming.Goal
{
    public class GoalPutDTO
    {
        private decimal? amount;
        private DateTime? deadline;
        private bool? isMain;
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

        public DateTime? Deadline
        {
            get => deadline;
            set
            {
                deadline = value;
                setProperties.Add(nameof(Deadline));
            }
        }

        public bool? IsMain
        {
            get => isMain; 
            set
            {
                isMain = value;
                setProperties.Add(nameof(IsMain));
            }
        }
    }
}
