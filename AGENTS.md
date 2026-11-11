# Lineamientos de microservicio BNC

Guía para armar un microservicio ASP.NET Core con la misma forma que `MsCreditCards`: un host HTTP delgado, negocio en servicios, acceso a datos solo en repositorios, y SQL fuera del código (nombres de stored procedures en configuración).

Reemplazá `{Dominio}` (ejemplo: `Cards`), `{Recurso}` (ejemplo: `Movements`) y `{Ms}` (ejemplo: `MsCreditCards`) por los del servicio nuevo. El proyecto host se llama `Ms{Dominio}`. El namespace de negocio es `BNC.Microservices.{Dominio}`.

## Stack

- ASP.NET Core Web API, `net10.0`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`.
- Un solo proyecto web por microservicio. La solución es un `.slnx` con ese proyecto.
- Contenedor Linux (`DockerDefaultTargetOS`), imagen `mcr.microsoft.com/dotnet/aspnet:10.0`, puertos `8080` (HTTP) y `8081` (HTTPS).
- Cultura del proceso: `CultureInfo.InvariantCulture` en cultura y UI, al inicio de `Program.Main`.
- No hay Entity Framework ni SQL embebido. Las lecturas pasan por Dapper y la librería compartida `IDBLayerDap`.

## Proyectos de infraestructura (hermanos, no viven en este repo)

El `.csproj` referencia librerías al mismo nivel que la carpeta del microservicio. El `Dockerfile` copia ese árbol (`CreditCards`, `DBLayerDap`, `Utilities`) porque `DockerfileContext` apunta dos niveles arriba del proyecto.

| Referencia | Rol en este servicio |
|---|---|
| `DBLayerDap` (`BNC.Microservices.Infrastructure`) | `AddDatabaseLayer`, `IDBLayerDap.QueryListAsync`, `IDBLayerDap.SaveExceptionAsync` |
| `Security` | `AddSecurityHardening`, `UseSecurityHardening`, `AddBankingCors` |
| `Utilities` | Atributos de validación compartidos (`DateLesserThan`, `DateGreaterThan`) |
| `SecretsHandler_Net10.0.dll` en `Libraries/SecretsHandler` | `RegisterSecretsHandler` |

No dupliques conexión, cifrado de secretos, CORS bancario ni endurecimiento de Kestrel dentro del microservicio. Llamá a esas extensiones desde `Program.cs`.

## Carpetas

```text
Ms{Dominio}/
  Program.cs                 composición: cultura, seguridad, secretos, DB, DI
  RepositorySettings.cs      options del catálogo de procedures y queries
  Controllers/               HTTP. Un controlador por recurso
  Services/                  orquestación. Interfaz + clase en el mismo archivo
  Repositories/              única capa que habla con IDBLayerDap
  Models/                    requests HTTP y DTOs de salida
  appsettings.json           connection names vacíos + mapa de procedures
  appsettings.Development.json
  Properties/launchSettings.json
  Dockerfile
```

## Flujo de una operación

```text
HTTP GET api/{Recurso}/{Accion}?query
  -> Controller valida el request por DataAnnotations
  -> I{Dominio}Service
  -> I{Dominio}Repository
  -> IDBLayerDap.QueryListAsync<TDto>(nombreSp, DynamicParameters, connectionName)
  -> 200 OK con el DTO
