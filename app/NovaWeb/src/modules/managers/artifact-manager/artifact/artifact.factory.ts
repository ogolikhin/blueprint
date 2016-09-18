import { IMessageService } from "../../../core";
import { Models } from "../../../main/models";
import { StatefulArtifactServices } from "../services";
import { IMetaDataService, IStatefulSubArtifact, IStatefulArtifact,  StatefulArtifact, StatefulSubArtifact } from "../";
import { IArtifactService } from "./artifact.svc";
import {
    // IStatefulArtifact, 
    ISession, 
    IStatefulArtifactServices, 
    IArtifactAttachmentsService
} from "../../models";

export interface IStatefulArtifactFactory {
    createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact;
    createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact;
    createStatefulProcessArtifact(artifact: Models.IArtifact);
}

export class StatefulArtifactFactory implements IStatefulArtifactFactory {

    public static $inject = [
        "$q",
        "session",
        "messageService",
        "artifactService",
        "artifactAttachments",
        "metadataService"
    ];

    private services: IStatefulArtifactServices;

    constructor(
        private $q: ng.IQService,
        private session: ISession,
        private messageService: IMessageService,
        private artifactService: IArtifactService,
        private attachmentService: IArtifactAttachmentsService,
        private metadataService: IMetaDataService
        ) {

        this.services = new StatefulArtifactServices( 
            this.$q,
            this.session,
            this.messageService,
            this.artifactService,
            this.attachmentService,
            this.metadataService);
    }

    public createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        return new StatefulArtifact(artifact, this.services);
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, this.services);
    }

    public createStatefulProcessArtifact(): IStatefulArtifact {
        // TODO: implement for process
        throw Error("this hasn't been implemented yet");
    }
}
