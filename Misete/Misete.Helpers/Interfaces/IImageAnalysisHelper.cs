using Misete.Models;

namespace Misete.Helpers.Interfaces
{
    public interface IImageAnalysisHelper
    {
        public IEnumerable<ImageAnalysisModel> GetImageDetails(byte[] image);
        public string GetContentAuth(byte[] image, string fileName);
    }
}