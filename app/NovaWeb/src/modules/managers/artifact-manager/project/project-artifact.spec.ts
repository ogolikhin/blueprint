import * as angular from "angular";
import "angular-mocks";
import "../../../shell";
import {LocalizationServiceMock} from "../../../core/localization/localization.service.mock";
import {Models, Enums} from "../../../main/models";
import {IStatefulArtifact} from "../artifact";
import {ArtifactRelationshipsMock} from "../relationships/relationships.svc.mock";
import {ArtifactAttachmentsMock} from "../attachments/attachments.svc.mock";
import {ArtifactServiceMock} from "../artifact/artifact.svc.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ProcessServiceMock} from "../../../editors/bp-process/services/process.svc.mock";
import {SelectionManager} from "../../selection-manager/selection-manager";
import {
    ArtifactManager,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService
} from "../../../managers/artifact-manager";
import {ValidationService} from "../validation/validation.svc";
import {PropertyDescriptorBuilderMock} from "../../../editors/configuration/property-descriptor-builder.mock";
import {UnpublishedArtifactsServiceMock} from "../../../editors/unpublished/unpublished.svc.mock";
import {MessageServiceMock} from "../../../main/components/messages/message.mock";


describe("Project", () => {
    let project: IStatefulArtifact;
    const subscribers: any[] = [];

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("processService", ProcessServiceMock);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("validationService", ValidationService);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
    }));

    beforeEach(inject((statefulArtifactFactory: IStatefulArtifactFactory) => {
        const projectModel = {
            id: 22,
            name: "Artifact",
            prefix: "My",
            lockedByUser: Enums.LockedByEnum.None,
            predefinedType: Models.ItemTypePredefined.Project,
            version: 0
        } as Models.IArtifact;
        project = statefulArtifactFactory.createStatefulArtifact(projectModel);
    }));

    it("can be saved", inject(() => {
        // arrange
        spyOn(project, "canBeSaved").and.callThrough();

        // act
        const result: boolean = project.canBeSaved();

        // assert
        expect(result).toEqual(false);
    }));

    it("can be published", inject(() => {
        // arrange
        spyOn(project, "canBePublished").and.callThrough();

        // act
        const result: boolean = project.canBePublished();

        // assert
        expect(result).toEqual(false);
    }));

    it("can be loaded", inject(() => {
        // arrange
        spyOn(project, "canBeLoaded").and.callThrough();

        // act
        const result: boolean = project.canBePublished();

        // assert
        expect(result).toEqual(false);
    }));
});
