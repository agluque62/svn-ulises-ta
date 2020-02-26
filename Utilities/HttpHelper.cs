using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;


namespace Utilities
{    public class HttpHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
            
        public static async Task<string> Get(string Url)
        {
            // The actual Get method
            using (var result = await _httpClient.GetAsync(Url))
            {
                string content = await result.Content.ReadAsStringAsync();
                return content;
            }
        }
    }
}
