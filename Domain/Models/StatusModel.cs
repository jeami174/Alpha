namespace Business.Models;

public class StatusModel
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public int ProjectCount { get; set; }
}
