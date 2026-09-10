using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantPOS.Infrastructure.Services;

namespace RestaurantPOS.Tests;

public sealed class JwtAuthenticationTests
{
    private const string Secret = "test-secret-key-for-restaurant-pos-2026-32-chars";
    private const string Issuer = "RestaurantPOS.Tests";
    private const string Audience = "RestaurantPOS.Tests.Client";

    [Fact]
    public void Generated_token_is_valid_and_preserves_role_and_branch_claims()
    {
        var userId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var service = CreateService();

        var token = service.GenerateToken(userId, "employee01", "Employee", "employee", branchId, "Branch A", "Cashier");

        var principal = service.ValidateToken(token);

        Assert.NotNull(principal);
        Assert.Equal(userId.ToString(), principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("employee", principal.FindFirst(ClaimTypes.Role)?.Value);
        Assert.Equal(branchId.ToString(), principal.FindFirst("branchId")?.Value);
    }

    [Fact]
    public void Expired_token_is_rejected()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(-1));

        Assert.Null(CreateService().ValidateToken(token));
    }

    [Fact]
    public void Token_with_wrong_signature_is_rejected()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(5), "another-secret-key-for-invalid-signature-32chars");

        Assert.Null(CreateService().ValidateToken(token));
    }

    [Fact]
    public void Token_with_wrong_issuer_is_rejected()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(5), issuer: "UntrustedIssuer");

        Assert.Null(CreateService().ValidateToken(token));
    }

    [Fact]
    public void Token_with_wrong_audience_is_rejected()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(5), audience: "UntrustedAudience");

        Assert.Null(CreateService().ValidateToken(token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("eyJhbGciOiJub25lIn0.invalid.token")]
    public void Malformed_or_empty_token_is_rejected(string token)
    {
        Assert.Null(CreateService().ValidateToken(token));
    }

    [Fact]
    public void Valid_token_without_role_claim_is_not_assigned_a_role()
    {
        var token = CreateToken(DateTime.UtcNow.AddMinutes(5), includeRole: false);

        var principal = CreateService().ValidateToken(token);

        Assert.NotNull(principal);
        Assert.Null(principal!.FindFirst(ClaimTypes.Role));
    }

    private static JwtService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = Secret,
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:ExpiryDays"] = "7"
            })
            .Build();

        return new JwtService(configuration);
    }

    private static string CreateToken(
        DateTime expires,
        string? secret = null,
        string issuer = Issuer,
        string audience = Audience,
        bool includeRole = true)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "user-without-role"),
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
        };
        if (includeRole)
        {
            claims.Add(new Claim(ClaimTypes.Role, "employee"));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? Secret)),
            SecurityAlgorithms.HmacSha256);
        var notBefore = expires.AddMinutes(-2);
        if (notBefore > DateTime.UtcNow)
        {
            notBefore = DateTime.UtcNow.AddMinutes(-1);
        }
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = notBefore,
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
