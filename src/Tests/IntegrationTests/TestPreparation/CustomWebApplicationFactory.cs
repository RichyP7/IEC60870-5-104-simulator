using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.TestPreparation;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    string OptionsPath = "../../../Configuration/SimulationOptionsTest.json";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.Sources.Clear();
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, OptionsPath);
            configBuilder.AddJsonFile(configPath, optional: false, reloadOnChange: true);
        });
    }

    public void UpdateOptionsPath(String newPath)
    {
        OptionsPath = newPath;
    }
}