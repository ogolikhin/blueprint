import "angular-mocks";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {PropertyDescriptorBuilderMock} from "../../../editorsModule/services";
import {UnpublishedArtifactsServiceMock} from "../../../editorsModule/unpublished/unpublished.service.mock";
import {MessageServiceMock} from "../../../main/components/messages/message.mock";
import {Enums, Models} from "../../../main/models";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {IStatefulArtifactFactory, MetaDataService, StatefulArtifactFactory} from "../../../managers/artifact-manager";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {SessionSvcMock} from "../../../shell/login/session.svc.mock";
import {SelectionManager} from "../../selection-manager/selection-manager";
import {IStatefulArtifact} from "../artifact";
import {ArtifactServiceMock} from "../artifact/artifact.svc.mock";
import {ArtifactAttachmentsMock} from "../attachments/attachments.svc.mock";
import {ArtifactRelationshipsMock} from "../relationships/relationships.svc.mock";
import {ValidationService} from "../validation/validation.svc";
import * as angular from "angular";

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
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("validationService", ValidationService);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
        $provide.service("session", SessionSvcMock);
    }));

    beforeEach(inject((statefulArtifactFactory: IStatefulArtifactFactory) => {
        const projectModel = {
            id: 22,
            name: "Artifact",
            prefix: "My",
            lockedByUser: Enums.LockedByEnum.None,
            predefinedType: ItemTypePredefined.Project,
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
