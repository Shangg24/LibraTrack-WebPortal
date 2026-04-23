using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class issue
{
    [Key]
    public int id { get; set; }

    public string issue_id { get; set; }
    public string full_name { get; set; }
    public string contact { get; set; }
    public string email { get; set; }
    public DateTime issue_date { get; set; }
    public DateTime return_date { get; set; }

    public DateTime? date_insert { get; set; }
    public DateTime? date_update { get; set; }
    public DateTime? date_delete { get; set; }

    public string copy_id { get; set; }
    public string ID_no { get; set; }
    public string grade_section { get; set; }
    public string status { get; set; }
}