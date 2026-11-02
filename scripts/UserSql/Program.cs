using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Identity;

if (args is ["--self-test"])
{
    var password = Guid.NewGuid().ToString("N");
    var hasher = new PasswordHasher<object>();
    var user = new object();
    var hash = hasher.HashPassword(user, password);
    var sql = CreateSql("o'brien@example.com", "User", hash);
    if (hasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Success
        || hasher.VerifyHashedPassword(user, hash, password + "wrong") != PasswordVerificationResult.Failed
        || !sql.Contains("N'o''brien@example.com'") || !sql.Contains(hash)
        || !sql.Contains("N'User'") || sql.Contains(password)
        || !CreateSql("admin@example.com", "Admin", hash).Contains("N'Admin'"))
        throw new InvalidOperationException("Fallo la comprobacion del generador.");
    Console.WriteLine("OK: hash Identity, rechazo de clave incorrecta, escape SQL, roles y ausencia de clave en SQL.");
    return;
}

try
{
    if (args.Length is < 1 or > 3)
        throw new ArgumentException("Uso: dotnet run --project scripts/UserSql -- correo [User|Admin] [archivo.sql]");
    var email = args[0].Trim().ToLowerInvariant();
    if (email.Length > 200 || !MailAddress.TryCreate(email, out var address) || address.Address != email)
        throw new ArgumentException("Introduce un correo valido de hasta 200 caracteres.");
    var role = args.Length > 1 ? args[1] : "User";
    if (role is not ("User" or "Admin"))
        throw new ArgumentException("El rol debe ser User o Admin.");
    var output = Path.GetFullPath(args.Length > 2 ? args[2] : "database/06-create-user.generated.sql");
    if (File.Exists(output))
        throw new IOException("El archivo ya existe. Indica otro nombre para no sobrescribirlo.");
    if (Console.IsInputRedirected)
        throw new InvalidOperationException("Ejecuta la herramienta en una terminal interactiva para introducir la clave oculta.");
    Console.Write("Contrasena (minimo 12 caracteres): ");
    var password = ReadPassword();
    Console.Write("Confirmar contrasena: ");
    var confirmation = ReadPassword();
    if (password.Length < 12 || password != confirmation)
        throw new ArgumentException("Las claves deben coincidir y tener al menos 12 caracteres.");
    var user = new object();
    var hasher = new PasswordHasher<object>();
    var hash = hasher.HashPassword(user, password);
    if (hasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Success)
        throw new InvalidOperationException("No se pudo verificar el hash.");
    using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
    using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
        writer.Write(CreateSql(email, role, hash));
    Console.WriteLine($"SQL generado: {output}");
    Console.WriteLine("Ejecutalo manualmente despues de crear las tablas. No se conecto a SQL Server.");
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Environment.ExitCode = 1;
}

static string ReadPassword()
{
    var password = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0) password.Length--;
        }
        else if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
    }
    Console.WriteLine();
    return password.ToString();
}

static string CreateSql(string email, string role, string hash) => $"""
USE [AtlanticPedidos];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF EXISTS (SELECT 1 FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK) WHERE Email = N'{email.Replace("'", "''")}')
    THROW 50001, 'El correo ya existe. No se modificaron sus credenciales ni su rol.', 1;
INSERT INTO dbo.Usuarios (Email, PasswordHash, Rol)
VALUES (N'{email.Replace("'", "''")}', N'{hash}', N'{role}');
COMMIT;
SELECT Id, Email, Rol FROM dbo.Usuarios WHERE Email = N'{email.Replace("'", "''")}';
GO
""";