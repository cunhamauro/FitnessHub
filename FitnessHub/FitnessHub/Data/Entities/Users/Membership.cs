﻿using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Membership : IEntity
    {
        public int Id { get; set; }

        [Range(0, 9)]
        [Required]
        public int Tier {  get; set; } // 0, 1, 2, etc...

        [Required]
        public string? Name { get; set; }

        [Required]
        public decimal Price { get; set; } // Monthly price

        [Required]
        public string? Description {  get; set; }
    }
}