```

El controlador no arma SQL ni parámetros de Dapper. El servicio no abre conexiones. El repositorio no devuelve `IActionResult`.

Varios controladores pueden compartir el mismo servicio y el mismo repositorio cuando operan sobre el mismo agregado. En este servicio, `CardsController` y `MovementsController` usan `ICardsService` / `ICardsRepository`. Un recurso nuevo (`Balances`) tiene su controlador propio aunque todavía no tenga acciones.

## `Program.cs`

Orden fijo:

1. `WebApplication.CreateBuilder`.
2. Cultura invariante.
3. `AddSecurityHardening()` y `AddBankingCors(configuration, "BNCCorsPolicy")`. La política se lee de configuración; el nombre de la policy es `BNCCorsPolicy`.
4. Si existe `/run/secrets`, `Configuration.AddKeyPerFile("/run/secrets", optional: true)`.
5. `RegisterSecretsHandler(configuration)` para descifrar secretos.
6. `AddDatabaseLayer(configuration)`.
7. Negocio en `AddScoped`: `I{Dominio}Repository` y `I{Dominio}Service`.
8. `Configure<RepositorySettings>(configuration.GetSection("RepositorySettings"))`.
9. `AddHealthChecks()`, `AddControllers()`, `AddOpenApi()`.
10. Pipeline: `UseSecurityHardening`, `MapOpenApi` solo en Development, `UseHttpsRedirection`, `UseCors("BNCCorsPolicy")`, `UseAuthorization`, `MapHealthChecks("/health")`, `MapControllers`.

Lifetime: servicio y repositorio son `Scoped`. No uses `Singleton` para nada que reciba `IDBLayerDap`.

## Controladores

- Namespace: `BNC.Microservices.Ms{Dominio}.Controllers`.
- `[ApiController]`, `[Route("api/[controller]")]`, heredan `ControllerBase`.
- Una acción por caso de uso. Verbo y ruta describen la consulta: `[HttpGet("ListById")]`, `[HttpGet("Last25")]`.
- Entrada por query string: `[FromQuery] {Accion}Request model`. `[ApiController]` responde `400` si fallan las `DataAnnotations`; no hace falta un `if (!ModelState.IsValid)` manual.
- Declarar `[ProducesResponseType]` para `200`, `400` y `500`.
- Comentario XML `/// <summary>` en cada acción, en español, describiendo qué devuelve.
- Inyectar el servicio de negocio y `IDBLayerDap`. Asignar ambos campos en el constructor. `IDBLayerDap` en el controlador solo se usa para registrar la excepción.
- Cuerpo de la acción: `try` / `await` al servicio / `return Ok(resultado)`.
- En el `catch (Exception ex)`:
  - `await _dbLayer.SaveExceptionAsync(ex, HttpContext.Request.GetDisplayUrl(), "{Servicio}.{Metodo}");`
  - el tercer argumento es el método que falló, no un nombre genérico reutilizado;
  - responder `500` con un cuerpo anónimo y mensaje fijo: `new { Message = "Ocurrió un error interno procesando la solicitud." }`;
  - no devolver `ex.Message`, stack trace ni detalle de base de datos.

```csharp
[HttpGet("ListById")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> ListById([FromQuery] CardsListByIdRequest model)
{
    try
    {
        var cards = await _cardsService.GetCardsByCustomerAsync(model.DocType.ToUpper(), model.DocNumber);
        return Ok(cards);
    }
    catch (Exception ex)
    {
        await _dbLayer.SaveExceptionAsync(ex, HttpContext.Request.GetDisplayUrl(), "CardsService.GetCardsByCustomerAsync");
        return StatusCode(StatusCodes.Status500InternalServerError,
            new { Message = "Ocurrió un error interno procesando la solicitud." });
    }
}
```

Normalizaciones mínimas de entrada (por ejemplo `DocType.ToUpper()`) pueden vivir en el controlador. Reglas de negocio no.

## Servicios

Archivo `Services/{Dominio}Service.cs`. Interfaz y clase en el mismo archivo. Namespace `BNC.Microservices.{Dominio}.Services`.

- La interfaz expone un método por caso de uso, `Task<IEnumerable<TDto>>` (u otro DTO de `Models`).
- La clase solo depende de `I{Dominio}Repository`.
- Cada método delega al repositorio. Si más adelante hay regla de negocio, va aquí, nunca en el controlador ni en el repositorio.

## Repositorios

Archivo `Repositories/{Dominio}Repository.cs`. Interfaz y clase en el mismo archivo. Namespace `BNC.Microservices.{Dominio}.Repositories`.

- Dependencias: `IDBLayerDap` y `IOptions<RepositorySettings>`.
- Constante con el nombre lógico de la conexión. Tiene que coincidir con una clave de `ConnectionStrings` (en este servicio, `BNCTREOInterface`).
- Por método:
  1. `DynamicParameters` con tipo explícito (`DbType.AnsiString`, `DbType.Int32`, `DbType.Decimal`, `DbType.Date`). Si el procedure fija largo, pasar `size`.
  2. Nombre del procedure desde `_repSettings.{Conexion}.StoredProcedures["ClaveLogica"]`. Prohibido hardcodear `[Schema].[Procedure]` en C#.
  3. `await _dbLayer.QueryListAsync<TDto>(sp, parameters, ConnectionName)`.
  4. Si `!response.IsSuccess`, lanzar `ApplicationException` con `response.Message`. El controlador la convierte en `500` genérico y la persiste.
  5. Devolver `response.Content`.

```csharp
public async Task<IEnumerable<CardSummaryDto>> GetCardsByCustomerIdAsync(string docType, int docNumber)
{
    var parameters = new DynamicParameters();
    parameters.Add("@DocType", docType, DbType.AnsiString, size: 1);
    parameters.Add("@DocNumber", docNumber, DbType.Int32);

    var response = await _dbLayer.QueryListAsync<CardSummaryDto>(
        _repSettings.BNCTREOInterface.StoredProcedures["CardsListById"],
        parameters,
        ConnectionName);

    if (!response.IsSuccess)
        throw new ApplicationException($"Error de base de datos al consultar tarjetas: {response.Message}");

    return response.Content;
}
```

