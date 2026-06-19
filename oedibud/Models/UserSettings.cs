namespace oedibud.Models;

public class UserSettings
{
    public int Id { get; set; } = 1;
    public DateTime StartMonth { get; set; } = new(DateTime.Today.Year, 1, 1);
    public bool QuarterView { get; set; } = false;
    /// <summary>Comma-separated list of expanded employee root IDs</summary>
    public string ExpandedEmployeeIds { get; set; } = "";
    /// <summary>Comma-separated list of expanded project root IDs</summary>
    public string ExpandedProjectIds { get; set; } = "";
}
