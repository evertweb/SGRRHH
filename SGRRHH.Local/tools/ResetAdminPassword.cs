// Script para resetear la contraseña del admin
// Ejecutar con: dotnet script ResetAdminPassword.cs

using BCrypt.Net;

var password = "admin123";
var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

Console.WriteLine($"Password: {password}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine();
Console.WriteLine("SQL para actualizar:");
Console.WriteLine($"UPDATE usuarios SET password_hash = '{hash}' WHERE username = 'admin';");
