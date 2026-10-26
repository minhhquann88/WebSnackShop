using System.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SnackShop.Api.IntegrationTests.Infrastructure;
using SnackShop.Application.Features.Auth;

namespace SnackShop.Api.IntegrationTests.Auth;

[Collection(ApiCollection.Name)]
public sealed class AuthEndpointsTests(ApiFactory factory) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Register_CreatesCustomer_SignsIn_AndSendsConfirmationEmail()
    {
        var (client, auth) = await factory.RegisterCustomerAsync("New.Customer@Example.com");

        auth.User.Email.ShouldBe("new.customer@example.com");
        auth.User.Roles.ShouldBe(["Customer"]);
        auth.User.EmailConfirmed.ShouldBeFalse();

        var me = await (await client.GetAsync("/api/v1/me", TestApi.Ct).ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<UserResponse>();
        me.Id.ShouldBe(auth.User.Id);

        var customerExists = await factory.WithDbAsync(db => db.CustomerSet.AnyAsync(c => c.Id == auth.User.Id, TestApi.Ct));
        customerExists.ShouldBeTrue();
        factory.Emails.Sent.ShouldContain(e => e.To == "new.customer@example.com" && e.HtmlBody.Contains("xac-nhan-email"));
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        await factory.RegisterCustomerAsync("dup@example.com");

        var response = await factory.CreateClient().PostJsonAsync("/api/v1/auth/register",
            new { email = "DUP@example.com", password = "Matkhau123", fullName = "X" }).ShouldBeAsync(HttpStatusCode.Conflict);

        (await response.ProblemAsync()).Title.ShouldBe("Auth.EmailTaken");
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400OnPasswordField()
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/v1/auth/register",
            new { email = "weak@example.com", password = "short", fullName = "X" }).ShouldBeAsync(HttpStatusCode.BadRequest);

        var problem = await response.ReadAsync<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>();
        problem.Errors.Keys.ShouldContain("password");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401_AndLocksAfterFiveAttempts()
    {
        await factory.RegisterCustomerAsync("lock@example.com");
        var client = factory.CreateClient();

        for (var i = 0; i < 4; i++)
        {
            var wrong = await client.PostJsonAsync("/api/v1/auth/login", new { email = "lock@example.com", password = "Saimatkhau1" })
                .ShouldBeAsync(HttpStatusCode.Unauthorized);
            (await wrong.ProblemAsync()).Title.ShouldBe("Auth.InvalidCredentials");
        }

        var fifth = await client.PostJsonAsync("/api/v1/auth/login", new { email = "lock@example.com", password = "Saimatkhau1" })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);
        (await fifth.ProblemAsync()).Title.ShouldBe("Auth.LockedOut");

        var correct = await client.PostJsonAsync("/api/v1/auth/login", new { email = "lock@example.com", password = "Matkhau123" })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);
        (await correct.ProblemAsync()).Title.ShouldBe("Auth.LockedOut");
    }

    [Fact]
    public async Task Login_UnknownEmail_LooksLikeWrongPassword()
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/v1/auth/login", new { email = "nobody@example.com", password = "Matkhau123" })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);

        (await response.ProblemAsync()).Title.ShouldBe("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Refresh_RotatesToken_OldTokenStopsWorking()
    {
        var (_, auth) = await factory.RegisterCustomerAsync();
        var client = factory.CreateClient();

        var rotated = await (await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken })
            .ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<AuthResponse>();

        rotated.RefreshToken.ShouldNotBe(auth.RefreshToken);
        var reused = await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);
        (await reused.ProblemAsync()).Title.ShouldBe("Auth.InvalidRefreshToken"); // within the race grace period

        await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = rotated.RefreshToken }).ShouldBeAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_ReusingRotatedTokenAfterGracePeriod_RevokesAllSessions()
    {
        var (_, auth) = await factory.RegisterCustomerAsync();
        var client = factory.CreateClient();
        var rotated = await (await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken })
            .ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<AuthResponse>();

        // Simulate the stolen old token being replayed a minute later.
        await factory.WithDbAsync(db => db.RefreshTokens.Where(t => t.RevokedReason == "Rotated")
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, t => t.RevokedAt!.Value.AddMinutes(-1)), TestApi.Ct));

        var replay = await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);
        (await replay.ProblemAsync()).Title.ShouldBe("Auth.RefreshTokenReused");

        // The legitimate (newest) session is closed too.
        await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = rotated.RefreshToken }).ShouldBeAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var (_, auth) = await factory.RegisterCustomerAsync();
        var client = factory.CreateClient();

        await client.PostJsonAsync("/api/v1/auth/logout", new { refreshToken = auth.RefreshToken }).ShouldBeAsync(HttpStatusCode.NoContent);

        await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken }).ShouldBeAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForgotAndResetPassword_ViaEmailedToken()
    {
        var (_, auth) = await factory.RegisterCustomerAsync("forgot@example.com");
        var client = factory.CreateClient();
        factory.Emails.Clear();

        await client.PostJsonAsync("/api/v1/auth/forgot-password", new { email = "forgot@example.com" }).ShouldBeAsync(HttpStatusCode.NoContent);
        await client.PostJsonAsync("/api/v1/auth/forgot-password", new { email = "unknown@example.com" }).ShouldBeAsync(HttpStatusCode.NoContent);

        var email = factory.Emails.Sent.ShouldHaveSingleItem();
        var link = System.Text.RegularExpressions.Regex.Match(email.HtmlBody, "token=([^\"&]+)").Groups[1].Value;
        var token = Uri.UnescapeDataString(System.Net.WebUtility.HtmlDecode(link));

        await client.PostJsonAsync("/api/v1/auth/reset-password", new { email = "forgot@example.com", token = "bad", newPassword = "Matkhaumoi1" })
            .ShouldBeAsync(HttpStatusCode.BadRequest);
        await client.PostJsonAsync("/api/v1/auth/reset-password", new { email = "forgot@example.com", token, newPassword = "Matkhaumoi1" })
            .ShouldBeAsync(HttpStatusCode.NoContent);

        await client.PostJsonAsync("/api/v1/auth/login", new { email = "forgot@example.com", password = "Matkhaumoi1" }).ShouldBeAsync(HttpStatusCode.OK);
        await client.PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken }).ShouldBeAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Google_NewUser_IsCreatedOnce_AndCanSignInAgain()
    {
        var client = factory.CreateClient();
        var token = FakeGoogleTokenVerifier.Token("g-123", "googler@gmail.com");

        var first = await (await client.PostJsonAsync("/api/v1/auth/google", new { idToken = token }).ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<AuthResponse>();
        var second = await (await client.PostJsonAsync("/api/v1/auth/google", new { idToken = token }).ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<AuthResponse>();

        first.User.Id.ShouldBe(second.User.Id);
        first.User.EmailConfirmed.ShouldBeTrue();
        (await factory.WithDbAsync(db => db.CustomerSet.CountAsync(c => c.Email == "googler@gmail.com", TestApi.Ct))).ShouldBe(1);
    }

    [Fact]
    public async Task Google_DoesNotLinkUnconfirmedAccount_ButLinksAfterEmailConfirmation()
    {
        var (_, auth) = await factory.RegisterCustomerAsync("both@example.com");
        var client = factory.CreateClient();
        var google = new { idToken = FakeGoogleTokenVerifier.Token("g-both", "both@example.com") };

        // Someone may have registered this address without owning it (account pre-hijacking).
        var refused = await client.PostJsonAsync("/api/v1/auth/google", google).ShouldBeAsync(HttpStatusCode.Conflict);
        (await refused.ProblemAsync()).Title.ShouldBe("Auth.UnconfirmedAccountExists");

        var email = factory.Emails.Sent.Single(e => e.To == "both@example.com" && e.HtmlBody.Contains("xac-nhan-email"));
        var token = Uri.UnescapeDataString(System.Net.WebUtility.HtmlDecode(
            System.Text.RegularExpressions.Regex.Match(email.HtmlBody, "token=([^\"&]+)").Groups[1].Value));
        await client.PostJsonAsync("/api/v1/auth/confirm-email", new { userId = auth.User.Id, token }).ShouldBeAsync(HttpStatusCode.NoContent);

        var linked = await (await client.PostJsonAsync("/api/v1/auth/google", google).ShouldBeAsync(HttpStatusCode.OK)).ReadAsync<AuthResponse>();
        linked.User.Id.ShouldBe(auth.User.Id);
    }

    [Fact]
    public async Task ResetPassword_DoesNotLiftAdminLock()
    {
        var admin = await factory.LoginAdminAsync();
        var (_, auth) = await factory.RegisterCustomerAsync("locked-reset@example.com");
        await admin.PostJsonAsync($"/api/v1/admin/users/{auth.User.Id}/lock", new { locked = true }).ShouldBeAsync(HttpStatusCode.NoContent);
        factory.Emails.Clear();
        var client = factory.CreateClient();

        await client.PostJsonAsync("/api/v1/auth/forgot-password", new { email = "locked-reset@example.com" }).ShouldBeAsync(HttpStatusCode.NoContent);
        var body = factory.Emails.Sent.ShouldHaveSingleItem().HtmlBody;
        var token = Uri.UnescapeDataString(System.Net.WebUtility.HtmlDecode(
            System.Text.RegularExpressions.Regex.Match(body, "token=([^\"&]+)").Groups[1].Value));
        await client.PostJsonAsync("/api/v1/auth/reset-password", new { email = "locked-reset@example.com", token, newPassword = "Matkhaumoi1" })
            .ShouldBeAsync(HttpStatusCode.NoContent);

        var login = await client.PostJsonAsync("/api/v1/auth/login", new { email = "locked-reset@example.com", password = "Matkhaumoi1" })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);
        (await login.ProblemAsync()).Title.ShouldBe("Auth.LockedOut");
    }

    [Fact]
    public async Task ConcurrentRefresh_WithSameToken_OnlyOneSucceeds()
    {
        var (_, auth) = await factory.RegisterCustomerAsync();

        var attempts = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ =>
            factory.CreateClient().PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken })));

        attempts.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);
    }

    [Fact]
    public async Task Google_UnverifiedEmail_OrStaffAccount_IsRejected()
    {
        var client = factory.CreateClient();

        var unverified = await client.PostJsonAsync("/api/v1/auth/google",
            new { idToken = FakeGoogleTokenVerifier.Token("g-x", "x@gmail.com", verified: false) }).ShouldBeAsync(HttpStatusCode.Unauthorized);
        (await unverified.ProblemAsync()).Title.ShouldBe("Auth.GoogleEmailNotVerified");

        var staff = await client.PostJsonAsync("/api/v1/auth/google",
            new { idToken = FakeGoogleTokenVerifier.Token("g-admin", ApiFactory.AdminEmail) }).ShouldBeAsync(HttpStatusCode.Forbidden);
        (await staff.ProblemAsync()).Title.ShouldBe("Auth.StaffMustUsePassword");
    }

    [Fact]
    public async Task ChangePassword_RequiresCurrent_AndSignsOutEverywhere()
    {
        var (client, auth) = await factory.RegisterCustomerAsync();

        await client.PutJsonAsync("/api/v1/me/password", new { currentPassword = "Saimatkhau1", newPassword = "Matkhaumoi1" })
            .ShouldBeAsync(HttpStatusCode.BadRequest);
        await client.PutJsonAsync("/api/v1/me/password", new { currentPassword = "Matkhau123", newPassword = "Matkhaumoi1" })
            .ShouldBeAsync(HttpStatusCode.NoContent);

        await factory.CreateClient().PostJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken })
            .ShouldBeAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401WithStableCode()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/me", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Unauthorized);

        (await response.ProblemAsync()).Title.ShouldBe("Auth.NotAuthenticated");
    }

    [Fact]
    public async Task InvalidBearer_OnAnonymousEndpoint_Returns401InsteadOfActingAsGuest()
    {
        var client = factory.CreateClient().WithToken("not-a-valid-jwt");

        var response = await client.GetAsync("/api/v1/categories", TestApi.Ct).ShouldBeAsync(HttpStatusCode.Unauthorized);

        (await response.ProblemAsync()).Title.ShouldBe("Auth.InvalidToken");
        response.Headers.WwwAuthenticate.ToString().ShouldContain("invalid_token");
    }

    [Fact]
    public async Task UnknownRoute_Returns404WithStableCode()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/khong-ton-tai", TestApi.Ct).ShouldBeAsync(HttpStatusCode.NotFound);

        (await response.ProblemAsync()).Title.ShouldBe("Http.NotFound");
    }

    [Fact]
    public async Task MalformedJson_Returns400NotServerError()
    {
        using var content = new StringContent("{not json", System.Text.Encoding.UTF8, "application/json");
        var response = await factory.CreateClient().PostAsync("/api/v1/auth/login", content, TestApi.Ct).ShouldBeAsync(HttpStatusCode.BadRequest);

        (await response.ProblemAsync()).Title.ShouldBe("Request.Malformed");
    }
}
