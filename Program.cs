using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace api_user_secrets
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
      return Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(
          (context, config) => {
            // if not in Production environment (dotnet run) don't setup KeyVault and use the default Secret Storage managed through dotnet user-secrets
            if (!context.HostingEnvironment.IsProduction()) return;
            
            // if in Production environment (dotnet publish) setup KeyVault -- pull the KeyVault name from appsettings.json

            var builtConfig = config.Build();

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
              new KeyVaultClient.AuthenticationCallback(
                azureServiceTokenProvider.KeyVaultTokenCallback
              )
            );

            config.AddAzureKeyVault(
              $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/",
              keyVaultClient,
              new DefaultKeyVaultSecretManager()
            );
          }
        )
        .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
  }
}
