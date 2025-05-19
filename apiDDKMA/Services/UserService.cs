using Microsoft.Data.SqlClient;
using Dapper;
using System;
using System.Threading.Tasks;
using apiDDKMA.Models;
using System.ComponentModel.DataAnnotations;

namespace apiDDKMA.Services
{
    public class UserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        // Get connection
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("AzureSQL"));
        }

        // Método existente (para obtener usuario)
        public async Task<Usuario> GetUserByIdAsync(int id)
        {
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                "SELECT * FROM [USER] WHERE Id = @Id",
                new { Id = id });
        }

        // Check if email exists
        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            using var connection = GetConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM [USER] WHERE Email = @Email",
                new { Email = email });
            return count > 0;
        }

        // Check if username exists
        public async Task<bool> UserExistsByUsernameAsync(string username)
        {
            using var connection = GetConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM [USER] WHERE Username = @Username",
                new { Username = username });
            return count > 0;
        }

        // Register new user
        public async Task<Usuario> RegisterUserAsync(RegisterRequest registerRequest)
        {
            // Check if user already exists
            if (await UserExistsByEmailAsync(registerRequest.Email) ||
                await UserExistsByUsernameAsync(registerRequest.Username))
            {
                throw new Exception("Usuario o email ya en uso");
            }

            // Hash the password ONCE
            string hashedPassword = HashPassword(registerRequest.Password);

            // Create new user with hashed password
            var newUser = new Usuario
            {
                Email = registerRequest.Email,
                Username = registerRequest.Username,
                PasswordHash = hashedPassword, // Use the variable we already created
                ProfilePicture = registerRequest.ProfilePicture,
                Telefono = registerRequest.Telefono,
                CreatedDT = DateTime.UtcNow,
                Rol = "cliente" // Default role
            };

            // Insert user into database
            using var connection = GetConnection();
            var sql = @"
        INSERT INTO [USER] (Email, Username, password, Rol, CreatedDT, ProfilePicture, Telefono)
        VALUES (@Email, @Username, @PasswordHash, @Rol, @CreatedDT, @ProfilePicture, @Telefono);
        SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await connection.ExecuteScalarAsync<int>(sql, newUser);
            newUser.Id = id;

            return newUser;
        }

        // Password hashing
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public async Task<Usuario> AuthenticateAsync(LoginRequest loginRequest)
        {
            using var connection = GetConnection();

            // Modificamos la consulta para ser explícitos con las columnas
            var sql = @"
        SELECT 
            Id, 
            Email, 
            Username, 
            Password AS PasswordHash, -- Explícitamente mapeamos Password a PasswordHash 
            Rol, 
            CreatedDT, 
            ProfilePicture, 
            Telefono 
        FROM [USER] 
        WHERE Email = @Login OR Username = @Login";

            var user = await connection.QueryFirstOrDefaultAsync<Usuario>(sql,
                new { Login = loginRequest.UsernameOrEmail });

            // Agregamos más logging para diagnóstico
            if (user == null)
            {
                // Console.WriteLine($"Usuario no encontrado: {loginRequest.UsernameOrEmail}");
                return null;
            }

            // Console.WriteLine($"Usuario encontrado: {user.Username}, Hash: {user.PasswordHash?.Substring(0, 20)}...");

            // Verificamos que el hash no sea null
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                // Console.WriteLine("El hash de contraseña es null o vacío");
                return null;
            }

            // Verificamos la contraseña con un poco más de diagnóstico
            bool passwordMatch = false;
            try
            {
                passwordMatch = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
                // Console.WriteLine($"Verificación de contraseña: {passwordMatch}");
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error al verificar contraseña: {ex.Message}");
                return null;
            }

            if (VerifyPassword(loginRequest.Password, user.PasswordHash))
                return user;

            return null;
        }

        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            {
                return false;
            }

            try
            {
                // Asegúrate de usar la implementación correcta
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error en verificación: {ex.Message}");
                return false;
            }
        }



    }
}