﻿using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNet;

using Microsoft.IdentityModel;
using Microsoft.Owin.Security;


//using Microsoft.AspNet.Builder;

//using Microsoft.AspNet.Http;

using Thinktecture.IdentityModel.Owin.ResourceAuthorization;


[assembly: OwinStartup(typeof(BI.UI.WebApp.Startup))]

namespace BI.UI.WebApp
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            //app.MapSignalR();

            //app.UseResourceAuthorization(new AuthorizationManager());
            ////
            ////app.UseInMemorySession(configure: s => s.IdleTimeout = TimeSpan.FromMinutes(30));
            /// 


            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

			// Challenges
			//https://stackoverflow.com/questions/31427066/set-challenge-in-owin-middleware
			app.Use((context, next) =>
			{
				if (context.Authentication.User != null &&
					context.Authentication.User.Identity != null &&
					context.Authentication.User.Identity.IsAuthenticated)
				{
					return next();
				}
				else
				{
					bool challenge = false;
					if (context.Request.Uri == new Uri("https://localhost:44305/default.aspx"))
						challenge = true;


					if (challenge)
						context.Authentication.Challenge(new AuthenticationProperties(), null);

				    return next(); //Task.FromResult(0);
				}
			});

			//https://pastebin.com/M2MX0u69
			app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = "webforms",
                Authority = "http://localhost:5000",
                RedirectUri = "https://localhost:44305/",
                SignInAsAuthenticationType = "Cookies",

                ResponseType = "id_token",
                Scope = "openid profile",

                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    MessageReceived = async n =>
                    {
                        //await SaveAccessToken(n.ProtocolMessage.AccessToken);
                        await DecodeAndWrite(n.ProtocolMessage.IdToken);
                        //await DecodeAndWrite(n.ProtocolMessage.AccessToken);

                    },
					AuthenticationFailed = (context) =>
					{
						if (context.Exception.Message.StartsWith("OICE_20004") || context.Exception.Message.Contains("IDX10311"))
						{
							context.SkipToNextMiddleware();
							//return Task.FromResult(0);
						}
						return Task.FromResult(0);
					}
				},



			});
			//app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
			//{
			//	ClientId = "webformshybrid",
			//	Authority = "https://localhost:44317/identity",
			//	RedirectUri = "https://localhost:44305/",
			//	SignInAsAuthenticationType = "Cookies",

			//	ResponseType = "code id_token token",
			//	Scope = "openid profile gallerymanagement",

			//	Notifications = new OpenIdConnectAuthenticationNotifications()
			//	{
			//		MessageReceived = async n =>
			//		{
			//			await SaveAccessToken(n.ProtocolMessage.AccessToken);
			//			await DecodeAndWrite(n.ProtocolMessage.IdToken);
			//			await DecodeAndWrite(n.ProtocolMessage.AccessToken);

			//		},
			//		AuthenticationFailed = (context) =>
			//		{
			//			if (context.Exception.Message.StartsWith("OICE_20004") || context.Exception.Message.Contains("IDX10311"))
			//			{
			//				context.SkipToNextMiddleware();
			//				//return Task.FromResult(0);
			//			}
			//			return Task.FromResult(0);
			//		}
			//	},



			//});

		}
        public static Task<string> DecodeAndWrite(string token)
        {
            //try
            //{
                var parts = token.Split('.');

                string partToConvert = parts[1];
                partToConvert = partToConvert.Replace('-', '+');
                partToConvert = partToConvert.Replace('_', '/');
                switch (partToConvert.Length % 4)
                {
                    case 0:
                        break;
                    case 2:
                        partToConvert += "==";
                        break;
                    case 3:
                        partToConvert += "=";
                        break;
                    default:
                        break;
                }

                var partAsBytes = Convert.FromBase64String(partToConvert);
                var partAsUTF8String = Encoding.UTF8.GetString(partAsBytes, 0, partAsBytes.Count());

                // Json .NET
                var jwt = JObject.Parse(partAsUTF8String);

                // Write to output
                Debug.Write(jwt.ToString());
				return Task<int>.Run(() =>
				{
                       return jwt.ToString();
                    });

            //}
            //catch (Exception ex)
            //{
            //    // something went wrong
            //    Debug.Write(ex.Message);

            //}
        }
        public static Task SaveAccessToken(string token)
        {
            HttpRuntime.Cache["AccessToken"] = token;

            //
			return Task.Run(() =>
			{
                Debug.Write("Storing the access token!");
            });


        }
    }







	// This is for the MVC stuff.  I'm not sure it's relevant for the webforms example.
    //public class AuthorizationManager : ResourceAuthorizationManager
    //{
    //    public override Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
    //    {
    //        switch (context.Resource.First().Value)
    //        {
    //            case "Caseload":
    //                return AuthorizeCaseloads(context);
    //            default:
    //                return Nok();
    //        }
    //    }



    //    //private Task<bool> AuthorizeCaseloads(ResourceAuthorizationContext context)
    //    //{

    //    //    switch (context.Action.First().Value)
    //    //    {
    //    //        case "Read":
    //    //            // to be able to read an expensegroups from the API, the user must be in the
    //    //            // WebReadUser role or MobileReadUser role
    //    //            return
    //    //                Eval(context.Principal.HasClaim("role", "MobileReadUser")
    //    //                || (context.Principal.HasClaim("role", "WebReadUser")));

    //    //        default:
    //    //            return Nok();
    //    //    }
    //    //}

    //}




}