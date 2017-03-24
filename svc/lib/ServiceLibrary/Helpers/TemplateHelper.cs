using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace ServiceLibrary.Helpers
{
    public static class TemplateHelper
    {
        //This will be availailabke starting with .NET 4.6
        //Otherwise have the error CA135 http://msdn.microsoft.com/library/ms182190.aspx

        //public static string CreatePasswordRecoveryEmail(dynamic model)
        //{

        //    var result =  $@"
        //        <html>
        //            <div>Hello {model.UserName}.</div>
        //            <br>
        //            <div>We have received a request to reset your password.</div>
        //            <br>
        //            <div>To confirm this password reset, visit the following address:</div>
        //            <a href='{model.Url}'>Reset Password</a>
        //            <br><br>
        //            <div>If you did not make this request, you can ignore this email, and no changes will be made.</div>
        //            <br>
        //            <div>If you have any questions, please contact your administrator. </div>
        //        </html>";
        //    return result;
        //}
    }
}
