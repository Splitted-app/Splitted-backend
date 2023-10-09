using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs.Incoming.User
{
    public class UserPutDTO
    {
        private decimal? bankBalance;
        private string? bank;
        private string? avatarImage;
        private readonly HashSet<string> setProperties = new HashSet<string>();


        public HashSet<string> SetProperties
        {
            get => new HashSet<string>(setProperties);
        }

        public decimal? BankBalance
        {
            get => bankBalance;
            set
            {
                bankBalance = value;
                setProperties.Add(nameof(BankBalance));
            }
        }

        public string? Bank
        {
            get => bank;
            set
            {
                bank = value;
                setProperties.Add(nameof(Bank));
            }
        }

        public string? AvatarImage
        {
            get => avatarImage;
            set
            {
                avatarImage = value;
                setProperties.Add(nameof(AvatarImage));
            }
        }
    }
}
