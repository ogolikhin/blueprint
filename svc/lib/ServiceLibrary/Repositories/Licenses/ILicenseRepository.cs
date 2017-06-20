using System.Collections.Generic;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public interface ILicenseRepository
    {
        List<ApplicationSetting> GetLicenseInfo();
    }
}
