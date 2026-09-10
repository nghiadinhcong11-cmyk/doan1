using System.Threading.Tasks;

namespace RestaurantPOS.AI.Services
{
    public interface IGeminiService
    {
        /// <summary>
        /// Gửi request tới Gemini API và trả về chuỗi JSON thô
        /// </summary>
        Task<string> GenerateContentAsync(object requestBody);
    }
}
