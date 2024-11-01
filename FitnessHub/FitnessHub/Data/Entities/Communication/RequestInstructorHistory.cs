﻿namespace FitnessHub.Data.Entities.Communication
{
    public class RequestInstructorHistory : IEntity
    {
        public int Id { get; set; }

        public string? ClientId { get; set; }

        public int GymId { get; set; }

        public string? Notes { get; set; }

        public DateTime? RequestDate { get; set; }
    }
}
