using FirebaseAdmin.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace SGRRHH.AuthServer;

/// <summary>
/// Servicio que delega la creación y verificación de sesiones al Firebase Admin SDK
/// mediante las "session cookies" (CreateSessionCookieAsync / VerifySessionCookieAsync).
/// </summary>
public class FirebaseSessionService
{
    private readonly TimeSpan _sessionDuration = TimeSpan.FromDays(14); // máximo recomendado por Firebase

    public FirebaseSessionService()
    {
    }

    /// <summary>
    /// Crea una Session Cookie (delegada por Firebase Admin) a partir de un idToken.
    /// Devuelve el valor de la cookie y su expiración UTC.
    /// </summary>
    public async Task<(string sessionCookie, DateTime expiresUtc)> CreateSessionCookieFromIdTokenAsync(string idToken)
    {
        var expiresIn = _sessionDuration;
        var sessionCookie = await FirebaseAuth.DefaultInstance.CreateSessionCookieAsync(idToken, new SessionCookieOptions
        {
            ExpiresIn = expiresIn
        });

        return (sessionCookie, DateTime.UtcNow.Add(expiresIn));
    }

    /// <summary>
    /// Verifica una session cookie usando Firebase Admin.
    /// Si checkRevoked es true, se validará que el token no haya sido revocado.
    /// </summary>
    public Task<FirebaseToken> VerifySessionCookieAsync(string sessionCookie, bool checkRevoked = false)
    {
        return FirebaseAuth.DefaultInstance.VerifySessionCookieAsync(sessionCookie, checkRevoked);
    }

    /// <summary>
    /// Revoca refresh tokens para un uid (útil en logout o invalidación).
    /// </summary>
    public Task RevokeRefreshTokensAsync(string uid)
    {
        return FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(uid);
    }
}
