using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;

namespace AuthTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string apiKey = "AIzaSyAJSJZEkcr8S28p4m1-cUVzvq_waEcjf3A";
            string authDomain = "rrhh-forestech.firebaseapp.com";
            var config = new FirebaseAuthConfig
            {
                ApiKey = apiKey,
                AuthDomain = authDomain,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };
            var client = new FirebaseAuthClient(config);

            // Prueba con admin y password incorrecto
            string email = "admin@sgrrhh.local";
            string password = "PASS_INCORRECTO";

            Console.WriteLine($"Probando login para: {email} con password incorrecto");

            try
            {
                await client.SignInWithEmailAndPasswordAsync(email, password);
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine("❌ ERROR DE FIREBASE AUTH");
                Console.WriteLine($"Reason: {ex.Reason}");
                Console.WriteLine($"Message: {ex.Message}");
            }
        }
    }
}
