var builder = WebApplication.CreateBuilder(args);
// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Register KeyVault service
builder.Services.AddSingleton<IKeyVaultService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var keyVaultUri = configuration["Azure:KeyVaultUri"] ??
                      throw new ArgumentNullException("KeyVaultUri is not configured.");
    return new KeyVaultService(keyVaultUri);
});

// Add OpenAI service
builder.Services.AddSingleton<IOpenAIService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new OpenAIService(sp.GetRequiredService<IKeyVaultService>(), configuration);
});

// Configure Cosmos Client
builder.Services.AddSingleton(provider =>
{
    var keyVaultService = provider.GetRequiredService<IKeyVaultService>();
    var configuration = provider.GetRequiredService<IConfiguration>();

    return Task.Run(async () =>
    {
        var connectionString = await keyVaultService.GetSecretAsync(
            configuration["Azure:CosmosDbConnectionString"] ??
            throw new ArgumentNullException("CosmosDbConnectionString is not configured."));
        return new CosmosClient(connectionString);
    }).Result;
});

// Register CosmosDB service
builder.Services.AddSingleton<ICosmosDbService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new CosmosDbService(sp.GetRequiredService<CosmosClient>(), configuration);
});


// Register controllers
builder.Services.AddControllers();


// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins("http://localhost:5500", "https://localhost:5500")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Get Azure B2C configuration
var tenantName = builder.Configuration["AzureB2C:TenantName"] ?? 
    throw new ArgumentNullException("TenantName is not configured.");
var policyName = builder.Configuration["AzureB2C:PolicyName"] ?? 
    throw new ArgumentNullException("PolicyName is not configured.");
var clientId = builder.Configuration["AzureB2C:ClientId"] ?? 
    throw new ArgumentNullException("ClientId is not configured.");

// Add authentication services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MetadataAddress = $"https://{tenantName}.b2clogin.com/{tenantName}.onmicrosoft.com/v2.0/.well-known/openid-configuration?p={policyName}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = clientId,
            ValidateLifetime = true,
        };
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SameUser", policy =>
        policy.RequireAssertion(context =>
        {
            var httpContext = context.Resource as HttpContext;
            if (httpContext != null && 
                httpContext.Request.RouteValues.TryGetValue("userId", out var requestedUserId))
            {
                var authenticatedUserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return authenticatedUserId != null && 
                       authenticatedUserId.Equals(requestedUserId.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }));
});

var app = builder.Build();

// Configure the HTTP request pipelines
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();