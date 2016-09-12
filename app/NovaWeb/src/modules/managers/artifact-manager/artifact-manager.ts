import { IMessageService } from "../../core/";
import { Models } from "../../main/models";
import { 
    IArtifactManager, 
    IStatefulArtifact, 
    ISession, 
    IStatefulArtifactServices, 
    IArtifactAttachmentsService,
    IArtifactService,
 } from "../models";
import { StatefulArtifact } from "./artifact";
import { StatefulArtifactServices } from "./services";


export class ArtifactManager  implements IArtifactManager {

    public static $inject = [
        "$http", 
        "$q",
        "session",
        "messageService",
        "artifactService",
        "artifactAttachments"
    ];

    private artifactList: IStatefulArtifact[];
    private services: IStatefulArtifactServices;

    constructor(
        private $http: ng.IHttpService, 
        private $q: ng.IQService,
        private session: ISession,
        private messageService: IMessageService,
        private artifactService: IArtifactService,
        private attachmentService: IArtifactAttachmentsService
        ) {

        this.services = new StatefulArtifactServices( 
            this.$q,
            this.messageService,
            this.artifactService,
            this.attachmentService);

        this.artifactList = [];
    }

    public get currentUser(): Models.IUserGroup {
        return this.session.currentUser;
    }

    public get messages(): IMessageService {
        return this.messageService;
    }

    public list(): IStatefulArtifact[] {
        return this.artifactList;
    }

    public get(id: number): IStatefulArtifact {
        return this.artifactList.filter((artifact: IStatefulArtifact) => artifact.id === id)[0] || null;
    }
    
    public add(artifact: Models.IArtifact): IStatefulArtifact {
        let length = this.artifactList.push(new StatefulArtifact(this, artifact, this.services));
        return this.artifactList[length - 1];
    }

    public remove(id: number): IStatefulArtifact {
        let stateArtifact: IStatefulArtifact;
        this.artifactList = this.artifactList.filter((artifact: IStatefulArtifact) => {
            if (artifact.id === id) {
                stateArtifact = artifact;
                return false;
            }
            return true;
        });
        return stateArtifact;
    }

    public update(id: number) {
        // TODO: 
    }



}
