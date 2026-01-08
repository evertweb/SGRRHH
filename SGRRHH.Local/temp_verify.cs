using BCrypt.Net;

var hash = @"$2a$11$XBN0YhJz5xJ8vE0F8XQKL.rNJE5qxQJvJ8VF3xLZqyK5Z5tHqLXm2";
var password = "admin123";

try {
    var result = BCrypt.Net.BCrypt.Verify(password, hash);
    Console.WriteLine("Verificacion: " + (result ? "CORRECTA" : "INCORRECTA"));
} catch (Exception ex) {
    Console.WriteLine("Error: " + ex.Message);
    
    // Generar nuevo hash
    var newHash = BCrypt.Net.BCrypt.HashPassword(password, 11);
    Console.WriteLine("Nuevo hash: " + newHash);
}
