import {ILocalizationService} from "../../../core/localization";
import {IMessageService, Message, MessageType} from "../../../core/messages";
import {IItemInfoService, IItemInfoResult} from "../../../core/navigation/item-info.svc";
import {IDialogService} from "../../../shared/";
import {ISession} from "../../../shell/login/session.svc";
import {IProcessService} from "../../../editors/bp-process/services/process.svc";
import {IProcessShape} from "../../../editors/bp-process/models/process-models";
import {Models} from "../../../main/models";
import {IArtifactAttachmentsService} from "../attachments";
import {IMetaDataService} from "../metadata";
import {StatefulSubArtifact, IStatefulSubArtifact} from "../sub-artifact";
import {IStatefulArtifact, IStatefulCollectionArtifact, StatefulArtifact, StatefulProcessArtifact, StatefulProcessSubArtifact} from "../artifact";
import {IArtifactRelationshipsService} from "../relationships";
import {
    StatefulArtifactServices,
    IStatefulArtifactServices,
    StatefulProcessArtifactServices,
    IStatefulProcessArtifactServices
} from "../services";
import {IArtifactService} from "./artifact.svc";
import {ILoadingOverlayService} from "../../../core/loading-overlay";
import {IPublishService} from "../../../managers/artifact-manager/publish.svc";
import {StatefulCollectionArtifact} from "./collection-artifact";

export interface IStatefulArtifactFactory {
    createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact;
    createStatefulArtifactFromId(artifactId: number): ng.IPromise<IStatefulArtifact>;
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
        "itemInfoService",
        "loadingOverlayService",
        "publishService"
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
                private itemInfoService: IItemInfoService,
                private loadingOverlayService: ILoadingOverlayService,
                private publishService: IPublishService) {

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
            this.loadingOverlayService,
            this.publishService);
    }

    public createStatefulArtifactFromId(artifactId: number): ng.IPromise<IStatefulArtifact> {

        return this.itemInfoService.get(artifactId).then((result: IItemInfoResult) => {
            if (this.itemInfoService.isArtifact(result)) {
                const artifact: Models.IArtifact = {
                    id: result.id,
                    projectId: result.projectId,
                    name: result.name,
                    parentId: result.parentId,
                    predefinedType: result.predefinedType,
                    prefix: result.prefix,
                    version: result.version,
                    orderIndex: result.orderIndex,
                    lockedByUser: result.lockedByUser,
                    lockedDateTime: result.lockedDateTime,
                    permissions: result.permissions
                };
                return this.createStatefulArtifact(artifact);
            }
        });
    }

    public createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        if (!artifact) {
            throw Error("Argument 'artifact' should not be null or undefined");
        }
        if (artifact.predefinedType === Models.ItemTypePredefined.Process) {
            return this.createStatefulProcessArtifact(artifact);
        }

        if (artifact.predefinedType === Models.ItemTypePredefined.ArtifactCollection) {
            return this.createStatefulCollectionArtifact(artifact);
        }

        return new StatefulArtifact(artifact, this.services);
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, this.services);
    }

    public createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact {
        return new StatefulProcessSubArtifact(artifact, subArtifact, this.services);
    }

    public createStatefulCollectionArtifact(artifact: Models.IArtifact): IStatefulCollectionArtifact {
        return new StatefulCollectionArtifact(artifact, this.services);
    }

    private createStatefulProcessArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        let processServices: IStatefulProcessArtifactServices =
            new StatefulProcessArtifactServices(this.services, this.$q, this.processService);

        return new StatefulProcessArtifact(artifact, processServices);
    }
}
