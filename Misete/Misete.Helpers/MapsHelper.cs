namespace Misete.Helpers
{
    public class MapsHelper : IMapsHelper
    {
        private readonly ILogger _logger;
        private readonly IAppConfigurationHelper _appConfiguration;
        private readonly string GPS_LONG_TAG_NAME = "GPS Longitude";
        private readonly string GPS_LAT_TAG_NAME = "GPS Latitude";
        private readonly string GPS_LAT_REF = "GPS Latitude Ref";
        private readonly string GPS_LG_REF = "GPS Longitude Ref";
        private readonly string GPS_LONG_DIRECTORY_NAME = "GPS";
        public MapsHelper(IAppConfigurationHelper appConfiguration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ImageAnalysisHelper>();
            _appConfiguration = appConfiguration;
        }
        private string GetLatLng(IEnumerable<ImageAnalysisModel> items)
        {
            try
            {
                if (items == null || !items.Any())
                {
                    return "";
                }
                var itsLat = items.Where(x => x.TagName.Equals(GPS_LAT_TAG_NAME, StringComparison.Ordinal)).FirstOrDefault(x => x.DirectoryName.Equals(GPS_LONG_DIRECTORY_NAME, StringComparison.Ordinal)).TagDescription.Replace("°", "").Replace("'", "").Replace("\"", "").Split(' ');
                var gpsRefLat = items.Where(x => x.TagName.Equals(GPS_LAT_REF, StringComparison.Ordinal)).FirstOrDefault(x => x.DirectoryName.Equals(GPS_LONG_DIRECTORY_NAME, StringComparison.Ordinal)).TagDescription;
                var itsLng = items.Where(x => x.TagName.Equals(GPS_LONG_TAG_NAME, StringComparison.Ordinal)).FirstOrDefault(x => x.DirectoryName.Equals(GPS_LONG_DIRECTORY_NAME, StringComparison.Ordinal)).TagDescription.Replace("°", "").Replace("'", "").Replace("\"", "").Split(' ');
                var gpsRefLng = items.Where(x => x.TagName.Equals(GPS_LG_REF, StringComparison.Ordinal)).FirstOrDefault(x => x.DirectoryName.Equals(GPS_LONG_DIRECTORY_NAME, StringComparison.Ordinal)).TagDescription;
                if (itsLat.Length > 0 && itsLat.Length == 3 && itsLng.Length > 0 && itsLng.Length == 3)
                {
                    //DMS Formatted: N 40º 34' 36.552" W 70º 45' 24.408.
                    Coordinate c = new Coordinate();
                    CoordinatesPosition cpLat, cpLng;
                    cpLat = gpsRefLat.ToLower().Equals("s") ? CoordinatesPosition.S : CoordinatesPosition.N;
                    cpLng = gpsRefLng.ToLower().Equals("w") ? CoordinatesPosition.W : CoordinatesPosition.E;
                    c.Latitude = new CoordinatePart(Math.Abs(Convert.ToInt16(itsLat[0])), Math.Abs(Convert.ToInt16(itsLat[1])), Convert.ToDouble(itsLat[2]), cpLat);
                    c.Longitude = new CoordinatePart(Math.Abs(Convert.ToInt16(itsLng[0])), Math.Abs(Convert.ToInt16(itsLng[1])), Convert.ToDouble(itsLng[2]), cpLng);
                    return $"{c.Latitude.ToDouble()},{c.Longitude.ToDouble()}"; // Returns 40.57682  (Signed Degree)

                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLatLng: {ex.Message}");
                return "";
            }
        }
        public async Task<AzureMapsReverseSearchModel> GetMapLocationAsync(string ltlg)
        {
            var url = _appConfiguration.AZUREMAPS_BASE_URL + $"search/address/reverse/json?api-version=1.0&subscription-key={_appConfiguration.AZUREMAPS_AUTH_KEY}&language=en-US&query={ltlg.Replace("\"", "")}&number=1";
            _logger.LogInformation($"Map URL: {url}");
            AzureMapsReverseSearchModel responseBody = new();
            using var httpClient = new HttpClient();
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var ret = await response.Content.ReadAsStringAsync();
                _logger.LogTrace(message: ret);
                responseBody = JsonConvert.DeserializeObject<AzureMapsReverseSearchModel>(ret);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"GetMapLocationAsync HTTP request failed: {e.Message}");
            }
            return responseBody;
        }
        public async Task<AzureMapsReverseSearchModel> GetLocationAsync(IEnumerable<ImageAnalysisModel> items)
        {
            return await GetMapLocationAsync(GetLatLng(items));
        }
    }
}
