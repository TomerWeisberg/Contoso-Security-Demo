using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ContosoClinic.Startup))]
namespace ContosoClinic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            InitializeAzureKeyVaultProvider();
        }

        private static Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential _clientCredential;

        static void InitializeAzureKeyVaultProvider()
        {
            //TODO: Determine the exact name of environment variable created by ARM template. change the following two lines accordingly. 
            string clientId = System.Environment.GetEnvironmentVariable("applicationADID");
            //System.Configuration.ConfigurationManager.AppSettings["AuthClientId"];
            string clientSecret = System.Environment.GetEnvironmentVariable("applicationADSecret");
                //System.Configuration.ConfigurationManager.AppSettings["applicationADSecret"];
            //Change the above lines to get the ID and Secret set correctly. 

            _clientCredential = new Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential(clientId, clientSecret);

            Microsoft.SqlServer.Management.AlwaysEncrypted.AzureKeyVaultProvider.SqlColumnEncryptionAzureKeyVaultProvider azureKeyVaultProvider =
              new Microsoft.SqlServer.Management.AlwaysEncrypted.AzureKeyVaultProvider.SqlColumnEncryptionAzureKeyVaultProvider(GetToken);

            System.Collections.Generic.Dictionary<string, System.Data.SqlClient.SqlColumnEncryptionKeyStoreProvider> providers =
              new System.Collections.Generic.Dictionary<string, System.Data.SqlClient.SqlColumnEncryptionKeyStoreProvider>();

            providers.Add(Microsoft.SqlServer.Management.AlwaysEncrypted.AzureKeyVaultProvider.SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, azureKeyVaultProvider);
            System.Data.SqlClient.SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);
        }

        public async static System.Threading.Tasks.Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(authority);
            Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult result = await authContext.AcquireTokenAsync(resource, _clientCredential);

            if (result == null)
                throw new System.InvalidOperationException("Failed to obtain the access token");

            return result.AccessToken;
        }
    }
}
