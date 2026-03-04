using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelehealthBooking.Domain.Entities;

public sealed class Patient : BaseEntity<Guid>
{
    public string Name { get; private set; }
    public string Email { get; private set; }

    private Patient() { Name = string.Empty; Email = string.Empty; }

    public static Patient Create(string name, string email)
    {
        return new Patient
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email
        };
    }
}