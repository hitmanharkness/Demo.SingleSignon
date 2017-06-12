using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Demo.IdentityServer.Config;

namespace Demo.IdentityServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/identity", idsrvApp =>
            {
                var corsPolicyService = new DefaultCorsPolicyService()
                {
                    AllowAll = true
                };
                
                var defaultViewServiceOptions = new DefaultViewServiceOptions();
                defaultViewServiceOptions.CacheViews = false;
                
                var idServerServiceFactory = new IdentityServerServiceFactory()
                                .UseInMemoryClients(Clients.Get())
                                .UseInMemoryScopes(Scopes.Get())
                                .UseInMemoryUsers(Users.Get());

                idServerServiceFactory.CorsPolicyService = new
                    Registration<IdentityServer3.Core.Services.ICorsPolicyService>(corsPolicyService);

                idServerServiceFactory.ConfigureDefaultViewService(defaultViewServiceOptions);

                var options = new IdentityServerOptions
                {
                    Factory = idServerServiceFactory,
                    SiteName = "Security Token Service",
                    SigningCertificate = LoadCertificate(),
                    IssuerUri = TripGallery.Constants.IssuerUri,
                    PublicOrigin = TripGallery.Constants.STSOrigin,
                    AuthenticationOptions = new AuthenticationOptions()
                    {
                        EnablePostSignOutAutoRedirect = true
                    },
                    CspOptions = new CspOptions()
                    {
                         Enabled = false                        
                         // once available, leave Enabled at true and use:
                         // FrameSrc = "https://localhost:44318 https://localhost:44316"
                         // or
                         // FrameSrc = "*" for all URI's.
                    }
                };

                idsrvApp.UseIdentityServer(options);
            });
        }


        X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2(
                string.Format(@"{0}\certificates\idsrv3test.pfx",
                AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }
    }
}
