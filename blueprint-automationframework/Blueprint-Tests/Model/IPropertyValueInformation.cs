namespace Model
{
    public enum PropertyTypePredefined
    {
        None = 0,
        ID = 4097,
        Name = 4098,
        Description = 4099,
        UseCaseLevel = 4100,
        ReadOnly = 4101,
        ItemLabel = 4102,
        RowLabel = 4103,
        ColumnLabel = 4104,
        DataObjectType = 4105,
        ExtensionType = 4106,
        Condition = 4107,
        BPObjectType = 4108,
        WidgetType = 4109,
        ReturnToStepName = 4110,
        RawData = 4111,
        ThreadStatus = 4112,
        ApprovalStatus = 4113,
        ClientType = 4114,
        Label = 4115,
        SharedViewPreferences = 4116,
        ValueType = 4117,
        IsSealedPublished = 4118,
        ALMIntegrationSettings = 4119,
        ALMExportInfo = 4120,
        ALMSecurity = 4121,
        StepOf = 4122,
        DataOperationSet = 4123,
        CreatedBy = 4124,
        CreatedOn = 4125,
        LastEditedBy = 4126,
        LastEditedOn = 4127,

        X = 8193,
        Y = 8194,
        Width = 8195,
        Height = 8196,
        ConnectorType = 8197,
        TruncateText = 8198,
        BackgroundColor = 8199,
        BorderColor = 8200,
        BorderWidth = 8201,
        Image = 8202,
        Orientation = 8203,
        ClientRawData = 8204,
        Theme = 8205,
        Thumbnail = 8206,

        CustomGroup = 16384
    }

    public interface IPropertyValueInformation
    {
        #region Properties

        /// <summary>
        /// The name of the property
        /// </summary>
        string PropertyName { get; set; }

        /// <summary>
        /// The predefined property type
        /// </summary>
        PropertyTypePredefined TypePredefined { get; set; }

        /// <summary>
        /// Property Type Id as defined in the blueprint project metadata
        /// </summary>
        int? TypeId { get; set; }

        /// <summary>
        /// The value of the property
        /// </summary>
        object Value { get; set; }

        #endregion Properties
    }
}
