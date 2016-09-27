import { IMessageService, ILocalizationService } from "../../../core";
import { IDialogService } from "../../../shared/";

import { IProcessService } from "../../../editors/bp-process/services/process.svc";
import { Models } from "../../../main/models";
import {
    StatefulArtifactServices,
    IStatefulArtifactServices,
    StatefulProcessArtifactServices,
    IStatefulProcessArtifactServices
} from "../services";
import { IArtifactService } from "./artifact.svc";
import {
    IMetaDataService,
    IStatefulSubArtifact,
    IStatefulArtifact,
    StatefulArtifact,
    StatefulSubArtifact,
    IArtifactRelationshipsService,
    StatefulProcessArtifact    
} from "../";
import {
    // IStatefulArtifact, 
    ISession, 
    IArtifactAttachmentsService
} from "../../models";

export interface IStatefulArtifactFactory {
    createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact;
    createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact;
}

export class StatefulArtifactFactory implements IStatefulArtifactFactory {

    public static $inject = [
        "$q",
        "session",
        "messageService",
        "dialogService",
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
        private dialogService: IDialogService,
        private localizationService: ILocalizationService,
        private artifactService: IArtifactService,
        private attachmentService: IArtifactAttachmentsService,
        private relationshipsService: IArtifactRelationshipsService,
        private metadataService: IMetaDataService,
        private processService: IProcessService
        ) {

        this.services = new StatefulArtifactServices( 
            this.$q,
            this.session,
            this.messageService,
            this.dialogService,
            this.localizationService,
            this.artifactService,
            this.attachmentService,
            this.relationshipsService,
            this.metadataService);
    }

    public createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        if (artifact &&
            artifact.predefinedType === Models.ItemTypePredefined.Process) {
            return this.createStatefulProcessArtifact(artifact);
        }
        return new StatefulArtifact(artifact, this.services);
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, this.services);
    }

    private createStatefulProcessArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        let processServices: IStatefulProcessArtifactServices =
            new StatefulProcessArtifactServices(this.services, this.$q, this.processService);

        return new StatefulProcessArtifact (artifact, processServices);
    }
}
