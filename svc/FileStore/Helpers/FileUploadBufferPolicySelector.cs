using System.Net.Http;
using System.Web.Http.WebHost;

namespace FileStore
{
    public class FileUploadBufferPolicySelector : WebHostBufferPolicySelector
    {
        public override bool UseBufferedInputStream(object hostContext)
        {
            return false;
        }

        public override bool UseBufferedOutputStream(HttpResponseMessage response)
        {

            return ((int)response.StatusCode) > 399 ? true : false;

        }
    }
}