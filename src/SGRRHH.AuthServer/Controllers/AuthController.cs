using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SGRRHH.AuthServer;
using System.ComponentModel.DataAnnotations;
using FirebaseAdmin.Auth;
using SGRRHH.Core.Enums;
using SGRRHH.Infrastructure.Firebase;

namespace SGRRHH.AuthServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FirebaseSessionService _sessionService;
    private readonly IWebHostEnvironment _env;

    private readonly FirebaseInitializer _firebase;

    public AuthController(FirebaseSessionService sessionService, FirebaseInitializer firebase, IWebHostEnvironment env)
    {
        _sessionService = sessionService;
        _firebase = firebase;
        _env = env;
    }

    public class SessionLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }

    [HttpPost("sessionLogin")]
    public async Task<IActionResult> SessionLogin([FromBody] SessionLoginRequest req)
    {
        if (string.IsNullOrEmpty(req.IdToken)) return BadRequest("idToken required");

        try
        {
            // Crear session cookie via Firebase Admin
            var (sessionCookie, expiresUtc) = await _sessionService.CreateSessionCookieFromIdTokenAsync(req.IdToken);

            // Set secure cookie (para cross-origin se usa SameSite=None)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expiresUtc,
                IsEssential = true
            };
            Response.Cookies.Append("sgrrhh_session", sessionCookie, cookieOptions);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("sessionLogout")]
    public async Task<IActionResult> SessionLogout()
    {
        var sessionCookie = Request.Cookies["sgrrhh_session"];
        if (!string.IsNullOrEmpty(sessionCookie))
        {
            try
            {
                // Verificar cookie para obtener uid y revocar tokens en Firebase
                var decoded = await _sessionService.VerifySessionCookieAsync(sessionCookie, checkRevoked: false);
                if (decoded != null)
                {
                    await _sessionService.RevokeRefreshTokensAsync(decoded.Uid);
                }
            }
            catch (FirebaseAuthException fae)
            {
                // Si la cookie es inválida/expirada, igual limpiar cookie
                Console.WriteLine($"SessionLogout: firebase verify error: {fae}");
            }

            // Borrar cookie del cliente
            Response.Cookies.Append("sgrrhh_session", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1),
                IsEssential = true
            });
        }
        return Ok(new { success = true });
    }

    [HttpPost("test/create-user")]
    public async Task<IActionResult> CreateTestUser([FromBody] CreateUserRequest req)
    {
        // Permitir si estamos en Development o si la flag Dev:AllowTestUserCreation está activada
        var allowTests = _env.IsDevelopment() || (HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) is Microsoft.Extensions.Configuration.IConfiguration cfg && cfg.GetValue<bool>("Dev:AllowTestUserCreation"));
        if (!allowTests) return Forbid();
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password)) return BadRequest("email and password required");

        try
        {
            var args = new UserRecordArgs
            {
                Email = req.Email,
                Password = req.Password,
                DisplayName = req.DisplayName
            };
            var user = await FirebaseAuth.DefaultInstance.CreateUserAsync(args);
            Console.WriteLine($"Created test user: {user.Uid} ({user.Email})");

            // Also create a minimal Firestore user document so that web login succeeds in E2E
            try
            {
                if (_firebase != null && _firebase.IsInitialized)
                {
                    var docRef = _firebase.GetDocument("users", user.Uid);
                    if (docRef != null)
                    {
                        var data = new Dictionary<string, object>
                        {
                            ["Username"] = req.Email,
                            ["FirebaseUid"] = user.Uid,
                            ["NombreCompleto"] = string.IsNullOrWhiteSpace(req.DisplayName) ? req.Email : req.DisplayName!,
                            ["Email"] = req.Email,
                            ["Rol"] = (int)RolUsuario.Operador,
                            ["Activo"] = true,
                            ["FechaCreacion"] = DateTime.UtcNow
                        };
                        await docRef.SetAsync(data);
                        Console.WriteLine($"Created Firestore user doc for: {user.Uid}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateTestUser: failed to create Firestore doc: {ex.Message}");
            }

            return Ok(new { uid = user.Uid, email = user.Email });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateTestUser error: {ex}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var sessionCookie = Request.Cookies["sgrrhh_session"];
        if (string.IsNullOrEmpty(sessionCookie)) return Unauthorized();

        try
        {
            var decoded = await _sessionService.VerifySessionCookieAsync(sessionCookie, checkRevoked: true);
            return Ok(new { uid = decoded.Uid, claims = decoded.Claims });
        }
        catch (FirebaseAuthException)
        {
            return Unauthorized();
        }
    }
}
