using System.Security.Claims;
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
            // Inserción principal en properties_by_id
            var insertMain = _session.Prepare(@"
                INSERT INTO properties_by_id (
                    id_property, id_user, title, description, address, city, country,
                    property_type, transaction_type, price, area, built_area, bedrooms,
                    status, photos, created_at, updated_at
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            var boundMain = insertMain.Bind(
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
            );
            await _session.ExecuteAsync(boundMain);

            // Inserción secundaria para consulta por ciudad
            var insertByCity = _session.Prepare(@"
                INSERT INTO properties_by_city (
                    city, id_property, id_user, title, description, address, country,
                    property_type, transaction_type, price, area, built_area, bedrooms,
                    status, photos, created_at, updated_at
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ");
            var boundByCity = insertByCity.Bind(
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
            );
            await _session.ExecuteAsync(boundByCity);

            // Inserción terciaria para consulta por usuario
            var insertByUser = _session.Prepare(@"
                INSERT INTO properties_by_user (
                    id_user, created_at, id_property, title, city, country, price, status
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)
            ");
            var boundByUser = insertByUser.Bind(
                property.IdUser,
                property.CreatedAt,
                property.IdProperty,
                property.Title,
                property.City,
                property.Country,
                property.Price,
                property.Status
            );
            await _session.ExecuteAsync(boundByUser);
        }

        public async Task<IEnumerable<Property>> GetAllAsync()
        {
            var result = await _session.ExecuteAsync(new SimpleStatement("SELECT * FROM properties_by_id"));
            return MapRowsToProperties(result);
        }

        public async Task<IEnumerable<Property>> GetByCityAsync(string city)
        {
            var statement = _session.Prepare("SELECT * FROM properties_by_city WHERE city = ?");
            var bound = statement.Bind(city);
            var result = await _session.ExecuteAsync(bound);
            return MapRowsToProperties(result);
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
            // Verifica que la propiedad existe y pertenece al usuario
            var select = "SELECT * FROM properties_by_id WHERE id_property = ?";
            var row = (await _session.ExecuteAsync(new SimpleStatement(select, idProperty))).FirstOrDefault();
            if (row == null || row.GetValue<Guid>("id_user") != idUser)
                return false;

            var updates = new List<string>();
            var values = new List<object>();

            if (input.Title != null)          { updates.Add("title = ?");            values.Add(input.Title); }
            if (input.Description != null)    { updates.Add("description = ?");      values.Add(input.Description); }
            if (input.Address != null)        { updates.Add("address = ?");          values.Add(input.Address); }
            if (input.City != null)           { updates.Add("city = ?");             values.Add(input.City); }
            if (input.Country != null)        { updates.Add("country = ?");          values.Add(input.Country); }
            if (input.PropertyType != null)   { updates.Add("property_type = ?");    values.Add(input.PropertyType); }
            if (input.TransactionType != null){ updates.Add("transaction_type = ?"); values.Add(input.TransactionType); }
            if (input.Price.HasValue)         { updates.Add("price = ?");            values.Add(input.Price.Value); }
            if (input.Area.HasValue)          { updates.Add("area = ?");             values.Add(input.Area.Value); }
            if (input.BuiltArea.HasValue)     { updates.Add("built_area = ?");       values.Add(input.BuiltArea.Value); }
            if (input.Bedrooms.HasValue)      { updates.Add("bedrooms = ?");         values.Add(input.Bedrooms.Value); }
            if (input.Status != null)         { updates.Add("status = ?");           values.Add(input.Status); }
            if (input.Photos != null)         { updates.Add("photos = ?");           values.Add(input.Photos); }

            updates.Add("updated_at = ?");
            values.Add(DateTime.UtcNow);

            var cql = $"UPDATE properties_by_id SET {string.Join(", ", updates)} WHERE id_property = ?";
            values.Add(idProperty);

            var stmt = new SimpleStatement(cql, values.ToArray());
            await _session.ExecuteAsync(stmt);
            return true;
        }

        /// <summary>
        /// Actualiza únicamente el campo status (y también actualiza el índice en properties_by_user).
        /// </summary>
        public async Task<bool> UpdateStatusAsync(Guid idProperty, Guid currentUserId, string newStatus)
        {
            //
            // 1) Actualizar en properties_by_id
            //
            var updateMain = new SimpleStatement(@"
                UPDATE properties_by_id
                SET status = ?, updated_at = toTimestamp(now())
                WHERE id_property = ?", newStatus, idProperty);
            await _session.ExecuteAsync(updateMain);

            //
            // 2) Recuperar 'created_at' de la fila correspondiente en properties_by_user
            //    para poder usarlo en el UPDATE del índice. 
            //    Nota: añadimos 'id_property' en el SELECT para filtrar correctamente.
            //
            var selectUserIndex = new SimpleStatement(@"
                SELECT id_property, created_at
                FROM properties_by_user 
                WHERE id_user = ? ALLOW FILTERING", currentUserId);
            var rows = await _session.ExecuteAsync(selectUserIndex);

            DateTime? createdAtOfProperty = null;
            foreach (var r in rows)
            {
                // Ahora sí tenemos id_property en cada row → filtramos
                if (r.GetValue<Guid>("id_property") == idProperty)
                {
                    createdAtOfProperty = r.GetValue<DateTime>("created_at");
                    break;
                }
            }

            if (!createdAtOfProperty.HasValue)
            {
                // No encontramos la fila en properties_by_user → devolvemos false
                return false;
            }

            //
            // 3) Actualizar el estado en properties_by_user usando el 'created_at' hallado
            //
            var updateByUser = new SimpleStatement(@"
                UPDATE properties_by_user 
                SET status = ?
                WHERE id_user = ? AND created_at = ? AND id_property = ?",
                newStatus, currentUserId, createdAtOfProperty.Value, idProperty);
            await _session.ExecuteAsync(updateByUser);

            return true;
        }
    }
}