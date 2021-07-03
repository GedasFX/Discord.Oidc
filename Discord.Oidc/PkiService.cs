using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace Discord.Oidc
{
    public class PkiService
    {
        private readonly JsonWebKey _jwk;
        private string? _publicKey;

        public PkiService(string? keyFile = null)
        {
            keyFile ??= Path.Combine(Directory.GetCurrentDirectory(), "pki.jwk");

            if (File.Exists(keyFile))
            {
                var json = File.ReadAllText(keyFile);
                _jwk = new JsonWebKey(json);
            }
            else
            {
                var key = new RsaSecurityKey(RSA.Create(2048))
                {
                    KeyId = CryptoRandom.CreateUniqueId(16, CryptoRandom.OutputFormat.Hex)
                };
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
                jwk.Alg = SecurityAlgorithms.RsaSha256;

                File.WriteAllText(keyFile, JsonSerializer.Serialize(jwk));

                _jwk = jwk;
            }
        }

        public SigningCredentials SigningCredentials => new(_jwk, _jwk.Alg);

        public string PublicKey
        {
            get
            {
                if (_publicKey != null)
                    return _publicKey;

                return _publicKey = JsonSerializer.Serialize(new
                {
                    _jwk.Alg,
                    _jwk.Kid,
                    _jwk.Kty,
                    _jwk.E,
                    _jwk.N,
                }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
        }
    }
}
