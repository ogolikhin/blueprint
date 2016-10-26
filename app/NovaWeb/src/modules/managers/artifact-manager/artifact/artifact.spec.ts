import * as angular from "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import { Models, Enums } from "../../../main/models";
import {IPublishService} from "./../publish.svc/publish.svc";
import {PublishServiceMock} from "./../publish.svc/publish.svc.mock";
import {IStatefulArtifact} from "./artifact";
import {ItemTypePredefined} from "../../../main/models/enums";
import {ArtifactRelationshipsMock} from "./../../../managers/artifact-manager/relationships/relationships.svc.mock";
import {ArtifactAttachmentsMock} from "./../../../managers/artifact-manager/attachments/attachments.svc.mock";
import {ArtifactServiceMock} from "./../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {ProcessServiceMock} from "../../../editors/bp-process/services/process.svc.mock";
import {SelectionManager} from "./../../../managers/selection-manager/selection-manager";
import {MessageServiceMock} from "../../../core/messages/message.mock";
import {
    ArtifactManager,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService
} from "../../../managers/artifact-manager";

describe("Artifact", () => {
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
        $provide.service("publishService", PublishServiceMock);
    }));

    describe("Publish", () => {
        it("success", inject((publishService: IPublishService, statefulArtifactFactory: IStatefulArtifactFactory, $rootScope: ng.IRootScopeService) => {
            // Arrange
            const artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                lockedByUser: Enums.LockedByEnum.CurrentUser,
                predefinedType: Models.ItemTypePredefined.Actor
            } as Models.IArtifact;
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);

            // Act
            artifact.publish();
            $rootScope.$digest();
            
            // Assert
            expect(artifact.artifactState.lockedBy).toEqual(Enums.LockedByEnum.None);
        }));

    });

});