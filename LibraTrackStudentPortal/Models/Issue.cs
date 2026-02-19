using System.ComponentModel.DataAnnotations;

public class issue
{
    [Key]
    public int id { get; set; }

    public string issue_id { get; set; }
    public string ID_no { get; set; }
    public DateTime issue_date { get; set; }
    public DateTime return_date { get; set; }
    public string status { get; set; }
}
