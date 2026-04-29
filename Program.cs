var builder = WebApplication.CreateBuilder(args);

// Debug: Show what config is loaded
var config = builder.Configuration.GetSection("Supabase");
var url = config["Url"];
var key = config["Key"];
Console.WriteLine($"=== DEBUG ===");
Console.WriteLine($"Supabase URL: '{url}'");
Console.WriteLine($"Supabase Key exists: {!string.IsNullOrEmpty(key)}");
Console.WriteLine($"Key starts with: {(key?.Length > 10 ? key.Substring(0, 10) : "null")}...");
Console.WriteLine($"==============");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddHttpClient("SupabaseClient", client =>
{
    var supabaseUrl = builder.Configuration["Supabase:Url"];
    var supabaseKey = builder.Configuration["Supabase:Key"];
    
    if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
    {
        throw new InvalidOperationException($"Supabase configuration is missing. URL: {(string.IsNullOrEmpty(supabaseUrl) ? "missing" : "present")}, Key: {(string.IsNullOrEmpty(supabaseKey) ? "missing" : "present")}");
    }
    
    client.BaseAddress = new Uri($"{supabaseUrl}/rest/v1/");
    client.DefaultRequestHeaders.Add("apikey", supabaseKey);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
    client.DefaultRequestHeaders.Add("Prefer", "return=representation");
});

builder.Services.AddScoped<SupabaseService>();
builder.Services.AddLogging();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
