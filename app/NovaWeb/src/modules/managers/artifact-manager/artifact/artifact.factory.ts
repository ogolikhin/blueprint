import { IMessageService } from "../../../core";
import { Models } from "../../../main/models";
import { StatefulArtifactServices } from "../services";
import { StatefulArtifact } from "./artifact";
import { IMetaDataService } from "../";
import { IArtifactService } from "./artifact.svc";
import {
    IStatefulArtifact, 
    ISession, 
    IStatefulArtifactServices, 
    IArtifactAttachmentsService,
} from "../../models";

export interface IStatefulArtifactFactory {
    createStatefulArtifact(artifact: Models.IArtifact);
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

    public createStatefulProcessArtifact(): IStatefulArtifact {
        // TODO: implement for process
        throw Error("this hasn't been implemented yet");
    }
}
