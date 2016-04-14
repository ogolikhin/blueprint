﻿using System.Collections.Generic;
using Model.OpenApiModel;
using Model.OpenApiModel.Impl;

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

        List<PropertyType> PropertyTypes { get; }
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
