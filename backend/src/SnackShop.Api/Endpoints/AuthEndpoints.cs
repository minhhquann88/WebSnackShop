using MediatR;
using SnackShop.Api.Contracts;
using SnackShop.Api.Extensions;
using SnackShop.Application.Features.Auth;
using SnackShop.Application.Features.Auth.ConfirmEmail;
using SnackShop.Application.Features.Auth.ForgotPassword;
using SnackShop.Application.Features.Auth.Login;
using SnackShop.Application.Features.Auth.LoginWithGoogle;
using SnackShop.Application.Features.Auth.Logout;
using SnackShop.Application.Features.Auth.RefreshSession;
using SnackShop.Application.Features.Auth.Register;
using SnackShop.Application.Features.Auth.ResetPassword;

namespace SnackShop.Api.Endpoints;

/// <summary>Sign-up, sign-in and session endpoints. All rate limited per IP.</summary>
public sealed class AuthEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder app)
    {
        // Strict per-IP limit only where credentials are guessed (login, register, reset). Session endpoints use a
        // looser limit: refresh runs on every page load and many shoppers share one IP behind carrier NAT.
        var group = app.MapGroup(ApiRoutes.V1 + "/auth")
            .WithTags("Auth")
            .AllowAnonymous();

        group.MapPost("/register", async (RegisterCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToOk())
            .WithName("Register").RequireRateLimiting(RateLimitPolicies.Auth).WithSummary("Create a customer account and sign in.")
            .Produces<AuthResponse>().ProducesErrors(StatusCodes.Status409Conflict, StatusCodes.Status429TooManyRequests);

        group.MapPost("/login", async (LoginCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToOk())
            .WithName("Login").RequireRateLimiting(RateLimitPolicies.Auth).WithSummary("Sign in with email and password.")
            .Produces<AuthResponse>().ProducesErrors(StatusCodes.Status401Unauthorized, StatusCodes.Status429TooManyRequests);

        group.MapPost("/google", async (LoginWithGoogleCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToOk())
            .WithName("LoginWithGoogle").RequireRateLimiting(RateLimitPolicies.Auth).WithSummary("Sign in or sign up with a Google ID token.")
            .Produces<AuthResponse>().ProducesErrors(StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden);

        group.MapPost("/refresh", async (RefreshTokenRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new RefreshSessionCommand(r.RefreshToken), ct)).ToOk())
            .WithName("RefreshSession").RequireRateLimiting(RateLimitPolicies.Session).WithSummary("Exchange a refresh token for a new token pair (rotation).")
            .Produces<AuthResponse>().ProducesErrors(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", async (RefreshTokenRequest r, ISender s, CancellationToken ct) =>
                (await s.Send(new LogoutCommand(r.RefreshToken), ct)).ToNoContent())
            .WithName("Logout").RequireRateLimiting(RateLimitPolicies.Session).WithSummary("Revoke a refresh token.")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors();

        group.MapPost("/forgot-password", async (ForgotPasswordCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToNoContent())
            .WithName("ForgotPassword").RequireRateLimiting(RateLimitPolicies.Auth).WithSummary("Email a password reset link (always 204).")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors();

        group.MapPost("/reset-password", async (ResetPasswordCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToNoContent())
            .WithName("ResetPassword").RequireRateLimiting(RateLimitPolicies.Auth).WithSummary("Set a new password from the emailed token; signs out all sessions.")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors();

        group.MapPost("/confirm-email", async (ConfirmEmailCommand c, ISender s, CancellationToken ct) => (await s.Send(c, ct)).ToNoContent())
            .WithName("ConfirmEmail").RequireRateLimiting(RateLimitPolicies.Session).WithSummary("Confirm the email address from the emailed link.")
            .Produces(StatusCodes.Status204NoContent).ProducesErrors();
    }
}
