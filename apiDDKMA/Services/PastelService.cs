using Dapper;
using Microsoft.Data.SqlClient;
using apiDDKMA.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiDDKMA.Services
{
    public class PastelService
    {
        private readonly IConfiguration _config;

        public PastelService(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("AzureSQL"));
        }

        // Obtener todos los pasteles disponibles (simplificado para la vista)
        public async Task<IEnumerable<PastelDto>> GetAllPastelesSimpleAsync()
        {
            using var connection = GetConnection();
            var pasteles = await connection.QueryAsync<Pastel>(
                "SELECT Id, Nombre, Descripcion, Precio, Imagen FROM [pastel] WHERE Disponible = 1 AND Deleted = 0");

            return pasteles.Select(p => new PastelDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Imagen = p.Imagen
            });
        }

        // Obtener pasteles destacados (simplificado)
        public async Task<IEnumerable<PastelDto>> GetPastelesDestacadosSimpleAsync()
        {
            using var connection = GetConnection();
            var pasteles = await connection.QueryAsync<Pastel>(
                "SELECT Id, Nombre, Descripcion, Precio, Imagen FROM [pastel] WHERE Destacado = 1 AND Disponible = 1 AND Deleted = 0");

            return pasteles.Select(p => new PastelDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Imagen = p.Imagen
            });
        }

        // Métodos completos para administración (usan el modelo completo)
        public async Task<IEnumerable<Pastel>> GetAllPastelesAsync()
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<Pastel>(
                "SELECT * FROM [pastel] WHERE Deleted = 0");
        }

        public async Task<Pastel> GetPastelByIdAsync(int id)
        {
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<Pastel>(
                "SELECT * FROM [pastel] WHERE Id = @Id AND Deleted = 0",
                new { Id = id });
        }

        public async Task<Pastel> CreatePastelAsync(Pastel pastel)
        {
            using var connection = GetConnection();
            var sql = @"
                INSERT INTO [pastel] 
                (Nombre, Descripcion, Precio, Imagen, Destacado, Stock, Disponible, FechaCreacion)
                VALUES 
                (@Nombre, @Descripcion, @Precio, @Imagen, @Destacado, @Stock, @Disponible, @FechaCreacion);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await connection.ExecuteScalarAsync<int>(sql, pastel);
            pastel.Id = id;
            return pastel;
        }

        public async Task<bool> UpdatePastelAsync(Pastel pastel)
        {
            using var connection = GetConnection();
            var affectedRows = await connection.ExecuteAsync(
                @"UPDATE [pastel] SET 
                    Nombre = @Nombre,
                    Descripcion = @Descripcion,
                    Precio = @Precio,
                    Imagen = @Imagen,
                    Destacado = @Destacado,
                    Stock = @Stock,
                    Disponible = @Disponible
                WHERE Id = @Id AND Deleted = 0", pastel);

            return affectedRows > 0;
        }

        public async Task<bool> DeletePastelAsync(int id)
        {
            using var connection = GetConnection();
            var affectedRows = await connection.ExecuteAsync(
                "UPDATE [pastel] SET Deleted = 1 WHERE Id = @Id",
                new { Id = id });

            return affectedRows > 0;
        }
    }
}