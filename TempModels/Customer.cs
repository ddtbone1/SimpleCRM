using System;
using System.Collections.Generic;

namespace SimpleCRM.TempModels;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}
