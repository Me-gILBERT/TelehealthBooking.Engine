using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelehealthBooking.Domain.Entities;

public sealed class Doctor : BaseEntity<Guid>
{
    public string Name { get; private set; }
    public string Specialization { get; private set; }

    private Doctor() { Name = string.Empty; Specialization = string.Empty; }

    public static Doctor Create(string name, string specialization)
    {
        return new Doctor
        {
            Id = Guid.NewGuid(),
            Name = name,
            Specialization = specialization
        };
    }
}