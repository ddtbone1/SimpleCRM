using System;
using System.Collections.Generic;

namespace SimpleCRM.TempModels;

public partial class Agent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string Position { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public int IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
}
