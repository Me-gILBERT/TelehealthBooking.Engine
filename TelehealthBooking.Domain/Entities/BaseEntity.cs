using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelehealthBooking.Domain.Entities;

// Using an abstract class because we never want to instantiate 'BaseEntity' on its own.
// The <TId> is a Generic. It allows some tables to use Guids, and others to use ints.
public abstract class BaseEntity<TId>
{
    public required TId Id { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    public void MarkAsUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}