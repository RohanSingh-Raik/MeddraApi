namespace MeddraService.Models;

public class MeddraLevelModel
{
    public string? Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsPrimaryPath { get; set; }
    public int PathId { get; set; }
}
