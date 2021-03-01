using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    static class StaticPKCE
    {
        static readonly string _codeVerifier;
        static readonly string _codeChallenge;
        
        static StaticPKCE()
        {
            var rand = "ngchKjMcYM4Az17wBSAknBZ8IzpWYZoU"; // use static value, for sample only

            var randToBytes = Encoding.ASCII.GetBytes(rand);

            _codeVerifier = WebEncoders.Base64UrlEncode(randToBytes);

            var code_verifier_sha256 = sha256(_codeVerifier);

            _codeChallenge = WebEncoders.Base64UrlEncode(code_verifier_sha256);
        }

        static byte[] sha256(string value)
        {
            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(Encoding.ASCII.GetBytes(value));

                return result;
            }
        }

        public static string CodeVerifier => _codeVerifier;

        public static string CodeChallenge => _codeChallenge;
    }
}
