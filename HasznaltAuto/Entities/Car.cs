using System;
using System.Collections.Generic;

namespace HasznaltAuto.Entities;

public partial class Car
{
    public int Id { get; set; }

    public string Make { get; set; } = null!;

    public string Model { get; set; } = null!;

    public int Price { get; set; }

    public string Registration { get; set; } = null!;

    public string LicensePlate { get; set; } = null!;

    public int Kilometres { get; set; }

    public int FuelType { get; set; }

    public int CurrentOwner { get; set; }

    public int PreviousOwners { get; set; }

    public bool IsDeleted { get; set; }

    public virtual FuelType FuelTypeNavigation { get; set; } = null!;
}
