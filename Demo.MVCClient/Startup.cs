using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using IdentityModel.Client;
using TripGallery.MVCClient.Helpers;
using Microsoft.IdentityModel.Protocols;


[assembly: OwinStartup(typeof(TripGallery.MVCClient.Startup))]
namespace TripGallery.MVCClient
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = 
                IdentityModel.JwtClaimTypes.Name;

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                ExpireTimeSpan = new TimeSpan(0, 30, 0),
                SlidingExpiration = true
            });
			app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
			{

				ClientId = "mvc.hybrid",
				ClientSecret = "secret",
				Authority = "http://localhost:5000",
				RedirectUri = Constants.MVC,
				SignInAsAuthenticationType = "Cookies",
				ResponseType = "code id_token token",
				//Scope = "openid profile address gallerymanagement roles offline_access",
				Scope = "openid profile email api1 offline_access",
				UseTokenLifetime = false,
				PostLogoutRedirectUri = Constants.MVC,

				Notifications = new OpenIdConnectAuthenticationNotifications()
				{
					SecurityTokenValidated = async n =>
					{
						Helpers.TokenHelper.DecodeAndWrite(n.ProtocolMessage.IdToken);
						Helpers.TokenHelper.DecodeAndWrite(n.ProtocolMessage.AccessToken);

						var givenNameClaim = n.AuthenticationTicket
							.Identity.FindFirst(IdentityModel.JwtClaimTypes.GivenName);

						var familyNameClaim = n.AuthenticationTicket
							.Identity.FindFirst(IdentityModel.JwtClaimTypes.FamilyName);

						var subClaim = n.AuthenticationTicket
							.Identity.FindFirst(IdentityModel.JwtClaimTypes.Subject);

						var roleClaim = n.AuthenticationTicket
							.Identity.FindFirst(IdentityModel.JwtClaimTypes.Role);

						// create a new claims, issuer + sub as unique identifier
						var nameClaim = new Claim(IdentityModel.JwtClaimTypes.Name,
									Constants.IssuerUri + subClaim.Value);

						var newClaimsIdentity = new ClaimsIdentity(
						   n.AuthenticationTicket.Identity.AuthenticationType,
						   IdentityModel.JwtClaimTypes.Name,
						   IdentityModel.JwtClaimTypes.Role);

						if (nameClaim != null)
						{
							newClaimsIdentity.AddClaim(nameClaim);
						}

						if (givenNameClaim != null)
						{
							newClaimsIdentity.AddClaim(givenNameClaim);
						}

						if (familyNameClaim != null)
						{
							newClaimsIdentity.AddClaim(familyNameClaim);
						}

						if (roleClaim != null)
						{
							newClaimsIdentity.AddClaim(roleClaim);
						}

						// request a refresh token
						var tokenClientForRefreshToken = new TokenClient(
								   Constants.TripGallerySTSTokenEndpoint,
								   "tripgalleryhybrid",
								   Constants.ClientSecret);

						var refreshResponse = await
							tokenClientForRefreshToken.RequestAuthorizationCodeAsync(
							n.ProtocolMessage.Code,
							Constants.MVC);

						var expirationDateAsRoundtripString
							= DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(refreshResponse.ExpiresIn)
							, DateTimeKind.Utc).ToString("o");


						newClaimsIdentity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
						newClaimsIdentity.AddClaim(new Claim("refresh_token", refreshResponse.RefreshToken));
						newClaimsIdentity.AddClaim(new Claim("access_token", refreshResponse.AccessToken));
						newClaimsIdentity.AddClaim(new Claim("expires_at", expirationDateAsRoundtripString));

						// create a new authentication ticket, overwriting the old one.
						n.AuthenticationTicket = new AuthenticationTicket(
												 newClaimsIdentity,
												 n.AuthenticationTicket.Properties);
					},
					RedirectToIdentityProvider = async n =>
					{
						// get id token to add as id token hint
						if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
						{
							var identityTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

							if (identityTokenHint != null)
							{
								n.ProtocolMessage.IdTokenHint = identityTokenHint.Value;
							}

						}
					}
				}
			});
			//app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
   //         {

   //             ClientId = "tripgalleryhybrid",
   //             Authority = Constants.TripGallerySTS,
   //             RedirectUri = Constants.MVC,
   //             SignInAsAuthenticationType = "Cookies",
   //             ResponseType = "code id_token token",
   //             Scope = "openid profile address gallerymanagement roles offline_access",
   //             UseTokenLifetime = false,
   //             PostLogoutRedirectUri = Constants.MVC,

   //             Notifications = new OpenIdConnectAuthenticationNotifications()
   //             {
   //                 SecurityTokenValidated = async n =>
   //                 {
   //                     Helpers.TokenHelper.DecodeAndWrite(n.ProtocolMessage.IdToken);
   //                     Helpers.TokenHelper.DecodeAndWrite(n.ProtocolMessage.AccessToken);

   //                     var givenNameClaim = n.AuthenticationTicket
   //                         .Identity.FindFirst(IdentityModel.JwtClaimTypes.GivenName);

   //                     var familyNameClaim = n.AuthenticationTicket
   //                         .Identity.FindFirst(IdentityModel.JwtClaimTypes.FamilyName);

   //                     var subClaim = n.AuthenticationTicket
   //                         .Identity.FindFirst(IdentityModel.JwtClaimTypes.Subject);

   //                     var roleClaim = n.AuthenticationTicket
   //                         .Identity.FindFirst(IdentityModel.JwtClaimTypes.Role);

   //                     // create a new claims, issuer + sub as unique identifier
   //                     var nameClaim = new Claim(IdentityModel.JwtClaimTypes.Name,
   //                                 Constants.IssuerUri + subClaim.Value);

   //                     var newClaimsIdentity = new ClaimsIdentity(
   //                        n.AuthenticationTicket.Identity.AuthenticationType,
   //                        IdentityModel.JwtClaimTypes.Name,
   //                        IdentityModel.JwtClaimTypes.Role);

   //                     if (nameClaim != null)
   //                     {
   //                         newClaimsIdentity.AddClaim(nameClaim);
   //                     }

   //                     if (givenNameClaim != null)
   //                     {
   //                         newClaimsIdentity.AddClaim(givenNameClaim);
   //                     }

   //                     if (familyNameClaim != null)
   //                     {
   //                         newClaimsIdentity.AddClaim(familyNameClaim);
   //                     }

   //                     if (roleClaim != null)
   //                     {
   //                         newClaimsIdentity.AddClaim(roleClaim);
   //                     }

   //                     // request a refresh token
   //                     var tokenClientForRefreshToken = new TokenClient(
   //                                Constants.TripGallerySTSTokenEndpoint,
   //                                "tripgalleryhybrid",
   //                                Constants.ClientSecret);

   //                     var refreshResponse = await
   //                         tokenClientForRefreshToken.RequestAuthorizationCodeAsync(
   //                         n.ProtocolMessage.Code,
   //                         Constants.MVC);

   //                     var expirationDateAsRoundtripString
   //                         = DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(refreshResponse.ExpiresIn)
   //                         , DateTimeKind.Utc).ToString("o");


   //                     newClaimsIdentity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
   //                     newClaimsIdentity.AddClaim(new Claim("refresh_token", refreshResponse.RefreshToken));
   //                     newClaimsIdentity.AddClaim(new Claim("access_token", refreshResponse.AccessToken));
   //                     newClaimsIdentity.AddClaim(new Claim("expires_at", expirationDateAsRoundtripString));

   //                     // create a new authentication ticket, overwriting the old one.
   //                     n.AuthenticationTicket = new AuthenticationTicket(
   //                                              newClaimsIdentity,
   //                                              n.AuthenticationTicket.Properties);
   //                 },
   //                 RedirectToIdentityProvider = async n =>
   //                 {
   //                     // get id token to add as id token hint
   //                     if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
   //                     {
   //                         var identityTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

   //                         if (identityTokenHint != null)
   //                         {
   //                             n.ProtocolMessage.IdTokenHint = identityTokenHint.Value;
   //                         }

   //                     }
   //                 }
   //             }
   //         });
        }
    }
}
