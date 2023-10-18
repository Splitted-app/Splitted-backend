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
        private string? avatarImage;
        private string? username;
        private readonly HashSet<string> setProperties = new HashSet<string>();


        public HashSet<string> SetProperties
        {
            get => new HashSet<string>(setProperties);
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

        public string? UserName
        {
            get => username;
            set
            {
                username = value;
                setProperties.Add(nameof(UserName));
            }
        }
    }
}
