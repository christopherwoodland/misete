using Npgsql;
using System.Runtime.CompilerServices;

namespace Misete.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly ILogger _logger;
        private readonly IAppConfigurationHelper _appConfiguration;

        public BlobHelper(IAppConfigurationHelper appConfiguration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BlobHelper>();
            _appConfiguration = appConfiguration;
        }
        private async Task<bool> UploadImageToBlobStorage(byte[] imageBytes, string fileName, string containerName)
        {
            try
            {
                BlobServiceClient blobServiceClient = new(_appConfiguration.MISETE_STORAGE_ACCNT_PRIMARY_CONNECTION_STRING);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                using (var ms = new MemoryStream(imageBytes, false))
                {
                    await blobClient.UploadAsync(ms, true);
                }
                _logger.LogTrace($"UploadImageToBlobStorage: {fileName}");
                return true;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"UploadImageToBlobStorage: Error uploading image to Azure Blob Storage: {ex.Message}");
                return false;
            }
        }

        public bool WriteToImportLog(string cs, string id, string imageUrl, string fullName)
        {
            try
            {
                using var con = new NpgsqlConnection(cs);
                con.Open();

                using var cmd = new NpgsqlCommand();
                cmd.Connection = con;

                Guid guidValue;

                if (Guid.TryParse(id, out guidValue))
                {
                    // The string was successfully converted to a Guid.
                    _logger.LogTrace("Converted string to Guid: " + guidValue);
                }
                else
                {
                    // The string could not be converted to a Guid.
                    _logger.LogTrace("Invalid Guid format");
                    return false;
                }



                cmd.CommandText = "INSERT INTO importlog(id, imageurl, full_file_name) VALUES(@id, @imageurl, @full_file_name)";
                cmd.Parameters.AddWithValue("id", guidValue);
                cmd.Parameters.AddWithValue("imageurl", imageUrl);
                cmd.Parameters.AddWithValue("full_file_name", fullName);

                cmd.ExecuteNonQuery();

                _logger.LogTrace($"WriteToImportLog: {id}|{imageUrl}|{fullName}|");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"WriteToImportLog: Error on import log: {ex.Message}");
                return false;
            }


        }
        public async Task<bool> DownloadFile(string imageUrl, string fileName, string containerName, string photoId)
        {
            try
            {
                var httpClient = new HttpClient();
                byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                await UploadImageToBlobStorage(imageBytes, fileName, containerName);
                WriteToImportLog(_appConfiguration.MISETE_POSTGRES_DB_PRIMARY_CONNECTION_STRING, photoId, imageUrl, fileName);
                _logger.LogTrace($"DownloadFile: {imageUrl}:{fileName} Image Downloaded to Blob");
                return true;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"DownloadFile: Error downloading image: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DownloadImagesFromDB(string connectionString, string tableName)
        {
            try
            {   // SQL query to select all fields from the flatfile table
                string query = $"SELECT * FROM {tableName}";

                using SqlConnection connection = new(connectionString);

                try
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // Access fields by name or index
                        string fileName = reader["full_file_name"].ToString();
                        string linkOriginal = reader["link_original"].ToString();
                        string containerName = reader["container_name"].ToString();
                        string photoId = reader["photo_uuid"].ToString();
                        // Process retrieved data
                        await DownloadFile(linkOriginal, fileName, containerName, photoId);
                    }

                    reader.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DownloadImagesFromDB: Error downloading image: {ex.Message}");
                    return false;
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"DownloadImagesFromDB: Error downloading image: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DownloadImagesFromDBPostGres(string connectionString, string tableName)
        {
            try
            {
                using NpgsqlConnection con = new(connectionString);
                try
                {
                    con.Open();
                    string query = $"SELECT link_original, full_file_name, photo_uuid ,container_name" +
                        $" FROM flatfile ff " +
                        $"WHERE NOT EXISTS " +
                        $" (SELECT 1 " +
                        $"FROM importlog il " +
                        $"  WHERE il.id = ff.photo_uuid AND ff.full_file_name = il.full_file_name)" +
                        $"LIMIT 10;";
                    using var cmd = new NpgsqlCommand(query, con);
                    cmd.CommandTimeout = 3600;
                    using NpgsqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {

                        // Access fields by name or index
                        string fileName = reader["full_file_name"].ToString();
                        string linkOriginal = reader["link_original"].ToString();
                        string containerName = reader["container_name"].ToString();
                        string photoId= reader["photo_uuid"].ToString();
                        // Process retrieved data
                        await DownloadFile(linkOriginal, fileName, containerName, photoId);
                       
                    }

                    reader.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DownloadImagesFromDBPostGres: Error downloading image: {ex.Message}");
                    return false;
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"DownloadImagesFromDBPostGres: Error downloading image: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateBlobMetaDataFiles(string connectionString, string tableName)
        {

            try
            {
                using var con = new NpgsqlConnection(connectionString);
                con.Open();

                string sql = $"SELECT * FROM {tableName}";
                using var cmd = new NpgsqlCommand(sql, con);

                using NpgsqlDataReader rdr = cmd.ExecuteReader();

                var results = new List<Dictionary<string, object>>();
                var fileCount = 1;

                while (rdr.Read())
                {
                    var result = new Dictionary<string, object>();
                    for (int i = 0; i < rdr.FieldCount; i++)
                    {
                        result.Add(rdr.GetName(i), rdr.GetValue(i));
                    }

                    results.Add(result);

                    if (results.Count == 3000)
                    {
                        File.WriteAllText($"metadata{fileCount}.json", JsonConvert.SerializeObject(results));
                        results.Clear();
                        fileCount++;
                    }
                }

                if (results.Count > 0)
                {
                    File.WriteAllText($"metadata{fileCount}.json", JsonConvert.SerializeObject(results));
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateBlobMetaDataFiles: Error downloading image: {ex.Message}");
                return false;
            }
        }
    }
}
