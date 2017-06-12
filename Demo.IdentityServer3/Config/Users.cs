using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Demo.IdentityServer.Config
{
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>() {
                 
                new InMemoryUser
	            {
	                Username = "dev",
	                Password = "secret",                    
	                Subject = "b05d3546-6ca8-4d32-b95c-77e94d705ddf",
                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Joe"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.Address, "1824 17th St #204 Boulder CO"),
                        new Claim("role", "Admin")
                    }
	             }
            };
        }
    }

}
