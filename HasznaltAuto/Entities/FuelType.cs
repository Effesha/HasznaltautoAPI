using System;
using System.Collections.Generic;

namespace HasznaltAuto.Entities;

public partial class FuelType
{
    public int Id { get; set; }

    public int Name { get; set; }

    public virtual ICollection<Car> Cars { get; } = new List<Car>();
}
