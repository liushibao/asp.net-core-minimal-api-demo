using Redis.OM;
using System;
using WebApi.Models;

namespace WebApi.Services
{
    public class IndexCreationService : IHostedService
    {
        private readonly RedisConnectionProvider _provider;
        public IndexCreationService(RedisConnectionProvider provider)
        {
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _provider.Connection.CreateIndexAsync(typeof(GdpRes));
            await _provider.Connection.CreateIndexAsync(typeof(InfoRes));
            await _provider.Connection.CreateIndexAsync(typeof(SmsCodeCache));
            await _provider.Connection.CreateIndexAsync(typeof(WxToken));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