El DTO genérico de `QueryListAsync<T>` se mapea por nombre de columna del procedure a las propiedades del record. No hay mapper manual.

## Modelos

Dos tipos, ambos bajo `Models/`:

| Tipo | Namespace | Forma | Responsabilidad |
|---|---|---|---|
| Request | el del host, junto al controlador que lo bindea | `class` | Contrato de entrada HTTP. Solo DataAnnotations |
| DTO | `BNC.Microservices.{Dominio}.Models` | `record` con propiedades `set` | Contrato de salida y mapeo del result set |

Requests:

- Una clase por acción: `{Accion}Request`.
- `[Required]`, `[Range]`, `[StringLength]`, `[RegularExpression]` con `ErrorMessage` en español.
- Comparación de fechas con los atributos de `Utilities` (`DateLesserThan`, `DateGreaterThan`), no con lógica en el controlador.
- Tipos alineados al procedure: documento como `string` de 1 carácter + `int`; número de tarjeta como `decimal` (no `string`, no `long`).

DTOs:

- Nombres de propiedades iguales a las columnas que devuelve el procedure.
- Sin atributos de validación y sin lógica.

## Configuración

`appsettings.json` deja las conexiones vacías y publica el catálogo de procedures. Los valores reales van a `appsettings.Development.json`, user-secrets o `/run/secrets`. No commitear passwords, usuarios ni servidores en el lineamiento ni en archivos de ejemplo.

```json
{
  "ConnectionStrings": {
    "BNCTREOInterface": "",
    "ExceptionDB": ""
  },
  "RepositorySettings": {
    "BNCTREOInterface": {
      "StoredProcedures": {
        "CardsListById": "[Cards].[p_API_CardsByID_Get]",
        "GetMovementsLast25": "[Movements].[p_API_CardAuthorizationsLast25_Get]",
        "GetMovementsByDate": "[Movements].[p_API_CardAuthorizationsByDate_Get]"
      }
    }
  }
}
```

`RepositorySettings` es la clase de options:

- Una propiedad por base (`BNCTREOInterface`, `Corebanking`, …).
- Cada base de procedures expone `Dictionary<string, string> StoredProcedures`.
- La clave del diccionario es el nombre lógico que usa el repositorio. El valor es `[Schema].[p_API_...]`.
- `ExceptionDB` es la conexión que usa `SaveExceptionAsync`; no se consulta desde los repositorios de negocio.
- Si el servicio también pega a core bancario, la sección `Corebanking` lleva `Schema` y `Queries`, separada de los stored procedures.

Al agregar un endpoint nuevo: procedure en el diccionario, método en la interfaz del repositorio, método en el servicio, acción en el controlador, request y DTO. En ese orden.

## Errores y respuestas

| Situación | Quién la produce | HTTP |
|---|---|---|
| Request inválido | `[ApiController]` + DataAnnotations | 400, mensaje del atributo |
| Fallo de `IDBLayerDap` (`IsSuccess == false`) | repositorio lanza `ApplicationException` | 500 genérico, excepción persistida |
| Cualquier otra excepción | `catch` del controlador | 500 genérico, excepción persistida |
| Consulta vacía | el procedure devuelve lista vacía | 200 con `[]` |

## Docker y ejecución local

- Multi-stage: `base` (aspnet, `USER $APP_UID`, `EXPOSE 8080` y `8081`), `build` (restore del csproj y de los proyectos hermanos), `publish` con `/p:UseAppHost=false`, `final` con `ENTRYPOINT ["dotnet", "Ms{Dominio}.dll"]`.
- Perfiles en `launchSettings.json`: `http`, `https`, `Container (Dockerfile)` e `IIS Express`. Development setea `ASPNETCORE_ENVIRONMENT=Development`.
- Health check público: `GET /health`.
- OpenAPI solo en Development (`MapOpenApi`).

## Qué no hacer

- No poner `DynamicParameters`, nombres de procedures ni connection strings dentro de controladores o servicios.
- No devolver el detalle de la excepción al cliente.
- No registrar servicio o repositorio como `Singleton`.
- No crear un repositorio por acción. Un repositorio por agregado; los controladores del mismo agregado lo comparten.
- No mapear columnas a mano si el nombre de la propiedad del DTO ya coincide con la columna.
- No inventar una segunda vía de acceso a datos (ADO.NET suelto, EF, HTTP a la base). Toda lectura nueva entra por `IDBLayerDap`.
