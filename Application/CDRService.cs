using Domain.Entities;
using Infrastructure;

namespace Application
{
    public class CDRService : ICDRService
    {
        private readonly ICDRRepository _cdrRepository;

        public CDRService(ICDRRepository cdrRepository)
        {
            _cdrRepository = cdrRepository ?? throw new ArgumentNullException(nameof(cdrRepository));
        }

        public async Task<CDR> GetByReferenceAsync(string reference)
        {
            return await _cdrRepository.GetByReferenceAsync(reference);
        }

        public async Task<Dictionary<string, object>> GetCallCountAndTotalDurationAsync(DateTime startDate, DateTime endDate)
        {
            var filteredCdrs = await _cdrRepository.GetCdrsByDateRangeAsync(startDate, endDate);

            var count = filteredCdrs.Count();
            var totalDuration = filteredCdrs.Sum(a => a.Duration);

            return new Dictionary<string, object>
            {
                { "Count", count },
                { "TotalDuration", totalDuration }
            };
        }

        public async Task<IEnumerable<CDR>> GetCdrsByCallerIdAsync(string callerId, DateTime startDate, DateTime endDate)
        {
            return await _cdrRepository.GetCdrsByCallerIdAsync(callerId, startDate, endDate);
        }

        public async Task<IEnumerable<CDR>> GetMostExpensiveCallsAsync(string callerId, DateTime startDate, DateTime endDate, int count)
        {
            var filteredCdrs = await _cdrRepository.GetCdrsByCallerIdAsync(callerId, startDate, endDate);
            return filteredCdrs.OrderByDescending(a => a.Cost).Take(count);
        }
    }
}