import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactProperties } from "../properties";
import { IStatefulArtifact, IProperty, IArtifactManagerService } from "../interfaces";


export class StatefullArtifact implements IStatefulArtifact {
    public manager: IArtifactManagerService;
    public state: ArtifactState;
    public properties: ArtifactProperties; 

    //TODO. 
    //Needs implementation of other object like 
    //attachments, traces and etc.


    constructor(manager: IArtifactManagerService, artifact: Models.IArtifact) {
        
        this.manager = manager;
        this.properties = new ArtifactProperties(this, artifact);
        this.state = new ArtifactState(this);
    }
 
}
