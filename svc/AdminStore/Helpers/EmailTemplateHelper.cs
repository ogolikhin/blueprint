using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminStore.Helpers
{
    public static class EmailTemplateHelper
    {
        public static string GetSendTestEmailTemplate(string websiteAddress)
        {
            return string.Format(CultureInfo.InvariantCulture, @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
                                    <html>
                                        <body>
                                            <p style='font-family:arial;font-size:10pt;'>
                                                A test email was requested from the Blueprint Instance Administration Console.
                                                <br>
                                                <br>
                                            </p>
                                            <p style='font-family:arial;font-size:8.5pt;color:gray'>
                                                This email was sent to you as a registered <a href='http://www.blueprintsys.com'>Blueprint</a>
                                                user from <a href='{0}'>{0}</a>
                                            </p>
                                        </body>
                                    </html>",
                                    websiteAddress);
        }
    }
}
