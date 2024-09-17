﻿using System.ComponentModel.DataAnnotations;

namespace FitnessHub.Data.Entities.Users
{
    public class Instructor : User
    {
        public Gym Gym { get; set; }

        [Range(1,5)]
        public int Rating {  get; set; }

        [Display(Name = "Reviews")]
        public int NumReviews {  get; set; }
    }
}