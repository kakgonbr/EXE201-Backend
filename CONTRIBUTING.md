# Contributing — API controller workflow

This document describes the basic, repeatable steps for adding a new API surface (repository → service → controller) in this repository. Keep changes small, testable and registered in DI.

## Before you start
- Create a feature branch and push frequently.
- Keep commits focused and include tests where appropriate.
- Follow existing conventions for folders: `Repositories`, `Services`, `Controllers`, `Models/Dto`, `Utils`.

## 1) Add a repository
1. Create a concrete repository in `Repositories`, e.g. `WorkshopRepository.cs`.
2. Extract an interface alongside it, e.g. `IWorkshopRepository.cs`, and program to the interface.
   - Interface example:
     ```csharp
     public interface IWorkshopRepository
     {
         Task<Workshop?> GetById(int id);
         Task Add(Workshop entity);
         Task<int> Update(Workshop entity);
         Task Delete(int id);
         Task<List<Workshop>> GetAll();
     }
     ```
3. Implement repository logic using the `ExeContext` (EF `DbContext`).
4. Register the repository in DI (in `Program.cs`):

## 2) Configure DTO mapping
1. Create DTOs under `Models/Dto` (e.g. `WorkshopDto.cs`) for data returned by controllers.
2. Add mappings in the AutoMapper profile `Utils/DtoMapper.cs`:
3. Ensure AutoMapper is registered in `Program.cs` (already present):

## 3) Create a service
1. Create a service class in `Services`, e.g. `WorkshopService.cs`.
2. Extract an interface `IWorkshopService.cs` and program to it.
   - Service interface example:
     ```csharp
     public interface IWorkshopService
     {
         Task<WorkshopDto?> GetWorkshop(int id);
         Task<PagedResult<WorkshopDto>> GetAll(int page, int pageSize);
     }
     ```
3. Inject required repositories, mapper and logger into the service:
4. Register the service in DI.

## 4) Create a controller
1. Create a controller class in `Controllers`, e.g. `WorkshopController.cs`.
2. Inject the service into the controller:
   - Example:
     ```csharp
     [ApiController]
     [Route("api/[controller]")]
     public class WorkshopController : ControllerBase
     {
         private readonly IWorkshopService _workshopService;
         public WorkshopController(IWorkshopService workshopService)
         {
             _workshopService = workshopService;
         }

         [HttpGet("{id}")]
         public async Task<IActionResult> Get(int id)
         {
             var dto = await _workshopService.GetWorkshop(id);
             if (dto == null) return NotFound();
             return Ok(dto);
         }

         [HttpGet("all")]
         public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
         {
             var result = await _workshopService.GetAll(page, pageSize);
             return Ok(result); // returns PagedResult<WorkshopDto>
         }
     }
     ```
3. Secure endpoints using `[Authorize]` (and policies) when required.
   - Ensure authentication and authorization middlewares are enabled in `Program.cs`:
     - call `app.UseAuthentication();` before `app.UseAuthorization();`
     - register authentication schemes with `builder.Services.AddAuthentication(...).AddJwtBearer(...);` etc.

Note: a controller may call multiple services and repositories (e.g., for complex workflows). Keep controllers thin — orchestrate work in services.

## 5) Image upload flow (important)
- All image uploads must go through the image service. Do not accept or persist images directly in controller code or bypass the service.
- Typical client flow:
1. Prompt the user to upload an image from the client UI.
2. Client POSTs the image file to the image upload endpoint (e.g., `POST /api/images`). The image service returns a temporary image name/status.
3. The client then sends the returned image name to your server (for example as part of the object create/update payload) so server-side logic can call the image service `CheckImagePresent` / `ConsumeImage` (or equivalent) to validate and persist the image for the user/entity.
4. Until the image is consumed/confirmed it is considered temporary and may be deleted by the image service (see existing behavior: temporary images are cleaned up after a timeout).
- Controller example (existing pattern):
- The `ImagesController.Upload` endpoint returns a name; your other controllers/services must call the image service to attach/consume that name before treating the image as permanent.
- Staff uploads:
- Use the staff-only upload endpoint (`[Authorize(Roles = "staff")]`) when appropriate; still follow the consume/check flow for permanence.
- Rationale: this two-step flow prevents orphaned files and lets the image service automatically clean up unused temporary uploads.

## 6) Prefer injectable helpers over static calls
- Time: inject the time provider (`ITimeProvider`) instead of using `DateTime.Now` directly.
- Example:
    ```csharp
    private readonly ITimeProvider _timeProvider;
    var now = _timeProvider.Now;
    ```
  - Implementations include `TimeMachine` (used in tests) or a normal provider in production. Register the chosen implementation in DI (`AddSingleton<ITimeProvider, TimeMachine>()` or similar).
- Configuration: inject `IConfigurationService` instead of calling `Environment.GetEnvironmentVariable(...)` directly.
  - Example:
    ```csharp
    private readonly IConfigurationService _config;
    var connString = _config.DATABASE_CONNECTION;
    ```

Using injectable abstractions makes code testable and configurable.

## 7) Final checklist before PR
- Code compiles and passes.
- DI registrations are updated in `Program.cs`.
- New DTOs added to `Models/Dto`, mappings added to `Utils/DtoMapper.cs`.
- Controllers use `[ApiController]`, correct routes `api/[controller]`, and appropriate HTTP attributes (`[HttpGet]`, `[HttpPost]`, ...).
- Endpoints that require authentication are decorated with `[Authorize]`.
- No direct calls to `DateTime.Now` or `Environment.GetEnvironmentVariable` — use `ITimeProvider` and `IConfigurationService` instead.
- Paginated endpoints return `PagedResult<T>` for a unified format.
- Image uploads follow the required upload → check/consume flow; temporary images must not be assumed permanent until validated.

## References / Examples in this repo
- `Controllers\UserController.cs` — a minimal example of controller wiring (also demonstrates paged endpoints).
- `Repositories\UserRepository.cs` & `Services\UserService.cs` — repository, service and paged result examples.
- `Utils/DtoMapper.cs` — example AutoMapper profile.
- `Services/TimeMachine.cs`, `Services/NormalTimeProvider.cs` & `Services/ConfigurationService.cs` — example injectable helpers to prefer over static calls.
- `Controllers\ImagesController.cs` & `Services\IImageService.cs` — image upload flow (temporary name returned, then check/consume to persist).