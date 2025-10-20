using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoListApi.DTOs;
using TodoListApi.Models;
using TodoListApi.Services;
using TodoListApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "TodoList API", 
        Version = "v1",
        Description = "A comprehensive TodoList API built with .NET 8 Minimal APIs with OAuth 2.0"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000", "http://127.0.0.1:5173", "http://127.0.0.1:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Add EF Core with SQLite
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

// Register services
//builder.Services.AddSingleton<ICategoryService, InMemoryCategoryService>();
//builder.Services.AddSingleton<ITodoService, InMemoryTodoService>();
builder.Services.AddScoped<ICategoryService, EfCategoryService>();
builder.Services.AddScoped<ITodoService, EfTodoService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Register HttpClient for making external API calls
// This provides a managed HttpClient with proper disposal and configuration
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    // Set a User-Agent header to identify our application to external APIs
    // Many APIs require this and it's good practice for monitoring
    client.DefaultRequestHeaders.Add("User-Agent", "TodoListAPI/1.0 (Educational Project)");
    
    // Set a default timeout for all HTTP requests made by this client
    // This prevents hanging requests when external APIs are slow
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Alternative registration if you prefer more explicit control:
// builder.Services.AddScoped<IWeatherService, WeatherService>();
// builder.Services.AddHttpClient(); // Registers IHttpClientFactory

var app = builder.Build();

// Ensure database is created with seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// Add authentication & authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// ==================== AUTH ENDPOINTS ====================

var authGroup = app.MapGroup("/api/auth")
    .WithTags("Authentication")
    .WithOpenApi();

