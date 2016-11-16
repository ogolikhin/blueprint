import "../../../";
import * as angular from "angular";
import * as _ from "lodash";
import "angular-mocks";
import {CreateNewArtifactController} from "./new-artifact";
import {ModalServiceInstanceMock} from "../../../../shell/login/mocks.spec";
import {DialogSettingsMock, DataMock} from "./new-artifact.mock";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {IProjectMeta, IItemType} from "../../../../main/models/models";
import {MetaDataService} from "../../../../managers/artifact-manager/metadata/metadata.svc";

describe("CreateNewArtifactController", () => {

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("ctrl", CreateNewArtifactController);
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("dialogSettings", DialogSettingsMock);
        $provide.service("dialogData", DataMock);
        $provide.service("metadataService", MetaDataService);
        // $provide.service("localization", LocalizationServiceMock);
        // $provide.service("dialogService", DialogServiceMock);
        // $provide.service("dialogSettings", DialogSettingsMock);
    }));

    it("filters ItemTypePredefined by parent",
        inject(($rootScope: ng.IRootScopeService, ctrl: CreateNewArtifactController) => {
            let types: ItemTypePredefined[];

            //Arrange
            types = ctrl.filterItemTypePredefinedByParent();

            //Assert
            expect(types.length).not.toBe(0);
            expect(types.indexOf(ItemTypePredefined.TextualRequirement)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.Process)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.Actor)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.Document)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.PrimitiveFolder)).not.toBe(-1);
            expect(types.indexOf(ItemTypePredefined.ArtifactCollection)).toBe(-1);
            expect(types.indexOf(ItemTypePredefined.CollectionFolder)).toBe(-1);
        }));

    it("list available types",
        inject(($rootScope: ng.IRootScopeService, ctrl: CreateNewArtifactController) => {
            let types: IItemType[];

            //Arrange
            types = ctrl.availableItemTypes();

            //Assert
            expect(types.length).toBe(0);
        }));
});
