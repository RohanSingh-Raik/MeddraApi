using MeddraService.Models;

namespace MeddraService;

public class MeddraApi
{
    public List<MeddraRecord> _meddraRecords;

    public MeddraApi(string filePath)
    {
        _meddraRecords = LoadMeddraFile(filePath);
    }

    public class MeddraRecord
    {
        public long PtCode { get; set; }
        public long HltCode { get; set; }
        public long HlgtCode { get; set; }
        public long SocCode { get; set; }
        public string? PtName { get; set; }
        public string? HltName { get; set; }
        public string? HlgtName { get; set; }
        public string? SocName { get; set; }
        public string? SocAbbrev { get; set; }
        public string? NullField { get; set; }
        public long? PtSocCode { get; set; }
        public bool PrimarySocFlag { get; set; }
    }

    public List<MeddraRecord> LoadMeddraFile(string filePath)
    {
        var records = new List<MeddraRecord>();

        foreach (string line in File.ReadLines(filePath))
        {
            var fields = line.Split('$');
            if (fields.Length < 12)
            {
                continue;                  // Skip invalid lines
            }  

            var record = new MeddraRecord
            {
                PtCode = long.Parse(fields[0]),
                HltCode = long.Parse(fields[1]),
                HlgtCode = long.Parse(fields[2]),
                SocCode = long.Parse(fields[3]),
                PtName = fields[4],
                HltName = fields[5],
                HlgtName = fields[6],
                SocName = fields[7],
                SocAbbrev = fields[8],
                NullField = fields[9],
                PtSocCode = string.IsNullOrEmpty(fields[10]) ? null : long.Parse(fields[10]),
                PrimarySocFlag = fields[11] == "Y"
            };
            records.Add(record);
        }

        return records;
    }

    public MeddraItems GetHierarchyByTerm(string termName, string termType)
    {
        var result = new MeddraItems
        {
            SocValues = [],
            HlgtValues = [],
            HltValues = [],
            PtValues = [],
            LltValues = [] // Note: LLT data not present in current schema
        };

        var matchingRecords = termType.ToUpper() switch
        {
            "PT" => _meddraRecords.Where(r => r.PtName!.Equals(termName, StringComparison.OrdinalIgnoreCase)),
            "HLT" => _meddraRecords.Where(r => r.HltName!.Equals(termName, StringComparison.OrdinalIgnoreCase)),
            "HLGT" => _meddraRecords.Where(r => r.HlgtName!.Equals(termName, StringComparison.OrdinalIgnoreCase)),
            "SOC" => _meddraRecords.Where(r => r.SocName!.Equals(termName, StringComparison.OrdinalIgnoreCase)),
            _ => throw new ArgumentException("Invalid term type. Must be PT, HLT, HLGT, or SOC.")
        };

        int pathId = 1;
        foreach (var record in matchingRecords)
        {
            // Add SOC level
            if (!result.SocValues.Any(s => s.Code == record.SocCode.ToString()))
            {
                result.SocValues.Add(new MeddraLevelModel
                {
                    Name = record.SocName,
                    Code = record.SocCode.ToString(),
                    IsPrimaryPath = record.PrimarySocFlag,
                    PathId = pathId
                });
            }

            // Add HLGT level
            if (!result.HlgtValues.Any(h => h.Code == record.HlgtCode.ToString()))
            {
                result.HlgtValues.Add(new MeddraLevelModel
                {
                    Name = record.HlgtName,
                    Code = record.HlgtCode.ToString(),
                    IsPrimaryPath = record.PrimarySocFlag,
                    PathId = pathId
                });
            }

            // Add HLT level
            if (!result.HltValues.Any(h => h.Code == record.HltCode.ToString()))
            {
                result.HltValues.Add(new MeddraLevelModel
                {
                    Name = record.HltName,
                    Code = record.HltCode.ToString(),
                    IsPrimaryPath = record.PrimarySocFlag,
                    PathId = pathId
                });
            }

            // Add PT level
            if (!result.PtValues.Any(p => p.Code == record.PtCode.ToString()))
            {
                result.PtValues.Add(new MeddraLevelModel
                {
                    Name = record.PtName,
                    Code = record.PtCode.ToString(),
                    IsPrimaryPath = record.PrimarySocFlag,
                    PathId = pathId
                });
            }

            pathId++;
        }

        return result;
    }
}
