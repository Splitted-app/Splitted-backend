﻿using Splitted_backend.Models.Entities;

namespace Splitted_backend.Managers
{
    public static class SearchManager
    {
        public static List<User> SearchUsers(IQueryable<User> users, string query, Guid userId)
            => users.Where(u => (u.Email.ToLower().Contains(query.ToLower()) || 
                    u.UserName.ToLower().Contains(query.ToLower())) && 
                    !u.Id.Equals(userId))
                .ToList();
    }
}
