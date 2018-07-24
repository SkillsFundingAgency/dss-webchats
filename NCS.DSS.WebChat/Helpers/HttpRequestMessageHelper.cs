using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.WebChat.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetWebChatFromRequest<T>(HttpRequestMessage req)
        {
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
