﻿using Model.OpenApiModel;

namespace Model
{
    public enum RelationshipDirection
    {
        Bidirectional,
        From,
        To
    }


    public interface IRelationship
    {
        RelationshipDirection Direction { get; set; }
        IArtifact RelatedArtifact { get; set; }
    }
}
