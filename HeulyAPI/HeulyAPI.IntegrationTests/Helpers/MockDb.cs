using HeulyAPI.Data;
using HeulyAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace HeulyAPI.IntegrationTests.Helpers
{
    public class MockDb<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remover o banco de dados real
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Adicionar banco de dados em memória
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryUsersDb");
                });

                // Criar um escopo para o serviço e injetar dados de teste
                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    var userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
                    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

                    db.Database.EnsureCreated();

                    try
                    {
                        // Injeta dados de teste no banco de dados
                        SeedData.Initialize(db, userManager, roleManager).Wait();
                    }
                    catch (Exception ex)
                    {
                        var logger = scopedServices.GetRequiredService<ILogger<MockDb<TProgram>>>();
                        logger.LogError(ex, "Ocorreu um erro ao injetar os dados de teste no banco de dados.");
                    }
                }
            });
        }
    }
}