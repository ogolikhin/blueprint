import { ArtifactState} from "../state";
import { ArtifactProperties } from "../properties";
import { IStatefulArtifact } from "../interfaces";


export class StatefullArtifact implements IStatefulArtifact {
    public manager: any;
    public artifactId: number;
    public state: ArtifactState;
    public properties: ArtifactProperties; 


    public static $inject = ["$http", "$q"];

    constructor(manager: any, artifact: any) {
        
        this.manager = manager;

        this.artifactId = artifact.id;
        this.state = new ArtifactState(this);
        this.properties = new ArtifactProperties(this);
    }

    public lock() {
        this.manager.lock(this)
    }

 
}
