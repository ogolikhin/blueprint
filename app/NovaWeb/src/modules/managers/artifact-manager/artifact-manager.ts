import { IStatefulArtifact } from "./interfaces";
import { StatefullArtifact  } from "./artifact";
import { Models, Enums } from "../../main/models";

export interface IArtifactManager {
    $q: ng.IQService;
    list(): IStatefulArtifact[];
    add(artifact: Models.IArtifact);
    get(id: number): IStatefulArtifact;
    // getObeservable(id: number): Rx.Observable<IStatefulArtifact>;
    remove(id: number);
    // removeAll(); // when closing all projects
    // refresh(id); // refresh lightweight artifact
    // refreshAll(id);
}

export class ArtifactManager  implements IArtifactManager {

    public static $inject = [
        "$http", 
        "$q"
    ];

    private artifactList: IStatefulArtifact[];

    constructor(
        private $http: ng.IHttpService, 
        public $q: ng.IQService) {

        this.artifactList = [];
    }

    public list(): IStatefulArtifact[] {
        return this.artifactList;
    }

    public get(id: number): IStatefulArtifact {
        const foundArtifacts = this.artifactList.filter((artifact: IStatefulArtifact) => 
            artifact.id === id);

        return foundArtifacts.length ? foundArtifacts[0] : null;
    }
    
    public add(artifact: Models.IArtifact) {
        this.artifactList.push(new StatefullArtifact(this, artifact));
    }

    public remove(id: number) {
        this.artifactList = this.artifactList.filter((artifact: IStatefulArtifact) => 
            artifact.properties.system().id !== id);
    }

    public update(id: number) {
        // TODO: 
    }

}
