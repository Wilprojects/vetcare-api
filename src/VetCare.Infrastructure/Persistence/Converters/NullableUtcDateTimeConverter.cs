using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VetCare.Infrastructure.Persistence.Converters;

public sealed class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter()
        : base(
            value => value.HasValue ? value.Value.Kind == DateTimeKind.Utc ? value.Value
                    : value.Value.ToUniversalTime()
                : value,
            value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value)
    {
    }
}
