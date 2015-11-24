using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        
    }
}