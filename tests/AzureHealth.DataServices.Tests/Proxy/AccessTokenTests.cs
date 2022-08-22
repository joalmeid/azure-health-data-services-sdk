﻿using System.Reflection;
using System.Threading.Tasks;
using Azure.Identity;
using AzureHealth.DataServices.Configuration;
using AzureHealth.DataServices.Security;
using AzureHealth.DataServices.Tests.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureHealth.DataServices.Tests.Proxy
{
    [TestClass]
    public class AccessTokenTests
    {
        private static ServiceIdentityConfig config;
        public AccessTokenTests()
        {

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables("PROXY_");
            IConfigurationRoot root = builder.Build();
            config = new ServiceIdentityConfig();
            root.Bind(config);
            System.Environment.SetEnvironmentVariable("AZURE_TENANT_ID", config.TenantId);
            System.Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", config.ClientId);
            System.Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", config.ClientSecret);
        }


        [TestMethod]
        public async Task AccessToken_UsingDefaultCredential_Test()
        {
            string resource = "https://localhost";
            IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new());
            Authenticator auth = new(options);
            string token = await auth.AcquireTokenForClientAsync(resource);
            Assert.IsNotNull(token, "Security token must not be null.");
        }

        [TestMethod]
        public async Task AccessToken_Acquisition_Test()
        {
            string resource = "https://localhost";
            IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new()
            {
                Certficate = config.Certficate,
                CredentialType = ClientCredentialType.ClientSecret,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                TenantId = config.TenantId,
            });
            Authenticator auth = new(options);
            ClientSecretCredential credential = new(config.TenantId, config.ClientId, config.ClientSecret);
            string token = await auth.AcquireTokenForClientAsync(resource, credential);
            Assert.IsNotNull(token, "Security token must not be null.");
        }

        [TestMethod]
        public async Task AccessToken_AcquisitionAuto_Test()
        {
            string resource = "https://localhost";
            IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new()
            {
                Certficate = config.Certficate,
                CredentialType = ClientCredentialType.ClientSecret,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                TenantId = config.TenantId,
            });
            Authenticator auth = new(options);
            string token = await auth.AcquireTokenForClientAsync(resource);
            Assert.IsNotNull(token, "Security token must not be null.");
        }

        [TestMethod]
        public async Task AccessToken_Acquisition_Generic_Test()
        {
            string resource = "https://localhost";
            string[] scopes = new string[] { "https://localhost/.default" };
            IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new()
            {
                Certficate = config.Certficate,
                CredentialType = ClientCredentialType.ClientSecret,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                TenantId = config.TenantId,
            });
            Authenticator auth = new(options);
            ClientSecretCredential credential = new(config.TenantId, config.ClientId, config.ClientSecret);
            string token = await auth.AcquireTokenForClientAsync(resource, credential, scopes);
            Assert.IsNotNull(token, "Security token must not be null.");
        }

        [TestMethod]
        public async Task AccessToken_Acquisition_Auto_Test()
        {
            string resource = "https://localhost";
            string[] scopes = new string[] { "https://localhost/.default" };
            IOptions<ServiceIdentityOptions> options = Options.Create<ServiceIdentityOptions>(new()
            {
                Certficate = config.Certficate,
                CredentialType = ClientCredentialType.ClientSecret,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                TenantId = config.TenantId,
            });
            Authenticator auth = new(options);
            string token = await auth.AcquireTokenForClientAsync(resource, scopes);
            Assert.IsNotNull(token, "Security token must not be null.");
        }

        [TestMethod]
        public async Task AccessToken_DefaultCredentialsExtension_Test()
        {
            string resource = "https://localhost";
            ServiceCollection services = new ServiceCollection();
            services.UseAuthenticator();
            ServiceProvider prov = services.BuildServiceProvider();
            IAuthenticator authn = (IAuthenticator)prov.GetService(typeof(IAuthenticator));
            string token = await authn.AcquireTokenForClientAsync(resource);
            Assert.IsNotNull(token, "access token is null.");
        }
    }
}
