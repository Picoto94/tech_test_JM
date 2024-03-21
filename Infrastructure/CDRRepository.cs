using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Infrastructure
{
    public class CDRRepository : ICDRRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<CDR> _cdrs;

        public CDRRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _cdrs = ReadCDRsFromCsv();
        }

        public IEnumerable<CDR> GetAllCDRs()
        {
            return _cdrs;
        }

        public async Task<CDR> GetByReferenceAsync(string reference)
        {
            // Implemente a lógica para obter um CDR por referência
            return await Task.FromResult(_cdrs.FirstOrDefault(a => a.Reference == reference));
        }

        public async Task<IEnumerable<CDR>> GetCdrsByCallerIdAsync(string callerId, DateTime startDate, DateTime endDate)
        {
            // Implemente a lógica para obter CDRs por ID do chamador e intervalo de datas
            var filteredCdrs = FilterCdrsByDateRange(startDate, endDate)
                                .Where(a => a.CallerId == callerId);
            return await Task.FromResult(filteredCdrs);
        }

        private IEnumerable<CDR> ReadCDRsFromCsv()
        {
            var csvFilePath = _configuration["CsvFilePath"];
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), csvFilePath);

            var cdrs = new List<CDR>();

            using (var reader = new StreamReader(fullPath))
            {
                // Ignore the header line
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var callerId = values[0];
                    var recipient = values[1];
                    var callDateStr = values[2].Trim(); // Remove espaços em branco extras
                    var endTimeStr = values[3].Trim(); // Remove espaços em branco extras
                    var durationStr = values[4].Trim(); // Remove espaços em branco extras
                    var costStr = values[5].Trim(); // Remove espaços em branco extras
                    var reference = values[6];
                    var currency = values[7];
                    var typeStr = values[8].Trim(); // Remove espaços em branco extras

                    DateTime callDate;
                    DateTime endTime;
                    double duration;
                    decimal cost;
                    CallType type;

                    // Tenta converter as strings de data para DateTime
                    if (!DateTime.TryParseExact(callDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out callDate))
                    {
                        throw new FormatException($"Failed to parse call date: {callDateStr}");
                    }

                    // Ajuste o formato da string de hora para incluir horas, minutos e segundos
                    if (!DateTime.TryParseExact(endTimeStr, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
                    {
                        throw new FormatException($"Failed to parse end time: {endTimeStr}");
                    }

                    // Tenta converter a string de duração para um número
                    if (!double.TryParse(durationStr, out duration))
                    {
                        // Se não for possível converter, defina a duração como zero (ou outro valor padrão)
                        duration = 0.0;
                        // Ou então, você pode lançar uma exceção aqui se preferir
                        // throw new FormatException($"Failed to parse duration: {durationStr}");
                    }

                    // Tenta converter a string de custo para um número
                    if (!decimal.TryParse(costStr, out cost))
                    {
                        throw new FormatException($"Failed to parse cost: {costStr}");
                    }

                    // Tenta converter a string de tipo para o enum CallType
                    if (!Enum.TryParse(typeStr, out type))
                    {
                        throw new FormatException($"Failed to parse call type: {typeStr}");
                    }

                    var cdr = new CDR
                    {
                        CallerId = callerId,
                        Recipient = recipient,
                        CallDate = callDate,
                        EndTime = endTime,
                        Duration = duration,
                        Cost = cost,
                        Reference = reference,
                        Currency = currency,
                        Type = type
                    };

                    cdrs.Add(cdr);
                }
            }

            return cdrs;
        }
        public async Task<IEnumerable<CDR>> GetCdrsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var filteredCdrs = await Task.FromResult(FilterCdrsByDateRange(startDate, endDate));

            return filteredCdrs;
        }
        private IEnumerable<CDR> FilterCdrsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _cdrs.Where(a => a.CallDate >= startDate && a.CallDate <= endDate);
        }
    }
}
