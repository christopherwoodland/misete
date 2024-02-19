namespace Misete.Functions
{
    public class ImageAnalysis
    {
        private readonly ILogger _logger;
        private readonly IImageAnalysisHelper _iiah;
        private readonly IMapsHelper _imh;
        public ImageAnalysis(ILoggerFactory loggerFactory, IImageAnalysisHelper iiah, IMapsHelper imh)
        {
            _logger = loggerFactory.CreateLogger<ImageAnalysis>();
            _iiah = iiah;
            _imh = imh;
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
    }
}