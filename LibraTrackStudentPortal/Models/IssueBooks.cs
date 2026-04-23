using System;
using System.ComponentModel.DataAnnotations;

public class issue_books
{
    [Key]
    public int id { get; set; }

    public string issue_id { get; set; }
    public string book_id { get; set; }
    public string status { get; set; }
    public DateTime? date_insert { get; set; }
}

