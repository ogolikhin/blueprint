import { IMessageService, ILocalizationService } from "../../../core";
import { Models } from "../../../main/models";
import { ItemTypePredefined } from "../../../main/models/enums";
import { StatefulArtifactServices, IStatefulArtifactServices } from "../services";
import { IArtifactService } from "./artifact.svc";
import { IProcessService } from "../../../editors/bp-process/services/process/process.svc";
import { StatefulProcessArtifact } from "../../../editors/bp-process/models/process-artifact";
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
    createStatefulProcessArtifact(artifact: Models.IArtifact): IStatefulArtifact;
}

export class StatefulArtifactFactory implements IStatefulArtifactFactory {

    public static $inject = [
        "$q",
        "session",
        "messageService",
        "localization",
        "artifactService",
        "artifactAttachments",
        "artifactRelationships",
        "metadataService",
        "processService"
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
        private metadataService: IMetaDataService,
        private processService: IProcessService) {

        this.services = new StatefulArtifactServices( 
            this.$q,
            this.session,
            this.messageService,
            this.localizationService,
            this.artifactService,
            this.attachmentService,
            this.relationshipsService,
            this.metadataService,
            this.processService);
    }

    public createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        if (artifact.predefinedType === ItemTypePredefined.Process) {
            return this.createStatefulProcessArtifact(artifact);
        } else {
            return new StatefulArtifact(artifact, this.services);
        }
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, this.services);
    }

    public createStatefulProcessArtifact(artifact: Models.IArtifact): IStatefulArtifact {
      
        return new StatefulProcessArtifact(artifact, this.services);
      
    }
}
