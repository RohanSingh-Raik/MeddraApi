using MeddraService.Models;

namespace MeddraService;

public class MeddraApi
{
    public List<MeddraRecord> _meddraRecords;
    public List<LltRecord> _lltRecords;

    public MeddraApi(string filePath, string lltFilePath)
    {
        _meddraRecords = LoadMeddraFile(filePath);
        _lltRecords = LoadLltFile(lltFilePath);
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

    public class LltRecord
    {
        public long LltCode { get; set; }
        public string? LltName { get; set; }
        public long PtCode { get; set; }
        public bool IsCurrent { get; set; }
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

    private List<LltRecord> LoadLltFile(string filePath)
    {
        var records = new List<LltRecord>();

        foreach (string line in File.ReadLines(filePath))
        {
            var fields = line.Split('$');
            if (fields.Length < 11) continue;

            // Remove quotes if present in LLT name
            string lltName = fields[1].Trim('"');

            var record = new LltRecord
            {
                LltCode = long.Parse(fields[0]),
                LltName = lltName,
                PtCode = long.Parse(fields[2]),
                IsCurrent = fields[9] == "Y"
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
            LltValues = []
        };

        // Handle LLT search separately
        if (termType.ToUpper() == "LLT")
        {
            // Find the LLT record
            var lltRecord = _lltRecords
                .FirstOrDefault(l => l.LltName != null &&
                    l.LltName.Equals(termName, StringComparison.OrdinalIgnoreCase) &&
                    l.IsCurrent);

            if (lltRecord == null)
            {
                return result;
            }

            // Find all MedDRA records with the same PT code
            var ptRecords = _meddraRecords
                .Where(r => r.PtCode == lltRecord.PtCode);

            int pathId1 = 1;
            foreach (var ptRecord in ptRecords)
            {
                // Add LLT Values
                if (!result.LltValues.Any(l => l.Code == lltRecord.LltCode.ToString()))
                {
                    result.LltValues.Add(new MeddraLevelModel
                    {
                        Name = lltRecord.LltName,
                        Code = lltRecord.LltCode.ToString(),
                        IsPrimaryPath = false,
                        PathId = pathId1
                    });
                }

                // Add PT Values
                if (!result.PtValues.Any(p => p.Code == ptRecord.PtCode.ToString()))
                {
                    result.PtValues.Add(new MeddraLevelModel
                    {
                        Name = ptRecord.PtName,
                        Code = ptRecord.PtCode.ToString(),
                        IsPrimaryPath = ptRecord.PrimarySocFlag,
                        PathId = pathId1
                    });
                }

                // Add HLT Values
                if (!result.HltValues.Any(h => h.Code == ptRecord.HltCode.ToString()))
                {
                    result.HltValues.Add(new MeddraLevelModel
                    {
                        Name = ptRecord.HltName,
                        Code = ptRecord.HltCode.ToString(),
                        IsPrimaryPath = ptRecord.PrimarySocFlag,
                        PathId = pathId1
                    });
                }

                // Add HLGT Values
                if (!result.HlgtValues.Any(h => h.Code == ptRecord.HlgtCode.ToString()))
                {
                    result.HlgtValues.Add(new MeddraLevelModel
                    {
                        Name = ptRecord.HlgtName,
                        Code = ptRecord.HlgtCode.ToString(),
                        IsPrimaryPath = ptRecord.PrimarySocFlag,
                        PathId = pathId1
                    });
                }

                // Add SOC Values
                if (!result.SocValues.Any(s => s.Code == ptRecord.SocCode.ToString()))
                {
                    result.SocValues.Add(new MeddraLevelModel
                    {
                        Name = ptRecord.SocName,
                        Code = ptRecord.SocCode.ToString(),
                        IsPrimaryPath = ptRecord.PrimarySocFlag,
                        PathId = pathId1
                    });
                }

                pathId1++;
            }

            return result;
        }

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

            // Add LLT level only when term type is PT
            if (termType.ToUpper() == "PT")
            {
                var lltRecordsForPt = _lltRecords
                    .Where(l => l.PtCode == record.PtCode && l.IsCurrent)
                    .DistinctBy(l => l.LltCode);

                foreach (var lltRecord in lltRecordsForPt)
                {
                    if (!result.LltValues.Any(l => l.Code == lltRecord.LltCode.ToString()))
                    {
                        result.LltValues.Add(new MeddraLevelModel
                        {
                            Name = lltRecord.LltName,
                            Code = lltRecord.LltCode.ToString(),
                            IsPrimaryPath = false,
                            PathId = pathId
                        });
                    }
                }
            }

            pathId++;
        }

        return result;
    }

    public MeddraItems SearchTerm(string searchText, string termLevel)
    {
        var result = new MeddraItems
        {
            SocValues = [],
            HlgtValues = [],
            HltValues = [],
            PtValues = [],
            LltValues = []
        };

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return result;
        }

        // Normalize search text to lowercase for case-insensitive partial matching
        string normalizedSearchText = searchText.ToLower();

        int pathId = 1;
        switch (termLevel.ToUpper())
        {
            case "SOC":
                var socRecords = _meddraRecords
                    .Where(r => r.SocName != null && r.SocName.ToLower().StartsWith(normalizedSearchText))
                    .DistinctBy(r => r.SocCode);

                foreach (var record in socRecords)
                {
                    result.SocValues.Add(new MeddraLevelModel
                    {
                        Name = record.SocName,
                        Code = record.SocCode.ToString(),
                        IsPrimaryPath = record.PrimarySocFlag,
                        PathId = pathId++
                    });
                }
                break;

            case "HLGT":
                var hlgtRecords = _meddraRecords
                    .Where(r => r.HlgtName != null && r.HlgtName.ToLower().StartsWith(normalizedSearchText))
                    .DistinctBy(r => r.HlgtCode);

                foreach (var record in hlgtRecords)
                {
                    result.HlgtValues.Add(new MeddraLevelModel
                    {
                        Name = record.HlgtName,
                        Code = record.HlgtCode.ToString(),
                        IsPrimaryPath = record.PrimarySocFlag,
                        PathId = pathId++
                    });
                }
                break;

            case "HLT":
                var hltRecords = _meddraRecords
                    .Where(r => r.HltName != null && r.HltName.ToLower().StartsWith(normalizedSearchText))
                    .DistinctBy(r => r.HltCode);

                foreach (var record in hltRecords)
                {
                    result.HltValues.Add(new MeddraLevelModel
                    {
                        Name = record.HltName,
                        Code = record.HltCode.ToString(),
                        IsPrimaryPath = record.PrimarySocFlag,
                        PathId = pathId++
                    });
                }
                break;

            case "PT":
                var ptRecords = _meddraRecords
                    .Where(r => r.PtName != null && r.PtName.ToLower().StartsWith(normalizedSearchText))
                    .DistinctBy(r => r.PtCode);

                foreach (var record in ptRecords)
                {
                    result.PtValues.Add(new MeddraLevelModel
                    {
                        Name = record.PtName,
                        Code = record.PtCode.ToString(),
                        IsPrimaryPath = record.PrimarySocFlag,
                        PathId = pathId++
                    });
                }
                break;

            case "LLT":
                var lltRecords = _lltRecords
                    .Where(r => r.LltName != null && r.LltName.ToLower().StartsWith(normalizedSearchText))
                    .DistinctBy(r => r.LltCode);

                foreach (var record in lltRecords)
                {
                    result.LltValues.Add(new MeddraLevelModel
                    {
                        Name = record.LltName,
                        Code = record.LltCode.ToString(),
                        IsPrimaryPath = false,
                        PathId = pathId++
                    });
                }
                break;

            default:
                throw new ArgumentException("Invalid term level. Must be SOC, HLGT, HLT, PT, or LLT.");
        }

        return result;
    }

}
