
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

export interface IProjectNode {
    id: number;
    type: number;
    name: string;
    parentFolderId: number;
    description?: string;
    hasChildren: boolean;
    children?: IProjectNode[];
}

export interface IObservable {
    setProperty(name: string, value: any);
}

export interface IArtifact extends IObservable  {
    id: number;
    name: string;
    projectId: number;
    typeId: number;
    parentId: number;
    predefinedType: ArtifactTypeEnum;
    prefix?: string;
    version?: number;
    hasChildren?: boolean;  
    artifacts?: IArtifact[];
    //flags:
}

export interface IProject extends IArtifact {
}


export class Project implements IProject {
    private _data: any;
    constructor(id: number, name: string, data?: IArtifact[]) { //
        this._data = {
            id: id,
            name: name,
            artifacts: data
        };
        this.notificator = angular.injector(["app.core"]).get("eventManager");
    };

    public get id(): number {
        return this._data["id"] as number;
    }
    public set id(value: number) {
        this.setProperty("id", value);
    }

    public get name(): string { 
        return this._data["name"] as string;
    }
    public set name(value: string) {
        this.setProperty("name", value);
    }

    public get projectId() {
        return this.id;
    }
    public typeId: number;
    public get parentId() {
        return -1;
    }

    public get predefinedType(): ArtifactTypeEnum {
        return ArtifactTypeEnum.Project;
    }

    public get hasChildren() {
        return this.artifacts.length > 0;
    }

    public get artifacts(): IArtifact[] {
        return this._data["artifacts"] as IArtifact[];
    }
    public set artifacts(value: IArtifact[]) {
        this.setProperty("artifacts", value);
    }

    private notificator;

    public setProperty(name: string, value: any) {
        if (name in this._data) {
            let oldValue = this._data[name];
            this._data[name] = value;
            if (this.notificator) {
                this.notificator.dispatch("main", "propertychange", this, name, value, oldValue);
            }
        }
    }
}


export class BaseItem {
    private notificator;
    constructor() {
        let injector = angular.injector["app.core"];
        this.notificator = injector.get("eventManager");
    }

    public setProperty(name: string, value: any) {
        if (this[name]) {
            this[name] = value;
        }
    }
}


