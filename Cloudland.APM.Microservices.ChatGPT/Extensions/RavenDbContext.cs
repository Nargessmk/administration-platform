using System.Security.Cryptography.X509Certificates;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.ChatGPT.Extensions;

public class RavenDbContext
{
    public RavenDbContext(IConfiguration configuration)
    {
        var url = configuration["RavenDb:Url"];
        var database = configuration["RavenDb:DatabaseName"];
        var certPath = configuration["RavenDb:CertPath"];
        var certPassword = configuration["RavenDb:CertPassword"];

        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(database) ||
            string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(certPassword))
            throw new ArgumentException("RavenDb configuration is incomplete.");

        // بارگذاری گواهی با استفاده از X509CertificateLoader
        var cert = X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword);

        Store = new DocumentStore
        {
            Urls = [url],
            Database = database,
            Certificate = cert
        }.Initialize();
    }

    private IDocumentStore Store { get; }

    public IAsyncDocumentSession OpenSession()
    {
        return Store.OpenAsyncSession();
    }
}