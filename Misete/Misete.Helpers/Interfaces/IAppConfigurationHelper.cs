namespace Misete.Helpers.Interfaces
{
    public interface IAppConfigurationHelper
    {
        public string FR_KEY { get; }
        public string FR_ENDPOINT { get; }
        public string WEE_MAIN_DB_AUTH_KEY { get; }
        public string WEE_MAIN_DB_ENDPOINT_URL { get; }
        public string OAI_END_POINT { get; }
        public string OAI_KEY { get; }
        public int OAI_MAX_TOKENS { get; }
        public string OAI_EMBD_MODEL { get; }
        public string COGSERVICE_SEARCH_KEY { get; }
        public string COGSERVICE_ENDPOINT_URL { get; }
        public string CUSTOMVISION_ENDPOINT_URL { get; }
        public string CUSTOMVISION_KEY { get; }
        public string CUSTOMVISION_REGION { get; }
        public string AZUREMAPS_AUTH_KEY { get; }
        public string AZUREMAPS_CLIENT_ID { get; }
        public string AZUREMAPS_BASE_URL { get; }
        public string MISETE_STORAGE_ACCNT_PRIMARY_KEY   { get; }
        public string MISETE_STORAGE_ACCNT_PRIMARY_NAME { get; }
        public string MISETE_STORAGE_ACCNT_PRIMARY_CONNECTION_STRING  { get;}
        public string MISETE_POSTGRES_DB_PRIMARY_CONNECTION_STRING { get; }
        public string MISETE_SQL_DB_PRIMARY_CONNECTION_STRING { get; }
        public Task<bool> RefreshConfiguration();
        public IConfiguration GetConfiguration();
    }
}
