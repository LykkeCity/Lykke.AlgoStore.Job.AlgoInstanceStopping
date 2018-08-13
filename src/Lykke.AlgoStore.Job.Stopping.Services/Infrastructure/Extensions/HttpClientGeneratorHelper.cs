using Lykke.AlgoStore.Service.Statistics.Client;

namespace Lykke.AlgoStore.Job.Stopping.Services.Infrastructure.Extensions
{
    public static class HttpClientGeneratorHelper
    {
        public static T GenerateClient<T>(string authToken, string serviceUrl) where T : class
        {
            var authHandler = new AlgoAuthorizationHeaderHttpClientHandler(authToken);
            var instanceEventHandler = HttpClientGenerator.HttpClientGenerator
            .BuildForUrl(serviceUrl)
            .WithAdditionalDelegatingHandler(authHandler);
            var client = instanceEventHandler.Create().Generate<T>();
            return client;
        }
    }
}
