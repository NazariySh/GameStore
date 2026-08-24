using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Gamestore.Domain.Shared;

public readonly struct EntityId
{
    public EntityId()
    {
    }

    public EntityId(Guid primaryId)
    {
        PrimaryId = primaryId;
    }

    public EntityId(int secondaryId)
    {
        SecondaryId = secondaryId;
    }

    public Guid? PrimaryId { get; }

    public int? SecondaryId { get; }

    [MemberNotNullWhen(true, nameof(PrimaryId))]
    [MemberNotNullWhen(false, nameof(SecondaryId))]
    public bool IsPrimary => PrimaryId.HasValue;

    [MemberNotNullWhen(true, nameof(SecondaryId))]
    [MemberNotNullWhen(false, nameof(PrimaryId))]
    public bool IsSecondary => SecondaryId.HasValue;

    public static EntityId? ParseOrDefault(string? id)
    {
        return !string.IsNullOrWhiteSpace(id)
            ? Parse(id)
            : null;
    }

    public static EntityId Parse(string id)
    {
        Guard.AgainstNullOrWhiteSpace(id);

        if (Guid.TryParse(id, out var guid))
        {
            return new EntityId(guid);
        }

        if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intId))
        {
            return new EntityId(intId);
        }

        throw new ArgumentException("Invalid ID format. ID must be a valid GUID or integer.", nameof(id));
    }

    public override string ToString()
    {
        return IsPrimary ? PrimaryId.ToString() : SecondaryId.ToString();
    }
}