﻿namespace FitnessHub.Data.Entities.GymMachines
{
    public class MachineDetail
    {
        public int Id { get; set; }

        public Machine Machine { get; set; }

        public Gym Gym { get; set; }

        public bool Status { get; set; }
    }
}
