using System;
using System.Collections.Generic;

namespace HasznaltAuto.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;
}
