import {IMessageService, ILocalizationService} from "../../../core";
import {IDialogService} from "../../../shared/";
import {ISession} from "../../../shell/login/session.svc";
import {IProcessService} from "../../../editors/bp-process/services/process.svc";
import {IProcessShape} from "../../../editors/bp-process/models/process-models";
import {Models} from "../../../main/models";
import {IArtifactAttachmentsService} from "../attachments";
import {IMetaDataService} from "../metadata";
import {StatefulSubArtifact, IStatefulSubArtifact} from "../sub-artifact";
import {IStatefulArtifact, StatefulArtifact, StatefulProcessArtifact, StatefulProcessSubArtifact} from "../artifact";
import {IArtifactRelationshipsService} from "../relationships";
import {
    StatefulArtifactServices,
    IStatefulArtifactServices,
    StatefulProcessArtifactServices,
    IStatefulProcessArtifactServices
} from "../services";
import {IArtifactService} from "./artifact.svc";
import {ILoadingOverlayService} from "../../../core/loading-overlay";

export interface IStatefulArtifactFactory {
    createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact;
    createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact;
    createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact;
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
        "processService",
        "loadingOverlayService"
    ];

    private services: IStatefulArtifactServices;

    constructor(private $q: ng.IQService,
                private session: ISession,
                private messageService: IMessageService,
                private dialogService: IDialogService,
                private localizationService: ILocalizationService,
                private artifactService: IArtifactService,
                private attachmentService: IArtifactAttachmentsService,
                private relationshipsService: IArtifactRelationshipsService,
                private metadataService: IMetaDataService,
                private processService: IProcessService,
                private loadingOverlayService: ILoadingOverlayService) {

        this.services = new StatefulArtifactServices(
            this.$q,
            this.session,
            this.messageService,
            this.dialogService,
            this.localizationService,
            this.artifactService,
            this.attachmentService,
            this.relationshipsService,
            this.metadataService,
            this.loadingOverlayService);
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

    public createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact {
        return new StatefulProcessSubArtifact(artifact, subArtifact, this.services);
    }

    private createStatefulProcessArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        let processServices: IStatefulProcessArtifactServices =
            new StatefulProcessArtifactServices(this.services, this.$q, this.processService);

        return new StatefulProcessArtifact(artifact, processServices);
    }
}
