import "../../../";
import "angular-mocks";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {MetaDataServiceMock} from "../../../../managers/artifact-manager/metadata/metadata.svc.mock";
import {ModalServiceInstanceMock} from "../../../../shell/login/mocks.spec";
import {ItemTypePredefined} from "../../../models/item-type-predefined";
import {IItemType} from "../../../models/models";
import {MessageServiceMock} from "../../messages/message.mock";
import {CreateNewArtifactController} from "./new-artifact";
import {DataMock, DialogSettingsMock} from "./new-artifact.mock";
import * as angular from "angular";

describe("CreateNewArtifactController", () => {

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("ctrl", CreateNewArtifactController);
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("dialogSettings", DialogSettingsMock);
        $provide.service("dialogData", DataMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("metadataService", MetaDataServiceMock);
    }));

    it("filters ItemTypePredefined by parent",
        inject((ctrl: CreateNewArtifactController) => {
            let types: ItemTypePredefined[];

            //Arrange
            types = ctrl.filterItemTypePredefinedByParent();

            //Assert
            expect(types.length).not.toBe(0);
            expect(types.indexOf(ItemTypePredefined.TextualRequirement)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.PrimitiveFolder)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.Actor)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.Document)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.Process)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.BaselineFolder)).toBe(-1);
            expect(types.indexOf(ItemTypePredefined.ArtifactBaseline)).toBe(-1);
            expect(types.indexOf(ItemTypePredefined.ArtifactReviewPackage)).toBe(-1);
            expect(types.indexOf(ItemTypePredefined.CollectionFolder)).toBe(-1);
            expect(types.indexOf(ItemTypePredefined.ArtifactCollection)).toBe(-1);
        }));

    it("list available types",
        inject((ctrl: CreateNewArtifactController) => {
            let types: IItemType[];

            //Arrange
            types = ctrl.availableItemTypes();

            //Assert
            expect(types.length).toBe(0);
        }));

    it("create button is disabled if type empty",
        inject((ctrl: CreateNewArtifactController) => {
            ctrl.newArtifactName = "New artifact";
            //Assert
            expect(ctrl.isCreateButtonDisabled).toBeTruthy();
        }));

    it("create button is disabled if name empty",
        inject((ctrl: CreateNewArtifactController) => {
            ctrl.newArtifactType = {
                id: 195,
                name: "Actor",
                projectId: 1,
                versionId: 33,
                prefix: "AC",
                predefinedType: 4104,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            };
            //Assert
            expect(ctrl.isCreateButtonDisabled).toBeTruthy();
        }));

    it("create button is enabled if both filled",
        inject((ctrl: CreateNewArtifactController) => {
            ctrl.newArtifactName = "New artifact";
            ctrl.newArtifactType = {
                id: 195,
                name: "Actor",
                projectId: 1,
                versionId: 33,
                prefix: "AC",
                predefinedType: 4104,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            };
            //Assert
            expect(ctrl.isCreateButtonDisabled).toBeFalsy();
        }));
});
