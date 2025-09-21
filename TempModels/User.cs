using System;
using System.Collections.Generic;

namespace SimpleCRM.TempModels;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Timesheet> TimesheetApprovedByUsers { get; set; } = new List<Timesheet>();

    public virtual ICollection<Timesheet> TimesheetUsers { get; set; } = new List<Timesheet>();
}
