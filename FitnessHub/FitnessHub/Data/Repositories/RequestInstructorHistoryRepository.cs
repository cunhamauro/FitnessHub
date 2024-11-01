﻿using FitnessHub.Data.Entities.Communication;

namespace FitnessHub.Data.Repositories
{
    public class RequestInstructorHistoryRepository : GenericRepository<RequestInstructorHistory>, IRequestInstructorHistoryRepository
    {
        private readonly DataContext _context;

        public RequestInstructorHistoryRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public List<RequestInstructorHistory> GetAllByGymId(int gymId)
        {
            return _context.RequestsIntructorHistory.Where(requests => requests.GymId == gymId).ToList();
        }
    }
}