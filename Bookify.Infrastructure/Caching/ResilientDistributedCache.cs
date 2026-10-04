using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Bookify.Infrastructure.Caching
{
    internal sealed class ResilientDistributedCache : IDistributedCache
    {
        private readonly IDistributedCache _innerCache;
        private readonly ILogger<ResilientDistributedCache> _logger;

        public ResilientDistributedCache(
            IDistributedCache innerCache,
            ILogger<ResilientDistributedCache> logger
            )
        {
            _innerCache = innerCache;
            _logger = logger;
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _innerCache.GetAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read key {Key} from cache", key);
                return null;
            }
        }

        public async Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                await _innerCache.SetAsync(key, value, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write key {Key} to cache", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _innerCache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove key {Key} from cache", key);
            }
        }

        public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _innerCache.RefreshAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh key {Key} in cache", key);
            }
        }

        public byte[]? Get(string key)
        {
            try
            {
                return _innerCache.Get(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read key {Key} from cache", key);
                return null;
            }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            try
            {
                _innerCache.Set(key, value, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write key {Key} to cache", key);
            }
        }

        public void Remove(string key)
        {
            try
            {
                _innerCache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove key {Key} from cache", key);
            }
        }

        public void Refresh(string key)
        {
            try
            {
                _innerCache.Refresh(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh key {Key} in cache", key);
            }
        }
    }
}