authGroup.MapPost("/google", async ([FromBody] GoogleLoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.AuthenticateGoogleAsync(request.IdToken);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("GoogleLogin")
.WithSummary("Authenticate with Google")
.WithDescription("Authenticate user with Google OAuth 2.0 ID token")
.AllowAnonymous();

authGroup.MapPost("/microsoft", async ([FromBody] MicrosoftLoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.AuthenticateMicrosoftAsync(request.Code);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("MicrosoftLogin")
.WithSummary("Authenticate with Microsoft")
.WithDescription("Authenticate user with Microsoft OAuth 2.0 authorization code")
.AllowAnonymous();

authGroup.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.RefreshTokenAsync(request.RefreshToken);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("RefreshToken")
.WithSummary("Refresh access token")
.WithDescription("Get a new access token using a refresh token")
.AllowAnonymous();

authGroup.MapGet("/me", (HttpContext context) =>
{
    var user = context.User;
    var userInfo = new
    {
        Id = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
        Email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
        Name = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
        Provider = user.FindFirst("provider")?.Value
    };
    return Results.Ok(userInfo);
})
.WithName("GetCurrentUser")
.WithSummary("Get current authenticated user")
.WithDescription("Returns information about the currently authenticated user")
.RequireAuthorization();

// ==================== TODO ENDPOINTS ====================

// Todo API Endpoints
var todosGroup = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .WithOpenApi()
    .RequireAuthorization(); // All todo endpoints require authentication

todosGroup.MapGet("/", async (HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var todos = await todoService.GetAllTodosAsync(userId);
    var response = todos.Select(t => new TodoResponse
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Priority = t.Priority.ToString(),
        Category = t.Category,
        IsCompleted = t.IsCompleted,
        CreatedDate = t.CreatedDate,
        DueDate = t.DueDate,
        Tags = t.Tags
    });
    return Results.Ok(response);
})
.WithName("GetAllTodos")
.WithSummary("Get all todos")
.WithDescription("Retrieves all todos for the authenticated user");

todosGroup.MapGet("/{id}", async (string id, HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var todo = await todoService.GetTodoByIdAsync(id, userId);
    if (todo == null)
        return Results.NotFound($"Todo with ID {id} not found");

    var response = new TodoResponse
    {
        Id = todo.Id,
        Title = todo.Title,
        Description = todo.Description,
        Priority = todo.Priority.ToString(),
        Category = todo.Category,
        IsCompleted = todo.IsCompleted,
        CreatedDate = todo.CreatedDate,
        DueDate = todo.DueDate,
        Tags = todo.Tags
    };
    return Results.Ok(response);
})
.WithName("GetTodoById")
.WithSummary("Get todo by ID")
.WithDescription("Retrieves a specific todo by its ID for the authenticated user");

todosGroup.MapPost("/", async ([FromBody] CreateTodoRequest request, HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var todo = new Todo
    {
        Title = request.Title,
        Description = request.Description,
        Priority = request.Priority,
        Category = request.Category,
        DueDate = request.DueDate,
        Tags = request.Tags
    };

    var createdTodo = await todoService.CreateTodoAsync(todo, userId);
    var response = new TodoResponse
    {
        Id = createdTodo.Id,
        Title = createdTodo.Title,
        Description = createdTodo.Description,
        Priority = createdTodo.Priority.ToString(),
        Category = createdTodo.Category,
        IsCompleted = createdTodo.IsCompleted,
        CreatedDate = createdTodo.CreatedDate,
        DueDate = createdTodo.DueDate,
        Tags = createdTodo.Tags
    };

    return Results.Created($"/api/todos/{createdTodo.Id}", response);
})
.WithName("CreateTodo")
.WithSummary("Create a new todo")
.WithDescription("Creates a new todo item for the authenticated user");

todosGroup.MapPut("/{id}", async (string id, [FromBody] UpdateTodoRequest request, HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var existingTodo = await todoService.GetTodoByIdAsync(id, userId);
    if (existingTodo == null)
        return Results.NotFound($"Todo with ID {id} not found");

    var updatedTodo = new Todo
    {
        Id = existingTodo.Id,
        Title = request.Title ?? existingTodo.Title,
        Description = request.Description ?? existingTodo.Description,
        Priority = request.Priority ?? existingTodo.Priority,
        Category = request.Category ?? existingTodo.Category,
        IsCompleted = request.IsCompleted ?? existingTodo.IsCompleted,
        CreatedDate = existingTodo.CreatedDate,
        DueDate = request.DueDate ?? existingTodo.DueDate,
        Tags = request.Tags ?? existingTodo.Tags
    };

    var result = await todoService.UpdateTodoAsync(id, updatedTodo, userId);
    if (result == null)
        return Results.NotFound($"Todo with ID {id} not found");

    var response = new TodoResponse
    {
        Id = result.Id,
        Title = result.Title,
        Description = result.Description,
        Priority = result.Priority.ToString(),
        Category = result.Category,
        IsCompleted = result.IsCompleted,
        CreatedDate = result.CreatedDate,
        DueDate = result.DueDate,
        Tags = result.Tags
    };

    return Results.Ok(response);
})
.WithName("UpdateTodo")
.WithSummary("Update a todo")
.WithDescription("Updates an existing todo item");

todosGroup.MapDelete("/{id}", async (string id, HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var deleted = await todoService.DeleteTodoAsync(id, userId);
    if (!deleted)
        return Results.NotFound($"Todo with ID {id} not found");

    return Results.NoContent();
})
.WithName("DeleteTodo")
.WithSummary("Delete a todo")
.WithDescription("Deletes a todo item by ID for the authenticated user");

todosGroup.MapPatch("/{id}/toggle", async (string id, HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var existingTodo = await todoService.GetTodoByIdAsync(id, userId);
    if (existingTodo == null)
        return Results.NotFound($"Todo with ID {id} not found");

    var updatedTodo = new Todo
    {
        Id = existingTodo.Id,
        Title = existingTodo.Title,
        Description = existingTodo.Description,
        Priority = existingTodo.Priority,
        Category = existingTodo.Category,
        IsCompleted = !existingTodo.IsCompleted,
        CreatedDate = existingTodo.CreatedDate,
        DueDate = existingTodo.DueDate,
        Tags = existingTodo.Tags
    };

    var result = await todoService.UpdateTodoAsync(id, updatedTodo, userId);
    if (result == null)
        return Results.NotFound($"Todo with ID {id} not found");

    var response = new TodoResponse
    {
        Id = result.Id,
        Title = result.Title,
        Description = result.Description,
        Priority = result.Priority.ToString(),
        Category = result.Category,
        IsCompleted = result.IsCompleted,
        CreatedDate = result.CreatedDate,
        DueDate = result.DueDate,
        Tags = result.Tags
    };

    return Results.Ok(response);
})
.WithName("ToggleTodoComplete")
.WithSummary("Toggle todo completion")
.WithDescription("Toggles the completion status of a todo item");

// Category API Endpoints
var categoriesGroup = app.MapGroup("/api/categories")
    .WithTags("Categories")
    .WithOpenApi();

categoriesGroup.MapGet("/", async (ICategoryService categoryService) =>
{
    var categories = await categoryService.GetAllCategoriesAsync();
    var response = categories.Select(c => new CategoryResponse
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        Color = c.Color,
        TodoCount = c.TodoCount
    });
    return Results.Ok(response);
})
.WithName("GetAllCategories")
.WithSummary("Get all categories")
.WithDescription("Retrieves all categories from the system");

categoriesGroup.MapGet("/{id}", async (string id, ICategoryService categoryService) =>
{
    var category = await categoryService.GetCategoryByIdAsync(id);
    if (category == null)
        return Results.NotFound($"Category with ID {id} not found");

    var response = new CategoryResponse
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Color = category.Color,
        TodoCount = category.TodoCount
    };
    return Results.Ok(response);
})
.WithName("GetCategoryById")
.WithSummary("Get category by ID")
.WithDescription("Retrieves a specific category by its ID");

categoriesGroup.MapPost("/", async ([FromBody] CreateCategoryRequest request, ICategoryService categoryService) =>
{
    var category = new Category
    {
        Name = request.Name,
        Description = request.Description,
        Color = request.Color
    };

    var createdCategory = await categoryService.CreateCategoryAsync(category);
    var response = new CategoryResponse
    {
        Id = createdCategory.Id,
        Name = createdCategory.Name,
        Description = createdCategory.Description,
        Color = createdCategory.Color,
        TodoCount = createdCategory.TodoCount
    };

    return Results.Created($"/api/categories/{createdCategory.Id}", response);
})
.WithName("CreateCategory")
.WithSummary("Create a new category")
.WithDescription("Creates a new category");

categoriesGroup.MapPut("/{id}", async (string id, [FromBody] UpdateCategoryRequest request, ICategoryService categoryService) =>
{
    var existingCategory = await categoryService.GetCategoryByIdAsync(id);
    if (existingCategory == null)
        return Results.NotFound($"Category with ID {id} not found");

    var updatedCategory = new Category
    {
        Id = existingCategory.Id,
        Name = request.Name ?? existingCategory.Name,
        Description = request.Description ?? existingCategory.Description,
        Color = request.Color ?? existingCategory.Color,
        TodoCount = existingCategory.TodoCount
    };

    var result = await categoryService.UpdateCategoryAsync(id, updatedCategory);
    if (result == null)
        return Results.NotFound($"Category with ID {id} not found");

    var response = new CategoryResponse
    {
        Id = result.Id,
        Name = result.Name,
        Description = result.Description,
        Color = result.Color,
        TodoCount = result.TodoCount
    };

    return Results.Ok(response);
})
.WithName("UpdateCategory")
.WithSummary("Update a category")
.WithDescription("Updates an existing category");

categoriesGroup.MapDelete("/{id}", async (string id, ICategoryService categoryService) =>
{
    var deleted = await categoryService.DeleteCategoryAsync(id);
    if (!deleted)
        return Results.NotFound($"Category with ID {id} not found");

    return Results.NoContent();
})
.WithName("DeleteCategory")
.WithSummary("Delete a category")
.WithDescription("Deletes a category by ID");

// Stats API Endpoint
app.MapGet("/api/stats", async (HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var stats = await todoService.GetStatsAsync(userId);
    return Results.Ok(stats);
})
.WithTags("Statistics")
.WithName("GetTodoStats")
.WithSummary("Get todo statistics")
.WithDescription("Retrieves comprehensive statistics about todos for the authenticated user")
.WithOpenApi()
.RequireAuthorization();

// Weather API Endpoints
// These endpoints demonstrate external API integration patterns
var weatherGroup = app.MapGroup("/api/weather")
    .WithTags("Weather") // Groups endpoints in Swagger documentation
    .WithOpenApi();      // Enables OpenAPI documentation generation

// GET /api/weather/current/{city} - Get current weather for a specific city
weatherGroup.MapGet("/current/{city}", async (string city, IWeatherService weatherService) =>
{
    // Input validation - ensure the city parameter is not empty
    if (string.IsNullOrWhiteSpace(city))
    {
        return Results.BadRequest("City parameter is required and cannot be empty");
    }
    
    // Call our weather service to get current weather data
    // This demonstrates how to integrate external APIs into your application
    var weather = await weatherService.GetCurrentWeatherAsync(city);
    
    // Handle the case where weather data is not available
    // This could happen due to network issues, API limits, or invalid city names
    if (weather == null)
    {
        return Results.NotFound($"Weather information not available for city: {city}");
    }
    
    // Return the weather data as JSON
    // The framework automatically serializes our WeatherInfo object to JSON
    return Results.Ok(weather);
})
.WithName("GetCurrentWeather")
.WithSummary("Get current weather for a city")
.WithDescription("Retrieves current weather information for the specified city using the wttr.in API")
.Produces<WeatherInfo>(200)      // Documents successful response type
.Produces(400)                   // Documents bad request response
.Produces(404);

// GET /api/weather/forecast/{city} - Get 5-day weather forecast for a specific city
weatherGroup.MapGet("/forecast/{city}", async (string city, IWeatherService weatherService) =>
{
    // Input validation - ensure the city parameter is not empty
    if (string.IsNullOrWhiteSpace(city))
    {
        return Results.BadRequest("City parameter is required and cannot be empty");
    }
    
    // Call our weather service to get 5-day forecast data
    var forecast = await weatherService.GetForecastAsync(city);
    
    // Handle the case where forecast data is not available
    if (forecast == null)
    {
        return Results.NotFound($"Weather forecast not available for city: {city}");
    }
    
    // Return the forecast data as JSON
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithSummary("Get 5-day weather forecast for a city")
.WithDescription("Retrieves 5-day weather forecast for the specified city using the wttr.in API")
.Produces<WeatherForecast5Day>(200)
.Produces(400)
.Produces(404);                  // Documents not found response

// GET /api/weather/todos - Get weather-sensitive todos
weatherGroup.MapGet("/todos", async (IWeatherService weatherService) =>
{
    try
    {
        // Get todos that might be affected by weather conditions
        // This demonstrates combining external API capabilities with local data
        var weatherTodos = await weatherService.GetWeatherSensitiveTodosAsync();
        
        // Convert to response DTOs for consistent API responses
        var response = weatherTodos.Select(todo => new TodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            Priority = todo.Priority.ToString(),
            Category = todo.Category,
            IsCompleted = todo.IsCompleted,
            CreatedDate = todo.CreatedDate,
            DueDate = todo.DueDate,
            Tags = todo.Tags
        });
        
        return Results.Ok(response);
    }
    catch (Exception)
    {
        // Log the error and return a generic error response
        // In production, you might want more sophisticated error handling and logging
        return Results.Problem(
            title: "Error retrieving weather-sensitive todos",
            detail: "An error occurred while processing your request",
            statusCode: 500
        );
    }
})
.WithName("GetWeatherSensitiveTodos")
.WithSummary("Get weather-sensitive todos")
.WithDescription("Retrieves todos that contain weather-related keywords and might be affected by weather conditions")
.Produces<IEnumerable<TodoResponse>>(200)
.Produces(500);

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
.WithTags("Health")
.WithName("HealthCheck")
.WithSummary("Health check")
.WithDescription("Returns the health status of the API")
.WithOpenApi();

app.Run();
