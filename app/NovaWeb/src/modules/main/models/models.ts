export enum ArtifactTypeEnum {
    Project = -1,

    Unknown = 0,
    // Artifacts
    Folder = 1,
    Actor = 2,
    Document = 3,
    DomainDiagram = 4,
    GenericDiagram = 5,
    Glossary = 6,
    Process = 7,
    Storyboard = 8,
    Requirement = 9,
    UiMockup = 10,
    UseCase = 11,
    UseCaseDiagram = 12,

    //BaseLines and Reviews
    BaselineReviewFolder = 13,
    Baleline = 14,
    Review = 15,

    //Collections
    CollectionFolder = 16,
    Collection = 17
}

export enum ItemTypePredefined {
    None = 0,
    BaselineArtifactGroup = 256,
    CollectionArtifactGroup = 512,
    PrimitiveArtifactGroup = 4096,
    Project = 4097,
    Baseline = 4098,
    Glossary = 4099,
    TextualRequirement = 4101,
    PrimitiveFolder = 4102,
    BusinessProcess = 4103,
    Actor = 4104,
    UseCase = 4105,
    DataElement = 4106,
    UIMockup = 4107,
    GenericDiagram = 4108,
    Document = 4110,
    Storyboard = 4111,
    DomainDiagram = 4112,
    UseCaseDiagram = 4113,
    Process = 4114,
    BaselineFolder = 4353,
    ArtifactBaseline = 4354,
    ArtifactReviewPackage = 4355,
    CollectionFolder = 4609,
    ArtifactCollection = 4610,
    SubArtifactGroup = 8192,
    GDConnector = 8193,
    GDShape = 8194,
    BPConnector = 8195,
    PreCondition = 8196,
    PostCondition = 8197,
    Flow = 8198,
    Step = 8199,
    Extension = 8200,
    Bookmark = 8213,
    BaselinedArtifactSubscribe = 8216,
    Term = 8217,
    Content = 8218,
    DDConnector = 8219,
    DDShape = 8220,
    BPShape = 8221,
    SBConnector = 8222,
    SBShape = 8223,
    UIConnector = 8224,
    UIShape = 8225,
    UCDConnector = 8226,
    UCDShape = 8227,
    PROShape = 8228,
    CustomArtifactGroup = 16384,
    ObsoleteArtifactGroup = 32768,
    DataObject = 32769,
    GroupMask = 61440
}

export enum ArtifactStateEnum {
    Published = 0,
    Draft = 1,
    Deleted = 2
}

export enum IPrimitiveType {
    Text = 0,
    Number = 1,
    Date = 2,
    User = 3,
    Choice = 4,
    Image = 5
}

export interface IProjectNode {
    id: number;
    type: number;
    name: string;
    parentFolderId: number;
    description?: string;
    hasChildren: boolean;
    children?: IProjectNode[];
}

export interface IArtifact {
    id: number;
    name: string;
    projectId: number;
    typeId: number;
    parentId: number;
    predefinedType: ItemTypePredefined;
    prefix?: string;
    version?: number;
    hasChildren?: boolean;
    artifacts?: IArtifact[];
    //flags:
}
export interface IItemType {
    id: number;
    name: string;
    projectId?: number;
    versionId?: number;
    instanceItemTypeId?: number;
    prefix: string;
    baseType: number;
    iconImageId: number;
    usedInThisProject: boolean;
    customPropertyTypeIds: number[];
}
export interface IPropertyType {
    id: number;
    versionId?: number;
    name: string;
    primitiveType: IPrimitiveType;
    instancePropertyTypeId?: number;
    isRichText?: boolean;
    decimalDefaultValue?: number;
    dateDefaultValue?: Date;
    userGroupDefaultValue?: any[];
    stringDefaultValue?: string;
    decimalPlaces?: number;
    maxNumber?: number;
    minNumber?: number;
    maxDate?: Date;
    minDate?: Date;
    isMultipleAllowed?: boolean;
    isRequired?: boolean;
    isValidated?: boolean;
    validValues?: string[];
    defaultValidValueIndex?: number;
    
    // Extra properties. Maintaned by client
    disabled?: boolean;
}
export interface IPropertyValue {
    typeId: number;
    versionId?: number;
    value: any;
}


export interface IArtifactDetails extends IArtifact {
    propertyValues: IPropertyValue[];
//    links: ILink[];
    //flags:
}
export interface IProjectMeta {
    artifactTypes: IItemType[];
    propertyTypes: IPropertyType[];
    subArtifactTypes: IItemType[];
    //flags:
}

export interface IProject extends IArtifact {
    description: string;
    meta?: IProjectMeta;
}

export class Artifact implements IArtifactDetails {
    private _propertyValues: IPropertyValue[];

    constructor(...data: any[]) { //
        angular.extend(this, ...data);
    };
    public id: number;

    public name: string;

    public projectId: number;
    public parentId: number;
    public predefinedType: ItemTypePredefined;
    public typeId: number;

    public get propertyValues() {
        return this._propertyValues || (this._propertyValues = []);
    }
    public artifacts: IArtifact[];
}

export class Project implements IProject {
    constructor(...data: any[]) { //
        angular.extend(this, ...data);
    };

    public id: number;

    public name: string;

    public description: string;

    public typeId: number;

    public artifacts: IArtifact[];

    public get projectId() {
        return this.id;
    }

    public get parentId() {
        return -1;
    }

    public get predefinedType(): ItemTypePredefined {
        return ItemTypePredefined.Project;
    }

    public get hasChildren() {
        return this.artifacts && this.artifacts.length > 0;
    }

}

export interface IArtifactDetailFields {
    systemFields: AngularFormly.IFieldConfigurationObject[];
    customFields: AngularFormly.IFieldConfigurationObject[];
    noteFields: AngularFormly.IFieldConfigurationObject[];
}




