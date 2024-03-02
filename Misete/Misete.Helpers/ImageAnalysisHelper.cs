

namespace Misete.Helpers
{
    public class ImageAnalysisHelper : IImageAnalysisHelper
    {
        private readonly ILogger _logger;
        private readonly IAppConfigurationHelper _appConfiguration;
        public ImageAnalysisHelper(IAppConfigurationHelper appConfiguration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ImageAnalysisHelper>();
            _appConfiguration = appConfiguration;
            //InitPython();
        }
        public IEnumerable<ImageAnalysisModel> GetImageDetails(byte[] imageBytes)
        {
            List<ImageAnalysisModel> ret = new();
            var directories = ImageMetadataReader.ReadMetadata(new MemoryStream(imageBytes));
            foreach (var directory in directories)
                foreach (var tag in directory.Tags)
                    ret.Add(new ImageAnalysisModel
                    {
                        DirectoryName = directory.Name,
                        TagName = tag.Name,
                        TagDescription = tag.Description
                    });

            return ret;
        }

        private static void InitPython()
        {
            if (String.IsNullOrWhiteSpace(Runtime.PythonDLL)) {
                Runtime.PythonDLL = $"python312.dll";
            }
            if (PythonEngine.IsInitialized.Equals(false))
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
        }

        public string GetContentAuth(byte[] image, string fileName)
        {
            //PYTHONNET_RUNTIME=coreclr
            //PYTHONNET_PYDLL=python312.dll
            var tempPath = $"{Path.GetTempPath()}";
            File.WriteAllBytes($"{tempPath}{fileName}", image);
           
            using (Py.GIL())
            {
                dynamic c2pa = Py.Import("c2pa");
                dynamic json_store = c2pa.read_file($"{tempPath}{fileName}", tempPath);
                _logger.LogInformation($"{json_store}");
                string ret = json_store;
                File.Delete($"{tempPath}{fileName}");
                return ret;

            }
        }
    }
}
