# Real Estate Properties Microservice

Este microservicio forma parte del ecosistema distribuido del sistema de gestión inmobiliaria. Es responsable de la gestión de propiedades, incluyendo su creación, almacenamiento y consulta, utilizando GraphQL como interfaz de consulta y Cassandra como motor NoSQL distribuido.

-----------------------------------------------------------------------

STACK TECNOLOGICO

- .NET 7 (ASP.NET Core)
- Cassandra DB
- GraphQL (HotChocolate)
- JWT Authentication
- FluentValidation
- Docker-ready (pendiente)

-----------------------------------------------------------------------

ESTRUCTURA DE CARPETAS

- Controllers/ — (si se agregan REST opcionalmente)
- GraphQL/
  - Mutations/
  - Queries/
  - Types/
- Dtos/ — Data Transfer Objects para validación/entrada
- Models/ — Entidades de dominio
- Repositories/ — Acceso a datos con Cassandra
- Services/ — Fábricas y utilidades compartidas
- Validators/ — Validaciones con FluentValidation
- Config/ — Opciones de configuración externa

-----------------------------------------------------------------------

INSTRUCCIONES DE EJECUCION

1. CONFIGURAR CASSANDRA

El microservicio requiere un keyspace creado previamente:

CREATE KEYSPACE IF NOT EXISTS realestate
WITH replication = {
  'class': 'SimpleStrategy',
  'replication_factor': 1
};

USE realestate;

CREATE TABLE IF NOT EXISTS properties_by_id (...);
CREATE TABLE IF NOT EXISTS properties_by_filter (...);

2. CONFIGURAR appsettings.json

Asegúrate de que el archivo tenga la sección correcta para Cassandra y JWT:

"Cassandra": {
  "ContactPoints": [ "127.0.0.1" ],
  "Port": 9042,
  "Keyspace": "realestate"
},
"Jwt": {
  "Issuer": "your-issuer",
  "Audience": "your-audience",
  "Key": "your-secret-key"
}

3. EJECUTAR

dotnet restore
dotnet run

Por defecto expone:
- Swagger UI: https://localhost:5191/swagger
- GraphQL Playground: https://localhost:5191/graphql

-----------------------------------------------------------------------

ENDPOINTS GRAPHQL

MUTATION

mutation {
  createProperty(input: {
    title: "Casa de Prueba"
    description: "Ejemplo"
    address: "Av. Siempre Viva 123"
    city: "Santa Cruz"
    country: "Bolivia"
    propertyType: "Casa"
    transactionType: "Venta"
    price: 95000
    area: 120
    builtArea: 90
    bedrooms: 2
    photos: ["https://ejemplo.com/foto1.jpg"]
  }) {
    idProperty
    title
  }
}

QUERY

query {
  propertiesByCity(city: "Santa Cruz") {
    idProperty
    title
    price
  }
}

-----------------------------------------------------------------------

SEGURIDAD

- Se requiere token JWT válido para mutaciones protegidas.
- El ClaimTypes.NameIdentifier se utiliza como IdUser.

-----------------------------------------------------------------------

NOTAS

- El microservicio no expone REST, sino unicamente GraphQL.
- El repositorio está listo para ser dockerizado.
- Asegúrate de registrar este microservicio en el Gateway o Service Registry correspondiente si se utiliza una arquitectura basada en API Gateway o Service Mesh.

-----------------------------------------------------------------------

AUTOR

Pablo Toledo
pablotoledo.dev | GitHub @PabloToledoBarahona
