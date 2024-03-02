namespace Misete.Functions
{
    public class Images
    {
        private readonly ILogger _logger;
        private readonly IBlobHelper _bh;
        private readonly IImageAnalysisHelper _iiah;
        private readonly IMapsHelper _imh;
        private readonly IAppConfigurationHelper _appConfiguration;
        public Images(ILoggerFactory loggerFactory, IBlobHelper bh, IImageAnalysisHelper iiah, IMapsHelper imh, 
            IAppConfigurationHelper appConfiguration)
        {
            _logger = loggerFactory.CreateLogger<Images>();
            _bh = bh;
            _iiah = iiah;
            _imh = imh;
            _appConfiguration = appConfiguration;
        }

        [Function("GetImageDetails")]
        public async Task<IActionResult> GetImageDetails([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("GetImageDetails: C# HTTP trigger function processed a request. ");
            try
            {
                IFormFile file = req.Form.Files["file"] ?? throw new 
                    PostedFileException("File posted to method either corrupt or missing. Check that the file was posted properly.");
                if (file == null)
                {
                    return new BadRequestObjectResult("GetImageDetails: Please upload a file.");
                }

                // Read the image data
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                var ret = _iiah.GetImageDetails(memoryStream.ToArray());
                _logger.LogInformation($"GetImageDetails Returns: {ret}");
                return new OkObjectResult(ret);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetImageDetails Returns: {ex.Message}");
                return new BadRequestObjectResult(ex);
            }
        }

        [Function("GetImageLocation")]
        public async Task<IActionResult> GetImageLocation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("GetImageLocation: C# HTTP trigger function processed a request. ");
            try
            {
                IFormFile file = req.Form.Files["file"] ?? throw new PostedFileException("File posted to method either corrupt or missing. Check that the file was posted properly.");
                if (file == null)
                {
                    return new BadRequestObjectResult("GetImageLocation: Please upload a file.");
                }
                // Read the image data
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                var imageDetails = _iiah.GetImageDetails(memoryStream.ToArray());
                _logger.LogInformation($"GetImageLocation Returns: {imageDetails}");
                var location = _imh.GetLocationAsync(imageDetails).Result;
                _logger.LogInformation($"GetImageLocation Returns: {location}");
                return new OkObjectResult(location);
            }
            catch(Exception ex)
            {
                _logger.LogError($"GetImageLocation Returns: {ex.Message}");
                return new BadRequestObjectResult(ex);
            }
        }
        [Function("GetContentAuth")]
        public async Task<IActionResult> GetContentAuth([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("GetContentAuth: C# HTTP trigger function processed a request. ");
            try
            {
                IFormFile file = req.Form.Files["file"] ?? throw new
                    PostedFileException("File posted to method either corrupt or missing. Check that the file was posted properly.");
                if (file == null)
                {
                    return new BadRequestObjectResult("GetContentAuth: Please upload a file.");
                }
                // Read the image data
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                var ret = _iiah.GetContentAuth(memoryStream.ToArray(), file.FileName);
                _logger.LogInformation($"GetContentAuth Returns: {ret}");
                return new OkObjectResult(ret);
            }
            catch(Exception ex)
            {
                if (ex.Message.Equals("ManifestNotFound no JUMBF data found"))
                {
                    return new BadRequestObjectResult($"NO_META_DATA_FOUND");
                }
                else
                {
                    _logger.LogError($"GetContentAuth Returns: {ex.Message}");
                    return new BadRequestObjectResult(ex);
                }
            }
        }
        
        [Function("DownloadImage")]
        public Task<IActionResult> DownloadImage([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("DownloadImage: C# HTTP trigger function processed a request.");
            try
            {
                string fileName = req.Query["fileName"];
                string containerName = req.Query["containerName"];
                string imageUrl = req.Query["imageUrl"];
                string photoId = req.Query["photoId"];
                if (String.IsNullOrEmpty(fileName) || String.IsNullOrEmpty(containerName) || String.IsNullOrEmpty(imageUrl))
                    return Task.FromResult<IActionResult>(new BadRequestObjectResult($"GetImageDetails: fileName|containerName|imageUrl is null or empty."));

                Task<bool> ret = _bh.DownloadFile(imageUrl, fileName, containerName, photoId);
                _logger.LogInformation($"DownloadImage Returns: {ret}");
                return Task.FromResult<IActionResult>(new OkObjectResult(ret.Result));
            }
            catch (Exception ex)
            {
                _logger.LogError($"DownloadImage Returns: {ex.Message}");
                return Task.FromResult<IActionResult>(new BadRequestObjectResult(ex));
            }
        }


        [Function("DownloadImagesFromDB")]
        public async Task<IActionResult> DownloadImagesFromDB([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("DownloadImagesFromDB: C# HTTP trigger function processed a request.");
            try
            {
                string tableName = req.Query["tableName"];
           
                if (String.IsNullOrEmpty(tableName))
                    return new BadRequestObjectResult($"DownloadImagesFromDB: tableName is null or empty.");
                var cs = _appConfiguration.MISETE_STORAGE_ACCNT_PRIMARY_CONNECTION_STRING;
                Task<bool> ret = _bh.DownloadImagesFromDB(cs, tableName);
                _logger.LogInformation($"DownloadImagesFromDB Returns: {ret}");
                return new OkObjectResult(ret.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DownloadImagesFromDB Returns: {ex.Message}");
                return new BadRequestObjectResult(ex);
            }
        }

        [Function("DownloadImagesFromDBPostGres")]
        public async Task<IActionResult> DownloadImagesFromDBPostGres([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("DownloadImagesFromDBPostGres: C# HTTP trigger function processed a request.");
            try
            {
                string tableName = req.Query["tableName"];

                if (String.IsNullOrEmpty(tableName))
                    return new BadRequestObjectResult($"DownloadImagesFromDBPostGres: tableName is null or empty.");
                var cs = _appConfiguration.MISETE_POSTGRES_DB_PRIMARY_CONNECTION_STRING;
                Task<bool> ret = _bh.DownloadImagesFromDBPostGres(cs, tableName);
                

                _logger.LogInformation($"DownloadImagesFromDBPostGres Returns: {ret}");
                return new OkObjectResult(ret.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DownloadImagesFromDBPostGres Returns: {ex.Message}");
                return new BadRequestObjectResult(ex);
            }
        }

    }
}