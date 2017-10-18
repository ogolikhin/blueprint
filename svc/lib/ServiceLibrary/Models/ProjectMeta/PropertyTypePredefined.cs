using System.Diagnostics.CodeAnalysis;

namespace ServiceLibrary.Models.ProjectMeta
{
    // The content is copied from the Raptor solution, and Display attributes are removed.
    // KEEP IN SYNC!

    /// <summary>
    /// For any property, this maps to the id in the database that defines the type of property it is in PropertyValueVersions
    /// </summary>
    /// <remarks>
    /// ID                      = 4097
    /// Name                    = 4098
    /// Description             = 4099
    /// UseCaseLevel            = 4100
    /// ReadOnly                = 4101
    /// ItemLabel               = 4102
    /// RowLabel                = 4103
    /// ColumnLabel             = 4104
    /// DataObjectType          = 4105
    /// ExtensionType           = 4106
    /// Condition               = 4107
    /// BPObjectType            = 4108
    /// WidgetType              = 4109
    /// ReturnToStepName        = 4110
    /// RawData                 = 4111
    /// ThreadStatus            = 4112
    /// ApprovalStatus          = 4113
    /// ClientType              = 4114
    /// Label                   = 4115
    /// SharedViewPreferences   = 4116
    /// ValueType               = 4117
    /// IsSealedPublished       = 4118
    /// ALMIntegrationSettings  = 4119
    /// ALMExportInfo           = 4120
    /// ALMSecurity             = 4121
    /// StepOf                  = 4122
    /// DataOperationSet        = 4123
    /// CreatedBy               = 4124
    /// CreatedOn               = 4125
    /// LastEditedBy            = 4126
    /// LastEditedOn            = 4127
    /// X                       = 8193
    /// Y                       = 8194
    /// Width                   = 8195
    /// Height                  = 8196
    /// ConnectorType           = 8197
    /// TruncateText            = 8198
    /// BackgroundColor         = 8199
    /// BorderColor             = 8200
    /// BorderWidth             = 8201
    /// Image                   = 8202
    /// Orientation             = 8203
    /// ClientRawData           = 8204
    /// Theme                   = 8205
    /// Thumbnail               = 8206
    ///
    /// CustomGroup            = 16384
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:CodeMustNotContainMultipleWhitespaceInARow", Justification = "Reviewed.")]
    public enum PropertyTypePredefined
    {
        // Predefined properties
        GroupMask              = 0xF000,

        // None
        None = 0x0000,

        // Predefined system properties
        SystemGroup            = 0x1000,
        ID                     = SystemGroup | 1,
        Name                   = SystemGroup | 2,
        Description            = SystemGroup | 3,
        UseCaseLevel           = SystemGroup | 4,
        ReadOnly               = SystemGroup | 5,
        ItemLabel              = SystemGroup | 6,
        RowLabel               = SystemGroup | 7,
        ColumnLabel            = SystemGroup | 8,
        DataObjectType         = SystemGroup | 9,
        ExtensionType          = SystemGroup | 10,
        Condition              = SystemGroup | 11,
        BPObjectType           = SystemGroup | 12,
        WidgetType             = SystemGroup | 13,
        ReturnToStepName       = SystemGroup | 14,
        RawData                = SystemGroup | 15,
        ThreadStatus           = SystemGroup | 16,
        ApprovalStatus         = SystemGroup | 17,
        ClientType             = SystemGroup | 18,
        Label                  = SystemGroup | 19,
        SharedViewPreferences  = SystemGroup | 20,
        ValueType              = SystemGroup | 21,
        IsSealedPublished      = SystemGroup | 22,
        ALMIntegrationSettings = SystemGroup | 23,
        ALMExportInfo          = SystemGroup | 24,
        ALMSecurity            = SystemGroup | 25,
        StepOf                 = SystemGroup | 26,
        DataOperationSet       = SystemGroup | 27,
        CreatedBy              = SystemGroup | 28,
        CreatedOn              = SystemGroup | 29,
        LastEditedBy           = SystemGroup | 30,
        LastEditedOn           = SystemGroup | 31,

        // Predefined visualization properties
        VisualizationGroup     = 0x2000,
        X                      = VisualizationGroup | 1,
        Y                      = VisualizationGroup | 2,
        Width                  = VisualizationGroup | 3,
        Height                 = VisualizationGroup | 4,
        ConnectorType          = VisualizationGroup | 5,
        TruncateText           = VisualizationGroup | 6,
        BackgroundColor        = VisualizationGroup | 7,
        BorderColor            = VisualizationGroup | 8,
        BorderWidth            = VisualizationGroup | 9,
        Image                  = VisualizationGroup | 10,
        Orientation            = VisualizationGroup | 11,
        ClientRawData          = VisualizationGroup | 12,
        Theme                  = VisualizationGroup | 13,
        Thumbnail              = VisualizationGroup | 14,

        // Custom properties
        CustomGroup            = 0x4000
    }
}
