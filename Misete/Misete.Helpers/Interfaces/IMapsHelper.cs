namespace Misete.Helpers.Interfaces
{
    public interface IMapsHelper
    {
        public Task<AzureMapsReverseSearchModel> GetLocationAsync(IEnumerable<ImageAnalysisModel> items);
        public Task<AzureMapsReverseSearchModel> GetMapLocationAsync(string ltlg);

    }
}