

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieMvcProject.Application.Interfaces.Caching;
using StackExchange.Redis;
using System.Text.Json;

namespace MovieMvcProject.Infrastructure.Services.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer? _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly bool _isRedisAvailable;
        private readonly IConfiguration _config;
        private readonly string _instanceName;

        public RedisCacheService(
            IDistributedCache cache,
            IConnectionMultiplexer? redis,
            ILogger<RedisCacheService> logger, IConfiguration config)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redis = redis;
            _isRedisAvailable = _redis?.IsConnected ?? false;
            if (!_isRedisAvailable)
            {
                _logger.LogWarning("Redis bağlantısı yok. Prefix/pattern kaldırma devre dışı.");
            }
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _instanceName = config.GetValue<string>("RedisCache:InstanceName") ?? "";
        }

        public string GetFullKey(string key) => _instanceName + key;

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options, cancellationToken);
            _logger.LogDebug("Cache set edildi: Key = {Key}", key);
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);
            if (json == null)
            {
                _logger.LogDebug("Cache miss: Key = {Key}", key);
                return default;
            }
            _logger.LogDebug("Cache hit: Key = {Key}", key);
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Cache kaldırıldı: Key = {Key}", key);
        }

        

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            //  Parametre kontrolü: Boş prefix kontrolü
            if (string.IsNullOrWhiteSpace(prefix)) return;

            //  Redis müsaitlik kontrolü
            if (!_isRedisAvailable || _redis == null) return;

            try
            {
                var keys = new List<RedisKey>();

                //  Prefix Hazırlığı: InstanceName + Kullanıcı Prefix'i + Wildcard (*)
                // Örn: "MovieApp:" + "movies:" + "*" => "MovieApp:movies:*"
                var fullPrefix = _instanceName + prefix;
                var searchPattern = fullPrefix + "*";

                //  Sunucu döngüsü: Tüm Redis node'larını tara
                foreach (var endpoint in _redis.GetEndPoints())
                {
                    var server = _redis.GetServer(endpoint);
                    if (!server.IsConnected) continue;

                    //  Anahtarları Listele: KeysAsync ile asenkron olarak akışa dahil etme
                    await foreach (var key in server.KeysAsync(database: 0, pattern: searchPattern))
                    {
                        keys.Add(key);
                    }
                }

                //  Silme İşlemi: Eğer listede anahtar varsa database üzerinden temizle
                if (keys.Any())
                {
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync(keys.ToArray());
                    _logger.LogInformation("{Count} adet anahtar prefix ile silindi: {Prefix}", keys.Count, prefix);
                }
                else
                {
                    _logger.LogDebug("Prefix ile eşleşen anahtar bulunamadı: {SearchPattern}", searchPattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Prefix silme hatası: {Prefix}", prefix);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            //  Parametre kontrolü: Boş pattern gönderilirse işlem yapma
            if (string.IsNullOrWhiteSpace(pattern)) return;

            //  Bağlantı kontrolü: Redis aktif değilse loglama ve çıkma
            if (!_isRedisAvailable || _redis == null)
            {
                _logger.LogWarning("Redis bağlantısı mevcut değil. Pattern silme atlandı: {Pattern}", pattern);
                return;
            }

            try
            {
                var keys = new List<RedisKey>();

                //  Prefix Birleştirme: IDistributedCache'in otomatik eklediği InstanceName'i  manuel ekliyoruz
                var fullPattern = _instanceName + pattern;

                //  Wildcard Kontrolü: Eğer pattern sonunda * yoksa, geniş kapsamlı silme için ekliyoruz
                var searchPattern = fullPattern.EndsWith("*") ? fullPattern : fullPattern + "*";

                //  Cluster/Node Gezintisi: Tüm Redis endpoint'lerini (master/slave) dönerek anahtarları arama
                foreach (var endpoint in _redis.GetEndPoints())
                {
                    var server = _redis.GetServer(endpoint);
                    if (!server.IsConnected) continue;

                    // Anahtarları Bul: Belirlenen database (0) üzerinden pattern'e uyan anahtarları asenkron getir
                    await foreach (var key in server.KeysAsync(database: 0, pattern: searchPattern, pageSize: 1000))
                    {
                        keys.Add(key);
                    }
                }

                //  Toplu Silme: Bulunan anahtar varsa Redis database'inden toplu olarak sil
                if (keys.Any())
                {
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync(keys.ToArray());

                    _logger.LogInformation("{Count} adet anahtar başarıyla silindi. (Pattern: {Pattern}, Aranan: {SearchPattern})",
                        keys.Count, pattern, searchPattern);
                }
                else
                {
                    // Debug seviyesinde log: Neden silinmediğini anlamak için prefixli hali gösteriyoruz
                    _logger.LogDebug("Pattern ile eşleşen anahtar bulunamadı: {SearchPattern}", searchPattern);
                }
            }
            catch (Exception ex)
            {
                //  Hata Yönetimi: İşlem sırasında oluşabilecek (network vb.) hataları logla
                _logger.LogError(ex, "Pattern silme işlemi sırasında hata oluştu: {Pattern}", pattern);
            }
        }
    }
}





