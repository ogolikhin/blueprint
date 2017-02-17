import {IItemInfoResult, IItemInfoService} from "../../../commonModule/itemInfo/itemInfo.service";
import {ILoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IProcessShape} from "../../../editorsModule/bp-process/models/process-models";
import {IStatefulCollectionArtifact, StatefulCollectionArtifact} from "../../../editorsModule/configuration/classes/collection-artifact";
import {IStatefulBaselineArtifact, StatefulBaselineArtifact} from "../../../editorsModule/configuration/classes/baseline-artifact";
import {IStatefulDiagramArtifact, StatefulDiagramArtifact} from "../../../editorsModule/diagram/diagram-artifact";
import {StatefulUseCaseArtifact} from "../../../editorsModule/diagram/usecase-artifact";
import {StatefulUseCaseDiagramArtifact} from "../../../editorsModule/diagram/usecase-diagram-artifact";
import {IStatefulGlossaryArtifact, StatefulGlossaryArtifact} from "../../../editorsModule/glossary/glossary-artifact";
import {IPropertyDescriptorBuilder} from "../../../editorsModule/services";
import {IUnpublishedArtifactsService} from "../../../editorsModule/unpublished/unpublished.service";
import {IMessageService} from "../../../main/components/messages/message.svc";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IArtifact, ISubArtifact} from "../../../main/models/models";
import {IDialogService} from "../../../shared/";
import {ISession} from "../../../shell/login/session.svc";
import {IStatefulArtifact, StatefulArtifact, StatefulProcessArtifact, StatefulProcessSubArtifact} from "../artifact";
import {IArtifactAttachmentsService} from "../attachments";
import {IMetaDataService} from "../metadata";
import {StatefulProjectArtifact} from "../project/project-artifact";
import {IArtifactRelationshipsService} from "../relationships";
import {IStatefulArtifactServices, StatefulArtifactServices} from "../services";
import {IStatefulSubArtifact, StatefulSubArtifact} from "../sub-artifact";
import {IValidationService} from "../validation/validation.svc";
import {IArtifactService} from "./artifact.svc";

export interface IStatefulArtifactFactory {
    createStatefulArtifact(artifact: IArtifact): IStatefulArtifact;
    createStatefulArtifactFromId(artifactId: number): ng.IPromise<IStatefulArtifact>;
    createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: ISubArtifact): IStatefulSubArtifact;
    createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact;
}

export class StatefulArtifactFactory implements IStatefulArtifactFactory {

    public static $inject = [
        "$q",
        "$log",
        "session",
        "messageService",
        "dialogService",
        "localization",
        "artifactService",
        "artifactAttachments",
        "artifactRelationships",
        "metadataService",
        "itemInfoService",
        "loadingOverlayService",
        "publishService",
        "validationService",
        "propertyDescriptorBuilder"
    ];

    private services: IStatefulArtifactServices;

    constructor(private $q: ng.IQService,
                private $log: ng.ILogService,
                private session: ISession,
                private messageService: IMessageService,
                private dialogService: IDialogService,
                private localizationService: ILocalizationService,
                private artifactService: IArtifactService,
                private attachmentService: IArtifactAttachmentsService,
                private relationshipsService: IArtifactRelationshipsService,
                private metadataService: IMetaDataService,
                private itemInfoService: IItemInfoService,
                private loadingOverlayService: ILoadingOverlayService,
                private publishService: IUnpublishedArtifactsService,
                private validationService: IValidationService,
                private propertyDescriptor: IPropertyDescriptorBuilder) {

        this.services = new StatefulArtifactServices(
            this.$q,
            this.$log,
            this.session,
            this.messageService,
            this.dialogService,
            this.localizationService,
            this.artifactService,
            this.attachmentService,
            this.relationshipsService,
            this.metadataService,
            this.loadingOverlayService,
            this.publishService,
            this.validationService,
            this.propertyDescriptor);
    }

    public createStatefulArtifactFromId(artifactId: number): ng.IPromise<IStatefulArtifact> {

        return this.itemInfoService.get(artifactId).then((result: IItemInfoResult) => {
            if (this.itemInfoService.isArtifact(result)) {
                const artifact: IArtifact = {
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

                let extendedArtifact = this.createStatefulArtifact(artifact);
                if (result.isDeleted) {
                    extendedArtifact.artifactState.deleted = result.isDeleted;
                    extendedArtifact.artifactState.deletedByDisplayName = result.deletedByUser.displayName;
                    extendedArtifact.artifactState.deletedDateTime = result.deletedDateTime;
                }
                return extendedArtifact;
            }
        });
    }

    public createStatefulArtifact(artifact: IArtifact): IStatefulArtifact {
        if (!artifact) {
            throw Error("Argument 'artifact' should not be null or undefined");
        }
        switch (artifact.predefinedType) {
            case ItemTypePredefined.Project:
                return this.createStatefulProjectArtifact(artifact);
            case ItemTypePredefined.GenericDiagram:
            case ItemTypePredefined.BusinessProcess:
            case ItemTypePredefined.DomainDiagram:
            case ItemTypePredefined.Storyboard:
            case ItemTypePredefined.UIMockup:
                return this.createStatefulDiagramArtifact(artifact);
            case ItemTypePredefined.UseCaseDiagram:
                return this.createStatefulUseCaseDiagramArtifact(artifact);
            case ItemTypePredefined.UseCase:
                return this.createStatefulUseCaseArtifact(artifact);
            case ItemTypePredefined.Process:
                return this.createStatefulProcessArtifact(artifact);
            case ItemTypePredefined.ArtifactCollection:
                return this.createStatefulCollectionArtifact(artifact);
            case ItemTypePredefined.Glossary:
                return this.createStatefulGlossaryArtifact(artifact);
            case ItemTypePredefined.ArtifactBaseline:
                return this.createStatefulBaselineArtifact(artifact);
            default:
                return new StatefulArtifact(artifact, this.services);
        }
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, this.services);
    }

    public createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact {
        return new StatefulProcessSubArtifact(artifact, subArtifact, this.services);
    }

    public createStatefulCollectionArtifact(artifact: IArtifact): IStatefulCollectionArtifact {
        return new StatefulCollectionArtifact(artifact, this.services);
    }

    public createStatefulBaselineArtifact(artifact: IArtifact): IStatefulBaselineArtifact {
        return new StatefulBaselineArtifact(artifact, this.services);
    }

    public createStatefulGlossaryArtifact(artifact: IArtifact): IStatefulGlossaryArtifact {
        return new StatefulGlossaryArtifact(artifact, this.services);
    }

    public createStatefulDiagramArtifact(artifact: IArtifact): IStatefulDiagramArtifact {
        return new StatefulDiagramArtifact(artifact, this.services);
    }

    public createStatefulUseCaseArtifact(artifact: IArtifact): IStatefulDiagramArtifact {
        return new StatefulUseCaseArtifact(artifact, this.services);
    }

    public createStatefulUseCaseDiagramArtifact(artifact: IArtifact): IStatefulDiagramArtifact {
        return new StatefulUseCaseDiagramArtifact(artifact, this.services);
    }

    private createStatefulProcessArtifact(artifact: IArtifact): IStatefulArtifact {
        return new StatefulProcessArtifact(artifact, this.services);
    }

    private createStatefulProjectArtifact(artifact: IArtifact): IStatefulArtifact {
        return new StatefulProjectArtifact(artifact, this.services);
    }
}
