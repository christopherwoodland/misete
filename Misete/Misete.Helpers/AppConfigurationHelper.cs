namespace Misete.Helpers
{
    public class AppConfigurationHelper: IAppConfigurationHelper
    {

        private readonly IConfiguration _configuration;
        private readonly IConfigurationRefresher _configurationRefresher;
        private readonly ILogger _logger;

        public AppConfigurationHelper(IConfiguration configuration,
                IConfigurationRefresherProvider configurationRefresher, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AppConfigurationHelper>();
            _configuration = configuration;
            try
            {
                _configurationRefresher = configurationRefresher.Refreshers.First();
            }
            catch {
                //DO NOTHING
            }
        }

        #region Properties
        public string FR_KEY => _configuration["DOCINTELL:FR_KEY"];
        public string FR_ENDPOINT => _configuration["DOCINTELL:FR_ENDPOINT"];
        public string WEE_MAIN_DB_AUTH_KEY => _configuration["COSMOSDB:WEE_MAIN_DB_AUTH_KEY"];
        public string WEE_MAIN_DB_ENDPOINT_URL => _configuration["COSMOSDB:WEE_MAIN_DB_ENDPOINT_URL"];
        public string OAI_END_POINT => _configuration["OPENAI:OAI_END_POINT"];
        public string OAI_KEY => _configuration["OPENAI:OAI_KEY"];
        public int OAI_MAX_TOKENS => Convert.ToInt32(_configuration["OPENAI:MAX_TOKENS"]);
        public string OAI_EMBD_MODEL => _configuration["OPENAI:OAI_EMBEDDINGS_DEPLOYMENT_NAME"];
        public string COGSERVICE_SEARCH_KEY => _configuration["COGSERVICE:SEARCH_KEY"];
        public string COGSERVICE_ENDPOINT_URL => _configuration["COGSERVICE:ENDPOINT_URL"];
        public string CUSTOMVISION_ENDPOINT_URL => _configuration["CUSTOMVISION:ENDPOINT"];
        public string CUSTOMVISION_KEY => _configuration["CUSTOMVISION:KEY"];
        public string CUSTOMVISION_REGION => _configuration["CUSTOMVISION:REGION"];
        public string AZUREMAPS_AUTH_KEY => _configuration["AZUREMAPS:AUTH_KEY"];
        public string AZUREMAPS_CLIENT_ID => _configuration["AZUREMAPS:CLIENT_ID"];
        public string AZUREMAPS_BASE_URL => _configuration["AZUREMAPS:BASE_URL"];

        public string MISETE_STORAGE_ACCNT_PRIMARY_KEY => _configuration["MISETE:STORAGE_ACCNT_PRIMARY_KEY"];

        public string MISETE_STORAGE_ACCNT_PRIMARY_NAME => _configuration["MISETE:STORAGE_ACCNT_PRIMARY_NAME"];

        public string MISETE_STORAGE_ACCNT_PRIMARY_CONNECTION_STRING => _configuration["MISETE:STORAGE_ACCNT_PRIMARY_CONNECTION_STRING"];

        public string MISETE_SQL_DB_PRIMARY_CONNECTION_STRING =>_configuration["MISETE:SQL_DB_PRIMARY_CONNECTION_STRING"]; 
        public string MISETE_POSTGRES_DB_PRIMARY_CONNECTION_STRING => _configuration["MISETE:POSTGRES_DB_PRIMARY_CONNECTION_STRING"];

        #endregion
        public async Task<bool> RefreshConfiguration()
        {
            _logger.LogInformation($"RefreshConfiguration Called {DateTime.UtcNow}");
            return await _configurationRefresher.TryRefreshAsync().ConfigureAwait(false);
        }
        public IConfiguration GetConfiguration()
        {
            return _configuration;
        }
    }
}
