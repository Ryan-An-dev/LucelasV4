using System.Net.Http;
using System.Threading.Tasks;

namespace AddressSearchManager
{
  public class RestfulAPI
  {
    public static async Task<string> GetAsync(string url)
    {
      using HttpClient? httpClient = new HttpClient();
      HttpResponseMessage? res = await httpClient.GetAsync(url);
      return await res.Content.ReadAsStringAsync();
    }
  }
}
