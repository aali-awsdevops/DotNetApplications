using DotNet_App.Contracts;
using DotNet_App.Data;
using DotNet_App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNet_App.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    RegistrationDbContext db,
    IPasswordHasher<User> passwordHasher,
    ILogger<UsersController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        if (await db.Users.AnyAsync(user => user.Username == username || user.Email == email, cancellationToken))
        {
            return Conflict("Username or email is already registered.");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = string.Empty,
            Email = email,
            Age = request.Age,
            Gender = request.Gender.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Registered user {UserId} with username {Username}.", user.Id, user.Username);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResponse(user));
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType<IReadOnlyCollection<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking()
            .OrderBy(user => user.Id)
            .Select(user => new UserResponse(
                user.Id, user.Username, user.Email, user.Age, user.Gender,
                user.CreatedAtUtc, user.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Listed {UserCount} registered users.", users.Count);
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var username = request.Username?.Trim();
        var email = request.Email?.Trim();
        if ((username is not null && username != user.Username && await db.Users.AnyAsync(item => item.Username == username, cancellationToken)) ||
            (email is not null && email != user.Email && await db.Users.AnyAsync(item => item.Email == email, cancellationToken)))
        {
            return Conflict("Username or email is already registered.");
        }

        if (username is not null) user.Username = username;
        if (email is not null) user.Email = email;
        if (request.Age.HasValue) user.Age = request.Age.Value;
        if (request.Gender is not null) user.Gender = request.Gender.Trim();
        if (request.Password is not null) user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated user {UserId}.", id);
        return Ok(ToResponse(user));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted user {UserId}.", id);
        return NoContent();
    }

    private static UserResponse ToResponse(User user) => new(
        user.Id, user.Username, user.Email, user.Age, user.Gender,
        user.CreatedAtUtc, user.UpdatedAtUtc);
}
