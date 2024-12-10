using Microsoft.Extensions.Configuration;

namespace RonSijm.Syringe.Tests.Features.TestHelpers
{
    public static class StringToConfigurationRootHelper
    {
        public static IConfigurationRoot ToConfiguration(this string appSetting)
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(appSetting));

            var configuration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            return configuration;
        }
    }
}