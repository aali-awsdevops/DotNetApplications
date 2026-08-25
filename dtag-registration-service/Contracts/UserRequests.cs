using System.ComponentModel.DataAnnotations;

namespace DotNet_App.Contracts;

public sealed class CreateUserRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Range(1, 130)]
    public int Age { get; init; }

    [Required, StringLength(30)]
    public string Gender { get; init; } = string.Empty;
}

public sealed class UpdateUserRequest
{
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; init; }

    [StringLength(100, MinimumLength = 8)]
    public string? Password { get; init; }

    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [Range(1, 130)]
    public int? Age { get; init; }

    [StringLength(30)]
    public string? Gender { get; init; }
}

public sealed record UserResponse(
    int Id,
    string Username,
    string Email,
    int Age,
    string Gender,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
