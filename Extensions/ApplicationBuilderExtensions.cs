using backend_api_dotnet9.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
            AppDbSeeder.SeedSampleDataAsync(dbContext).GetAwaiter().GetResult();
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
          {
              options.SwaggerEndpoint("/swagger/v1/swagger.json", "backend-api-dotnet9 v1");
              options.RoutePrefix = "swagger";
          });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseCors(ServiceCollectionExtensions.ClientCorsPolicy);
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapControllers();
        app.MapGet("/", () => Results.Redirect("/swagger"));

        return app;
    }
}
