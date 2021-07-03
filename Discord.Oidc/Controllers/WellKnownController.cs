using Microsoft.AspNetCore.Mvc;

namespace Discord.Oidc.Controllers
{
    [Route(".well-known")]
    public class WellKnownController
    {
        private readonly PkiService _pkiService;

        public WellKnownController(PkiService pkiService)
        {
            _pkiService = pkiService;
        }

        [HttpGet("jwks.json")]
        public string GetPublicKey()
        {
            return _pkiService.PublicKey;
        }
    }
}
