using apiDDKMA.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace apiDDKMA.Services
{
    public class CarritoService
    {
        private readonly IConfiguration _config;
        private readonly PastelService _pastelService;

        public CarritoService(IConfiguration config, PastelService pastelService)
        {
            _config = config;
            _pastelService = pastelService;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("AzureSQL"));
        }

        public async Task<CarritoDto> GetUserCartAsync(int userId)
        {
            using var connection = GetConnection();

            // Obtener el carrito activo
            var cart = await connection.QueryFirstOrDefaultAsync<Carrito>(
                "SELECT * FROM [carrito] WHERE fk_usuario = @UserId AND activo = 1",
                new { UserId = userId });

            if (cart == null)
            {
                return new CarritoDto
                {
                    UsuarioId = userId,
                    Activo = true,
                    Items = new List<CarritoItemDto>(),
                    Total = 0
                };
            }

            // Obtener los items del carrito con información del pastel
            var items = (await connection.QueryAsync<CarritoItemDto>(
                @"SELECT ci.id AS Id, 
             ci.fk_carrito AS CarritoId, 
             ci.fk_pastel AS PastelId, 
             p.Nombre AS NombrePastel, 
             p.Imagen AS ImagenPastel, 
             ci.cantidad AS Cantidad, 
             ci.precio_unitario AS PrecioUnitario,
             (ci.cantidad * ci.precio_unitario) AS Subtotal
      FROM [carrito_items] ci
      INNER JOIN [pastel] p ON ci.fk_pastel = p.Id
      WHERE ci.fk_carrito = @CartId",
                new { CartId = cart.Id })).ToList();

            // Calcular el total
            var total = items.Sum(i => i.Subtotal);

            return new CarritoDto
            {
                Id = cart.Id,
                UsuarioId = userId,
                Fecha = cart.Fecha,
                Activo = cart.Activo,
                Items = items,
                Total = total
            };
        }

        public async Task<CarritoItemDto> AddToCartAsync(AddToCartRequest request)
        {
            if (request.UserId <= 0)
            {
                throw new ArgumentException("El ID de usuario no es válido");
            }

            // Validar el pastel primero
            var pastel = await _pastelService.GetPastelByIdAsync(request.ProductId);
            if (pastel == null || !pastel.Disponible || pastel.Deleted)
            {
                throw new Exception("El pastel no está disponible");
            }

            if (pastel.Stock < request.Quantity)
            {
                throw new Exception($"No hay suficiente stock. Disponible: {pastel.Stock}");
            }

            using var connection = GetConnection();
            await connection.OpenAsync();

            try
            {
                // Configurar parámetros para el stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@UsuarioId", request.UserId);
                parameters.Add("@PastelId", request.ProductId);
                parameters.Add("@Cantidad", request.Quantity);
                parameters.Add("@PrecioUnitario", pastel.Precio);
                parameters.Add("@CarritoId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                // Ejecutar el stored procedure
                await connection.ExecuteAsync(
                    "sp_AddProductToCart",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // Obtener el ID del carrito
                var cartId = parameters.Get<int>("@CarritoId");

                if (cartId == 0)
                {
                    throw new Exception("No se pudo crear o obtener el carrito");
                }

                // Obtener el ítem con los datos del pastel en una sola consulta
                var itemDto = await connection.QueryFirstOrDefaultAsync<CarritoItemDto>(
                    @"SELECT 
                ci.id AS Id,
                ci.fk_carrito AS CarritoId,
                ci.fk_pastel AS PastelId,
                p.Nombre AS NombrePastel,
                p.Imagen AS ImagenPastel,
                ci.cantidad AS Cantidad,
                ci.precio_unitario AS PrecioUnitario,
                (ci.cantidad * ci.precio_unitario) AS Subtotal
              FROM carrito_items ci
              INNER JOIN pastel p ON ci.fk_pastel = p.Id
              WHERE ci.fk_carrito = @CartId AND ci.fk_pastel = @ProductId",
                    new { CartId = cartId, ProductId = request.ProductId });

                if (itemDto == null)
                {
                    throw new Exception("El ítem no se pudo obtener del carrito después de agregarlo.");
                }

                return itemDto;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error de base de datos: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar al carrito: {ex.Message}", ex);
            }
        }


        public async Task<bool> RemoveFromCartAsync(int userId, int itemId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            // Verificar que el ítem pertenece al usuario antes de eliminar
            var affectedRows = await connection.ExecuteAsync(
                @"DELETE ci 
          FROM carrito_items ci
          JOIN carrito c ON ci.fk_carrito = c.id
          WHERE ci.id = @ItemId AND c.fk_usuario = @UserId",
                new
                {
                    ItemId = itemId,
                    UserId = userId
                });

            return affectedRows > 0;
        }

        public async Task<CarritoItemDto> UpdateCartItemAsync(int userId, int itemId, UpdateCartItemRequest request)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            try
            {
                // 1. Verificar que el ítem pertenece al usuario y obtener información necesaria
                var itemInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT ci.id, ci.fk_carrito AS CarritoId, ci.fk_pastel AS PastelId, 
                 p.stock, p.disponible, p.deleted, p.nombre AS PastelNombre,
                 p.imagen AS PastelImagen, ci.cantidad AS CurrentQuantity,
                 ci.precio_unitario AS precio_unitario
          FROM carrito_items ci
          JOIN pastel p ON ci.fk_pastel = p.id
          JOIN carrito c ON ci.fk_carrito = c.id
          WHERE ci.id = @ItemId AND c.fk_usuario = @UserId",
                    new { ItemId = itemId, UserId = userId });

                if (itemInfo == null)
                {
                    throw new Exception("Ítem no encontrado en el carrito del usuario");
                }

                // 2. Validaciones
                if (itemInfo.deleted || !itemInfo.disponible)
                {
                    throw new Exception("El pastel ya no está disponible");
                }

                if (itemInfo.stock < request.NewQuantity)
                {
                    throw new Exception($"No hay suficiente stock. Disponible: {itemInfo.stock}");
                }

                // 3. Actualizar cantidad usando el stored procedure
                await connection.ExecuteAsync(
                    "sp_UpdateCartItemQuantity",
                    new
                    {
                        CarritoId = itemInfo.CarritoId,
                        ItemId = itemId,
                        Cantidad = request.NewQuantity,
                        IsCustom = false
                    },
                    commandType: CommandType.StoredProcedure);

                // 4. Devolver el ítem actualizado
                return new CarritoItemDto
                {
                    Id = itemId,
                    CarritoId = itemInfo.CarritoId,
                    PastelId = itemInfo.PastelId,
                    NombrePastel = itemInfo.PastelNombre,
                    ImagenPastel = itemInfo.PastelImagen,
                    Cantidad = request.NewQuantity,
                    PrecioUnitario = itemInfo.precio_unitario,
                    Subtotal = request.NewQuantity * itemInfo.precio_unitario
                };
            }
            catch (Exception ex)
            {
                // Log del error (mejor usar ILogger en producción)
                Console.WriteLine($"Error al actualizar ítem: {ex.Message}");
                throw;
            }
        }

        private async Task<CarritoItemDto> GetCartItemDtoAsync(int itemId)
        {
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<CarritoItemDto>(
                @"SELECT 
            ci.id AS Id, 
            ci.fk_carrito AS CarritoId,  // Alias cambiado a CarritoId
            ci.fk_pastel AS PastelId, 
            p.Nombre AS NombrePastel, 
            p.Imagen AS ImagenPastel, 
            ci.cantidad AS Cantidad, 
            ci.precio_unitario AS PrecioUnitario,
            (ci.cantidad * ci.precio_unitario) AS Subtotal
          FROM [carrito_items] ci
          INNER JOIN [pastel] p ON ci.fk_pastel = p.Id
          WHERE ci.id = @ItemId",
                new { ItemId = itemId });
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            using var connection = GetConnection();

            var affectedRows = await connection.ExecuteAsync(
                @"DELETE FROM [carrito_items] 
              WHERE fk_carrito IN (SELECT Id FROM [carrito] WHERE fk_usuario = @UserId AND activo = 1);
              
              UPDATE [carrito] SET activo = 0 WHERE fk_usuario = @UserId AND activo = 1",
                new { UserId = userId });

            return affectedRows > 0;
        }
    }
}
