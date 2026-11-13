# MsAuth — autenticación y panel

Este repo tiene los dos proyectos juntos. Las copias separadas están en `C:\Users\gabot\source\repos\authentication` (solo el API) y `C:\Users\gabot\source\repos\authenticationmanagement` (solo el panel). Si hay que alinear código, la fuente funcional es la de esos dos repos.

`MsAuth` emite el JWT y mantiene usuarios, servicios, endpoints, permisos y roles. `MsAuthAdmin` es el sitio Razor que llama a ese API. La API key no se guarda en claro: SQL la persiste con `HASHBYTES('SHA2_256', ...)`. El mantenimiento exige un Bearer con el claim `role` = `Administrator`. `POST api/Permissions/IssueToken` no pide ese rol: se autentica con `uidUser` + `apiKey` + `idService`.

## Stack

- `net10.0`, nullable e implicit usings. Dos soluciones: `MsAuth.slnx` y `MsAuthAdmin.slnx`.
- API: cultura invariante, sin Entity Framework y sin SQL en C#. Dapper vía `IDBLayerDap.QueryListAsync`.
- Paquetes BNC en `Packages/` (`nuget.config` apunta ahí y a nuget.org): `BNC.DBLayerDap`, `BNC.Security`, `BNC.Utilities`, `BNC.SecretsHandler` 1.0.0. Además hay un `Reference` a `..\..\Libraries\SecretsHandler\SecretsHandler_Net10.0.dll`.
- JWT con `System.IdentityModel.Tokens.Jwt`. OpenAPI y Scalar solo en Development.
- Contenedor Linux del API, `mcr.microsoft.com/dotnet/aspnet:10.0`, puertos `8080` y `8081`. `DockerfileContext` es `..\..`.
- Panel: Razor Pages, Bootstrap en tema oscuro, sin base propia y sin paquetes NuGet extra.

`AddSecurityHardening`, `AddBankingCors`, `RegisterSecretsHandler` y `AddDatabaseLayer` vienen de las librerías BNC. No reimplementar CORS, Kestrel, cifrado de secretos ni la conexión dentro del microservicio.

## Carpetas

```text
msAuth/
  nuget.config
  Packages/              nupkg locales de BNC
  Scripts.txt            esquema [Auth] y procedures; se corre a mano
  MsAuth.slnx
  MsAuth/                API
    Program.cs
    RepositorySettings.cs
    JwtSettings.cs
    Controllers/         MaintenanceController + un controlador por recurso
    Services/AuthService.cs
    Repositories/AuthRepository.cs
    Models/              requests en MsAuth.Models; DTOs en BNC.Microservices.Auth.Models
  MsAuthAdmin.slnx
  MsAuthAdmin/           panel
    Program.cs
    Models/ApiModels.cs
    Services/            AuthApiClient, AdminSession, AdminAuthorizeFilter
    Pages/               Login, Users, Services, Endpoints, Permissions, Roles
```

Namespaces del API: host y controladores `BNC.Microservices.MsAuth`; negocio `BNC.Microservices.Auth.Services`; datos `BNC.Microservices.Auth.Repositories`; DTOs `BNC.Microservices.Auth.Models`; requests `MsAuth.Models`. El panel vive en `MsAuthAdmin` y `MsAuthAdmin.Pages`.

## API

```text
HTTP
 -> Controller (DataAnnotations; mantenimiento vía MaintenanceController.ExecuteAsync)
 -> IAuthService
 -> IAuthRepository
 -> IDBLayerDap.QueryListAsync<T>(procedure de config, DynamicParameters, "BNCAuth")
```

El controlador no arma SQL ni `DynamicParameters`. El servicio no abre conexiones. El repositorio no devuelve `IActionResult`. Un solo `IAuthRepository` / `IAuthService`, ambos `Scoped`. No registrar como `Singleton` nada que reciba `IDBLayerDap`.

`Program.Main`, en este orden: cultura invariante, `AddSecurityHardening`, `AddBankingCors(..., "BNCCorsPolicy")`, `/run/secrets` si existe, `RegisterSecretsHandler`, `AddDatabaseLayer`, un `Dictionary<string, object>` singleton con `ConnectionStrings`, `AddScoped<IDBLayerDap, DBLayerDap>`, los dos servicios de negocio, `RepositorySettings` y `JwtSettings`, health, controllers, OpenAPI. Pipeline: `UseSecurityHardening`, en Development `MapOpenApi` y `MapScalarApiReference`, HTTPS, CORS, `UseAuthorization`, `GET /health`, controllers. No hay `AddAuthentication`: el Bearer lo valida `AuthService`.

`MaintenanceController.ExecuteAsync`:

