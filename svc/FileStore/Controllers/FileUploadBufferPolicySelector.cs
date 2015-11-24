using System.Net.Http;
using System.Web.Http.WebHost;

namespace FileStore.Controllers
{
    public class FileUploadBufferPolicySelector : WebHostBufferPolicySelector
    {
        private static string[] _unbufferedControllers = new string[1] { "Files" };

        public override bool UseBufferedInputStream(object hostContext)
        {
            return false;
        }

        public override bool UseBufferedOutputStream(HttpResponseMessage response)
        {
            return false;
        }
    }
}