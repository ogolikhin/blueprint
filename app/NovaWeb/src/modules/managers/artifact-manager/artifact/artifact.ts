import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactProperties } from "../properties";
import { ArtifactAttachments } from "../attachments";
import { IStatefulArtifact, IProperty, IArtifactManagerService } from "../interfaces";


export class StatefullArtifact implements IStatefulArtifact {
    public manager: IArtifactManagerService;
    public state: ArtifactState;
    public properties: ArtifactProperties; 
    public attachments: ArtifactAttachments;

    //TODO. 
    //Needs implementation of other object like 
    //attachments, traces and etc.


    constructor(manager: IArtifactManagerService, artifact: Models.IArtifact) {
        this.manager = manager;
        this.properties = new ArtifactProperties(this, artifact);
        this.attachments = new ArtifactAttachments(this);
        this.state = new ArtifactState(this);
    }
 
}