| `MaintenanceResult` | HTTP |
|---|---|
| `IsAuthorized == false` | 401 `{ Message = "Credenciales o permisos inválidos." }` |
| `ResultCode` 0 | 200 con `Data` |
| 1 | 404 con `Message` |
| 2 | 409 con `Message` |
| otro (3 inválido, 4 rol protegido) | 400 con `Message` |
| excepción | `SaveExceptionAsync(ex, url, "{Servicio}.{Metodo}")` y 500 genérico, sin `ex.Message` |

`PermissionsController.IssueToken` no usa `ExecuteAsync`: mismos 401 y 500, y 200 con `IssueTokenResponse`.

| Verbo y ruta | Body / query | Qué hace |
|---|---|---|
| `POST api/Permissions/IssueToken` | `UidUser`, `ApiKey`, `IdService` | Emite el JWT. No exige Administrator |
| `POST api/Users/Create` | `Name`, `ApiKey` | El GUID lo genera el procedure |
| `POST api/Users/Update` | `IdUser`, `Name`, `IsActive`, `ApiKey?` | `ApiKey` vacía conserva el hash |
| `GET api/Users/List` | — | Sin API key |
| `POST api/Users/Delete` | `IdUser` | Borra el usuario, sus permisos, sus roles y sus tokens |
| `POST api/Services/Create` | `Name` | |
| `POST api/Services/Update` | `IdService`, `Name`, `IsActive` | |
| `GET api/Services/List` | — | |
| `POST api/Endpoints/Create` | `IdService`, `Name`, `HttpMethod`, `Route` | |
| `POST api/Endpoints/Update` | `IdEndpoint`, `Name`, `HttpMethod`, `Route`, `IsActive` | |
| `GET api/Endpoints/ListByService` | `IdService` | |
| `POST api/Permissions/Create` | `IdUser`, `IdService`, `IdEndpoint` | Si ya existe, el procedure lo reactiva |
| `POST api/Permissions/Update` | `IdPermission`, `IsActive` | |
| `GET api/Permissions/ListByUser` | `IdUser` | |
| `POST api/Roles/Create` | `Name` | |
| `POST api/Roles/Update` | `IdRole`, `Name`, `IsActive` | |
| `GET api/Roles/List` | — | |
| `POST api/Roles/Assign` | `IdUser`, `IdRole` | Si ya existe, la reactiva |
| `POST api/Roles/UpdateAssignment` | `IdUserRole`, `IsActive` | |
| `GET api/Roles/ListByUser` | `IdUser` | |

No hay `GetById`. El panel lista y filtra.

Requests en `MsAuth.Models`, solo DataAnnotations, mensajes en español. Nombres con largo máximo y sin quedar en blanco. Ids con `[Range(1, int.MaxValue)]`. `IsActive` es `bool?` con `[Required]`. `HttpMethod` acepta `*` (todos los verbos), `GET`, `POST`, `PUT`, `PATCH` y `DELETE`. `Route` puede ser un controlador (`CreditCards`), un controlador y acción (`Cards/ListAvailableCards`) o una ruta que empiece con `/`, hasta 300 caracteres. `IssueTokenRequest` rechaza `Guid.Empty`.

### Emisión del token

1. `GetUserPermissions`. Sin filas, o `ResultCode` distinto de 0 y de 3, o ids vacíos: 401.
2. Códigos de ese procedure: `0` con endpoints, `1` usuario o API key inválidos, `2` servicio inexistente o inactivo, `3` usuario y servicio válidos pero sin endpoints.
3. Endpoints solo con código 0, `IdEndpoint > 0` y método y ruta. Roles activos con `GetUserRoles`.
4. Sin endpoints y sin roles: 401. Con código 3 y al menos un rol, el JWT sale sin claims `endpoint`.
5. HMAC-SHA256. `LifetimeHours` de config, o 24 si viene en 0. `SigningKey` de al menos 32 caracteres.
6. Claims: `sub` = `UidUser`, `jti`, `serviceId`, un `role` por rol, un `endpoint` con el texto `{HttpMethod} {Route}`. Issuer `BNC.MsAuth`, audience `BNC.Microservices`.
7. `SaveAccessToken`. La respuesta trae `Token`, `Bearer` y `DtExpires` de la fila guardada.

El mantenimiento pasa por `Gate`: Bearer con o sin prefijo, issuer, audience, firma y lifetime (`ClockSkew` de 1 minuto, `MapInboundClaims = false`). Hace falta el claim `role` = `Administrator`.

`FromRow`: 0 devuelve la fila; 1 no existe; 2 duplicado; 4 el rol Administrator no se renombra ni se desactiva; otro usa el mensaje del caller o "La solicitud no es válida para el mantenimiento." Si el procedure no devuelve fila, `ApplicationException`. `ResultCode` de los DTO tiene `[JsonIgnore]`.

