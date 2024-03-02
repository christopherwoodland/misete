namespace Misete.Helpers.Interfaces
{
    public interface IBlobHelper
    {
        Task<bool> DownloadFile(string imageUrl, string fileName, string containerName, string photoId);
        Task<bool> DownloadImagesFromDB(string connectionString, string tableName);
        Task<bool> DownloadImagesFromDBPostGres(string connectionString, string tableName);
        Task<bool> CreateBlobMetaDataFiles(string connectionString, string tableName);
        bool WriteToImportLog(string cs, string id, string imageUrl, string fullName);
    }
}