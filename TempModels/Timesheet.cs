using System;
using System.Collections.Generic;

namespace SimpleCRM.TempModels;

public partial class Timesheet
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public double HoursWorked { get; set; }

    public string Description { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int IsBillable { get; set; }

    public string Category { get; set; } = null!;

    public int Status { get; set; }

    public int? ApprovedByUserId { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string ApprovalComments { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual User? ApprovedByUser { get; set; }

    public virtual User User { get; set; } = null!;
}
