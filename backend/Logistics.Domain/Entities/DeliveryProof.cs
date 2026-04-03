using System.ComponentModel.DataAnnotations;

namespace Logistics.Domain.Entities;

public sealed class DeliveryProof
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>
    /// Client-generated id for idempotent sync (offline proof retries).
    /// Not in the original DDL sketch; added by EF migration as NVARCHAR(450) UNIQUE.
    /// </summary>
    [MaxLength(450)]
    public string ClientProofId { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;

    public DateTimeOffset DeliveredAt { get; set; }
}

