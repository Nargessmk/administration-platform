using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Cloudland.APM.Microservices.AppPermissionManagement.Extensions;

public static class RedisExtensions
{
    public static bool CheckServerCertificate(object sender, X509Certificate certificate,
        X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // if (sslPolicyErrors != SslPolicyErrors.RemoteCertificateChainErrors) return false;
        var ca = X509CertificateLoader.LoadCertificateFromFile("certs/cert.pem");
        var caFound = chain.ChainElements
            .Any(x => x.Certificate.Thumbprint == ca.Thumbprint);
        return caFound;
    }
}