### Datos

Conexión lógica `BNCAuth`. Procedures desde `_repSettings.BNCAuth.StoredProcedures`, nunca hardcodeados. `ExceptionDB` solo la usa `SaveExceptionAsync`.

`Scripts.txt` define el esquema `[Auth]`: `User` (`uidUser`, `apiKeyHash`), `Service`, `Endpoint` único por `(idService, httpMethod, route)`, `Permission` único por `(idUser, idEndpoint)`, `AccessToken` (este código no revoca), `Role` (siembra `Administrator`) y `UserRole`. Un permiso cuyo endpoint no pertenece al servicio, o un rol inactivo al asignar, devuelve código 3.

Catálogo en `appsettings.json`: `GetUserPermissions`, `GetUserRoles`, `SaveAccessToken`, `InsertUser`, `UpdateUser`, `ListUsers`, `InsertService`, `UpdateService`, `ListServices`, `InsertEndpoint`, `UpdateEndpoint`, `ListEndpointsByService`, `InsertPermission`, `UpdatePermission`, `ListPermissionsByUser`, `InsertRole`, `UpdateRole`, `ListRoles`, `AssignUserRole`, `UpdateUserRole`, `ListUserRolesByUser`.

`appsettings.json` deja `BNCAuth`, `ExceptionDB` y `Jwt:SigningKey` vacíos. Los valores reales van a user-secrets, `appsettings.Development.json` local o `/run/secrets`. No commitear passwords, servidores ni la clave de firma. Perfiles del API: `http://localhost:5090`, `https://localhost:7090`, IIS Express SSL `44360`.

Para una operación nueva: procedure en `Scripts.txt` y en el catálogo, método en el repositorio, método en el servicio (`Gate` si es mantenimiento), request y DTO, acción con `ExecuteAsync`.

## Panel

`AuthApi:BaseUrl` y `AuthApi:ServiceId` salen de configuración. En Development de este repo la base es `http://localhost:5090/` (el perfil http del API de acá) y el servicio es `1`. `HttpClient<AuthApiClient>` usa esa base, camelCase y timeout de 30 segundos. `ApiCall<T>` carga `Message`, después `errors`, después `title`. Status 0 es fallo de conexión o de configuración.

La sesión es la cookie `.MsAuthAdmin` (`HttpOnly`, `Lax`, esencial, idle de 12 horas) con `msauth.token`, `msauth.expires` y `msauth.user`. `AdminAuthorizeFilter` deja pasar solo `/Login`, `/Logout` y `/Error`. Un 401 del API cierra la sesión y vuelve a `/Login`. Logout es POST. El alta de usuarios tiene el botón Generar, que arma 32 caracteres con `crypto.getRandomValues` y los muestra para copiarlos.

| Ruta | Llamadas |
|---|---|
| `/Login` | `POST api/Permissions/IssueToken` |
| `/Users` y `/Users/Edit/{id}` | `api/Users/List`, `Create`, `Update` |
| `/Services` y `/Services/Edit/{id}` | `api/Services/List`, `Create`, `Update` |
| `/Endpoints` | `api/Services/List`, `api/Endpoints/ListByService`, `Create`, `Update` |
| `/Permissions` | listas de usuarios, servicios y endpoints, `api/Permissions/ListByUser`, `Create`, `Update` |
| `/Roles` | `api/Roles/List`, `api/Users/List`, `api/Roles/ListByUser`, `Create`, `Update`, `Assign`, `UpdateAssignment` |

Editar vuelve a pedir el listado y busca el id. Alertas: `TempData["Notice"]`, `ViewData["Error"]`, `TempData["Error"]`. Perfiles del panel: `http://localhost:5109`, `https://localhost:7227`, IIS Express SSL `44366`.

Una pantalla nueva hereda `AdminPageModel`, llama al cliente con `Token`, pasa por `RedirectIfUnauthorized` y se enlaza en `_Layout` y en el inicio.

## Qué no hacer

- No poner `DynamicParameters`, nombres de procedures ni connection strings en controladores o servicios.
- No devolver el detalle de una excepción al cliente ni loguear la API key.
- No abrir SQL desde el panel, ni guardar el token fuera de la sesión.
- No saltear `AdminAuthorizeFilter`. No tratar un 401 como error de formulario.
- No duplicar en el panel las reglas del API (rol Administrator, hash, rol protegido, unicidad).
- No hardcodear la URL del API. Sale de `AuthApi:BaseUrl`.
- El `Dockerfile` del API todavía copia proyectos hermanos (`DBLayerDap`, `Security`, `Utilities`) bajo `msAuth/`. El csproj ya usa los paquetes de `Packages/`. Si se toca la imagen, alinear el Dockerfile con el csproj.
