using Microsoft.Extensions.Configuration;

namespace TiendaPromElec.Helpers
{
    public static class JwtHelper
    {
        public static string GetJwtKey(IConfiguration configuration)
        {
            var key = configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                // Fallback key for consistency
                return "EstaEsUnaClaveSuperSecretaParaElRetoDelTec2026!";
            }
            return key;
        }
    }
}
