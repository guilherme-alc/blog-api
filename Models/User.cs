﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class User
    {
        public User()
        {
            Posts = new List<Post>();
            Roles = new List<Role>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Bio { get; set; }
        public string? Image { get; set; }
        public string Slug { get; set; }
        public string Github { get; set; }
        public IList<Post> Posts { get; set; }
        public IList<Role> Roles { get; set; }
    }
}
