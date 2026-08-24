using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Gamestore.Domain.Shared;

public static class Guard
{
    public static void AgainstNull(
        [NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName, "Value cannot be null.");
        }
    }

    public static void AgainstEmpty(
        EntityId argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument.IsPrimary)
        {
            AgainstEmpty(argument.PrimaryId.Value, paramName);
        }
        else
        {
            AgainstNegative(argument.SecondaryId.Value, paramName);
        }
    }

    public static void AgainstEmpty(
        Guid argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be an empty GUID.", paramName);
        }
    }

    public static void AgainstNegative(
        int argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, argument, "Value cannot be negative.");
        }
    }

    public static void AgainstNullOrWhiteSpace(
        [NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }
    }
}