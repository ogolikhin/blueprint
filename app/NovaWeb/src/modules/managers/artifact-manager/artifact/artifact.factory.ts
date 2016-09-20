import { IMessageService, ILocalizationService } from "../../../core";
import { Models } from "../../../main/models";
import { StatefulArtifactServices, IStatefulArtifactServices } from "../services";
import { IArtifactService } from "./artifact.svc";
import { 
    IMetaDataService, 
    IStatefulSubArtifact, 
    IStatefulArtifact,  
    StatefulArtifact, 
    StatefulSubArtifact,
    IArtifactRelationshipsService 
} from "../";
import {
    // IStatefulArtifact, 
    ISession, 
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
        "localizationService",
        "localization",
        "artifactService",
        "artifactAttachments",
        "artifactRelationships",
        "metadataService"
    ];

    private services: IStatefulArtifactServices;

    constructor(
        private $q: ng.IQService,
        private session: ISession,
        private messageService: IMessageService,
        private localizationService: ILocalizationService,
        private artifactService: IArtifactService,
        private attachmentService: IArtifactAttachmentsService,
        private relationshipsService: IArtifactRelationshipsService,
        private metadataService: IMetaDataService
        ) {

        this.services = new StatefulArtifactServices( 
            this.$q,
            this.session,
            this.messageService,
            this.localizationService,
            this.artifactService,
            this.attachmentService,
            this.relationshipsService,
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
