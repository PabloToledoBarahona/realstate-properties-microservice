using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using PropertiesService.Dtos;
using PropertiesService.Models;
using PropertiesService.Services;

namespace PropertiesService.Repositories
{
    public class PropertyRepository
    {
        private readonly Cassandra.ISession _session;

        public PropertyRepository(CassandraSessionFactory factory)
        {
            _session = factory.GetSession();
        }

        public async Task CreateAsync(Property property)
        {
            //
            // 1) Inserción principal en properties_by_id
            //
            var insertMain = _session.Prepare(@"
                INSERT INTO properties_by_id (
                    id_property, id_user, title, description, address, city, country,
                    property_type, transaction_type, price, area, built_area, bedrooms,
                    status, photos, created_at, updated_at
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            await _session.ExecuteAsync(insertMain.Bind(
                property.IdProperty,
                property.IdUser,
                property.Title,
                property.Description,
                property.Address,
                property.City,
                property.Country,
                property.PropertyType,
                property.TransactionType,
                property.Price,
                property.Area,
                property.BuiltArea,
                property.Bedrooms,
                property.Status,
                property.Photos,
                property.CreatedAt,
                property.UpdatedAt
            ));

            //
            // 2) Inserción en properties_by_city (opción 1)
            //
            var insertByCity = _session.Prepare(@"
                INSERT INTO properties_by_city (
                    city, id_property, id_user, title, description, address, country,
                    property_type, transaction_type, price, area, built_area, bedrooms,
                    status, photos, created_at, updated_at
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            await _session.ExecuteAsync(insertByCity.Bind(
                property.City,
                property.IdProperty,
                property.IdUser,
                property.Title,
                property.Description,
                property.Address,
                property.Country,
                property.PropertyType,
                property.TransactionType,
                property.Price,
                property.Area,
                property.BuiltArea,
                property.Bedrooms,
                property.Status,
                property.Photos,
                property.CreatedAt,
                property.UpdatedAt
            ));

            //
            // 3) Inserción en properties_by_filter (opción 2)
            //
            var insertByFilter = _session.Prepare(@"
                INSERT INTO properties_by_filter (
                    city, property_type, transaction_type, status,
                    price, area, id_property, created_at, id_user, title
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            await _session.ExecuteAsync(insertByFilter.Bind(
                // El orden debe coincidir con la definición de la tabla:
                property.City,             // city
                property.PropertyType,     // property_type
                property.TransactionType,  // transaction_type
                property.Status,           // status
                property.Price,            // price
                property.Area,             // area
                property.IdProperty,       // id_property
                property.CreatedAt,        // created_at
                property.IdUser,           // id_user
                property.Title             // title
            ));

            //
            // 4) Inserción en properties_by_user
            //
            var insertByUser = _session.Prepare(@"
                INSERT INTO properties_by_user (
                    id_user, created_at, id_property, title, city, country, price, status
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)
            ");
            await _session.ExecuteAsync(insertByUser.Bind(
                property.IdUser,
                property.CreatedAt,
                property.IdProperty,
                property.Title,
                property.City,
                property.Country,
                property.Price,
                property.Status
            ));
        }

        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            var rs = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM properties_by_id"));
            return MapRowsToProperties(rs);
        }

        public async Task<IEnumerable<Property>> GetByCityAsync(string city)
        {
            // Consulta por ciudad usando properties_by_city
            var stmt = _session.Prepare("SELECT * FROM properties_by_city WHERE city = ?");
            var rs = await _session.ExecuteAsync(stmt.Bind(city));
            return MapRowsToProperties(rs);
        }

        private IEnumerable<Property> MapRowsToProperties(RowSet rows)
        {
            return rows.Select(row => new Property
            {
                IdProperty = row.GetValue<Guid>("id_property"),
                IdUser = row.GetValue<Guid>("id_user"),
                Title = row.GetValue<string>("title"),
                Description = row.GetValue<string>("description"),
                Address = row.GetValue<string>("address"),
                City = row.GetValue<string>("city"),
                Country = row.GetValue<string>("country"),
                PropertyType = row.GetValue<string>("property_type"),
                TransactionType = row.GetValue<string>("transaction_type"),
                Price = row.GetValue<decimal>("price"),
                Area = row.GetValue<int>("area"),
                BuiltArea = row.GetValue<int>("built_area"),
                Bedrooms = row.GetValue<int>("bedrooms"),
                Status = row.GetValue<string>("status"),
                Photos = row.GetValue<List<string>>("photos"),
                CreatedAt = row.GetValue<DateTime>("created_at"),
                UpdatedAt = row.GetValue<DateTime>("updated_at")
            });
        }

        public async Task<bool> UpdateAsync(Guid idProperty, Guid idUser, UpdatePropertyInput input)
        {
            // Verifica propietario
            var select = "SELECT id_user FROM properties_by_id WHERE id_property = ?";
            var row = (await _session.ExecuteAsync(new SimpleStatement(select, idProperty))).FirstOrDefault();
            if (row == null || row.GetValue<Guid>("id_user") != idUser)
                return false;

            var updates = new List<string>();
            var values = new List<object>();

            if (input.Title != null) { updates.Add("title = ?"); values.Add(input.Title); }
            if (input.Description != null) { updates.Add("description = ?"); values.Add(input.Description); }
            if (input.Address != null) { updates.Add("address = ?"); values.Add(input.Address); }
            if (input.City != null) { updates.Add("city = ?"); values.Add(input.City); }
            if (input.Country != null) { updates.Add("country = ?"); values.Add(input.Country); }
            if (input.PropertyType != null) { updates.Add("property_type = ?"); values.Add(input.PropertyType); }
            if (input.TransactionType != null) { updates.Add("transaction_type = ?"); values.Add(input.TransactionType); }
            if (input.Price.HasValue) { updates.Add("price = ?"); values.Add(input.Price.Value); }
            if (input.Area.HasValue) { updates.Add("area = ?"); values.Add(input.Area.Value); }
            if (input.BuiltArea.HasValue) { updates.Add("built_area = ?"); values.Add(input.BuiltArea.Value); }
            if (input.Bedrooms.HasValue) { updates.Add("bedrooms = ?"); values.Add(input.Bedrooms.Value); }
            if (input.Status != null) { updates.Add("status = ?"); values.Add(input.Status); }
            if (input.Photos != null) { updates.Add("photos = ?"); values.Add(input.Photos); }

            updates.Add("updated_at = ?");
            values.Add(DateTime.UtcNow);
            values.Add(idProperty);

            var cql = $"UPDATE properties_by_id SET {string.Join(", ", updates)} WHERE id_property = ?";
            await _session.ExecuteAsync(new SimpleStatement(cql, values.ToArray()));

            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid idProperty, Guid currentUserId, string newStatus)
        {
            // 1) Actualiza en properties_by_id
            await _session.ExecuteAsync(new SimpleStatement(@"
                UPDATE properties_by_id
                SET status = ?, updated_at = toTimestamp(now())
                WHERE id_property = ?", newStatus, idProperty));

            // 2) Obtiene created_at para el índice de usuario
            var rs = await _session.ExecuteAsync(new SimpleStatement(@"
                SELECT created_at
                FROM properties_by_user
                WHERE id_user = ? ALLOW FILTERING", currentUserId));

            var createdAt = rs.FirstOrDefault()?.GetValue<DateTime>("created_at");
            if (createdAt == null)
                return false;

            // 3) Actualiza en properties_by_user
            await _session.ExecuteAsync(new SimpleStatement(@"
                UPDATE properties_by_user
                SET status = ?
                WHERE id_user = ? AND created_at = ? AND id_property = ?",
                newStatus, currentUserId, createdAt.Value, idProperty));

            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Delete
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(Guid idProperty, Guid currentUserId)
        {
            // 1) Recuperamos la fila para chequear propietario y llaves de borrado
            var row = (await _session.ExecuteAsync(
                new SimpleStatement("SELECT * FROM properties_by_id WHERE id_property = ?", idProperty)))
                .FirstOrDefault();

            if (row is null || row.GetValue<Guid>("id_user") != currentUserId)
                return false; // No existe o no es tuya

            // Campos necesarios para eliminar índices
            var city = row.GetValue<string>("city");
            var propertyType = row.GetValue<string>("property_type");
            var transactionType = row.GetValue<string>("transaction_type");
            var status = row.GetValue<string>("status");
            var createdAt = row.GetValue<DateTime>("created_at");

            //
            // 2) Borramos tabla principal
            //
            await _session.ExecuteAsync(new SimpleStatement(
                "DELETE FROM properties_by_id WHERE id_property = ?", idProperty));

            //
            // 3) Borramos índices secundarios
            //
            // 3-a  properties_by_city
            await _session.ExecuteAsync(new SimpleStatement(
                "DELETE FROM properties_by_city WHERE city = ? AND id_property = ?",
                city, idProperty));

            // 3-b  properties_by_filter
            await _session.ExecuteAsync(new SimpleStatement(@"
        DELETE FROM properties_by_filter
        WHERE  city = ?
          AND  property_type = ?
          AND  transaction_type = ?
          AND  status = ?
          AND  price = ?
          AND  area  = ?
          AND  id_property = ?",
                city, propertyType, transactionType, status,
                row.GetValue<decimal>("price"),
                row.GetValue<int>("area"),
                idProperty));

            // 3-c  properties_by_user
            await _session.ExecuteAsync(new SimpleStatement(@"
        DELETE FROM properties_by_user
        WHERE id_user = ? AND created_at = ? AND id_property = ?",
                currentUserId, createdAt, idProperty));

            return true;
        }

        public async Task<IEnumerable<Property>> GetByUserAsync(Guid userId)
        {
            var stmt = _session.Prepare("SELECT * FROM properties_by_user WHERE id_user = ?");
            var bound = stmt.Bind(userId);
            var rows = await _session.ExecuteAsync(bound);

            return rows.Select(row => new Property
            {
                IdProperty = row.GetValue<Guid>("id_property"),
                IdUser = userId,
                Title = row.GetValue<string>("title"),
                City = row.GetValue<string>("city"),
                Country = row.GetValue<string>("country"),
                Price = row.GetValue<decimal>("price"),
                Status = row.GetValue<string>("status"),
                CreatedAt = row.GetValue<DateTime>("created_at"),
                UpdatedAt = DateTime.UtcNow
            });
        }
    }
}