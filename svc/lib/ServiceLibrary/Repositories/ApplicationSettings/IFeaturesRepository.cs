using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ServiceLibrary.Repositories.ApplicationSettings
{
    public class Feature
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }

    public interface IFeaturesRepository
    {
        Task<IEnumerable<Feature>> GetFeaturesAsync(bool includeExpired = false);
    }
}