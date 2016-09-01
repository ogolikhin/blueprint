import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactAttachments } from "../attachments";
import { CustomProperties } from "../properties";
import { IStatefulArtifact, IArtifactPropertyValues, IArtifactManagerService } from "../interfaces";


export class StatefullArtifact implements IStatefulArtifact, Models.IArtifact {
    private artifact: Models.IArtifact;
    public manager: IArtifactManagerService;
    public state: ArtifactState;
    public attachments: ArtifactAttachments;
    public customProperties: IArtifactPropertyValues; 

    //TODO. 
    //Needs implementation of other object like 
    //attachments, traces and etc.

    public get id(): number {
        return this.artifact.id;
    }

    public get name(): string {
        return this.artifact.name;
    }

    public get description(): string {
        return this.artifact.name;
    }

    public get itemTypeId(): number {
        return this.artifact.itemTypeId;
    }
    public get itemTypeVersionId(): number {
        return this.artifact.itemTypeVersionId;
    }
    public get predefinedType(): Models.ItemTypePredefined {
        return this.artifact.predefinedType;
    }

    public get permissions(): Enums.RolePermissions {
        return this.artifact.permissions;
    }

    public get projectId() {
        return this.artifact.projectId;
    }

    public get prefix(): string {
        return this.artifact.prefix;
    }

    public get parentId(): number {
        return this.artifact.parentId;
    }
    

    constructor(manager: IArtifactManagerService, artifact: Models.IArtifact) {
        this.manager = manager;
        this.artifact = artifact;
        this.state = new ArtifactState(this);
        this.attachments = new ArtifactAttachments(this);
        this.customProperties = new CustomProperties(this, artifact.customPropertyValues);
    }

 
 //ArtifactProperties.name
}
