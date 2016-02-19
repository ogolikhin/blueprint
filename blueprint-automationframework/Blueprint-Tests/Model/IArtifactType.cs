using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IArtifactType
    {
        #region Properties
        int Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        string Prefix { get; set; }

        BaseArtifactType BaseArtifactType { get; set; }

        #endregion Properties
        #region Methods
        #endregion Methods
    }
}
