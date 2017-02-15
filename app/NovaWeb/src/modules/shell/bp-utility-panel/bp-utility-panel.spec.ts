import "angular";
import "angular-mocks";
import "angular-sanitize";
import {LocalizationServiceMock} from "../../commonModule/localization/localization.service.mock";
import {ItemTypePredefined} from "../../main/models/itemTypePredefined.enum";
import {IStatefulArtifactFactory, StatefulArtifactFactory} from "../../managers/artifact-manager/artifact/artifact.factory";
import {ArtifactService} from "../../managers/artifact-manager/artifact/artifact.svc";
import {ArtifactAttachmentsService} from "../../managers/artifact-manager/attachments/attachments.svc";
import {MetaDataService} from "../../managers/artifact-manager/metadata/metadata.svc";
import {ArtifactRelationshipsService} from "../../managers/artifact-manager/relationships/relationships.svc";
import {ISelectionManager, SelectionManager} from "../../managers/selection-manager/selection-manager";
import {ComponentTest} from "../../util/component.test";
import {ArtifactHistoryMock} from "./bp-history-panel/artifact-history.mock";
import {BPUtilityPanelController} from "./bp-utility-panel";
import * as angular from "angular";

describe("Component BPUtilityPanel", () => {

    let directiveTest: ComponentTest<BPUtilityPanelController>;
    let template = `<bp-utility-panel></bp-utility-panel>`;
    let vm: BPUtilityPanelController;

    //todo: when refactor you should only bootstrap one app. module not both as this is creating dependancy no needed
    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactService", ArtifactService);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("metadataService", MetaDataService);
        $provide.service("artifactRelationships", ArtifactRelationshipsService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
    }));

    beforeEach(inject(() => {
        directiveTest = new ComponentTest<BPUtilityPanelController>(template, "bp-utility-panel");
        vm = directiveTest.createComponent({});
    }));

    afterEach(() => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".utility-panel-title").length).toBe(1);
        expect(directiveTest.element.find("bp-history-panel").length).toBe(1);
        expect(directiveTest.element.find("bp-discussion-panel").length).toBe(1);
        expect(directiveTest.element.find("bp-attachments-panel").length).toBe(1);
    });

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService, selectionManager: ISelectionManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifactModel = {id: 22, name: "Artifact", predefinedType: ItemTypePredefined.CollectionFolder, prefix: "My"};
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            const selectedArtifact = selectionManager.getArtifact();

            // Assert
            expect(selectedArtifact).toBeDefined();
            expect(selectedArtifact.id).toBe(22);
            expect(vm.itemDisplayName).toBe("My22: Artifact");
        }));

    it("should not hide all tabs for baselines and reviews folder",
        inject(($rootScope: ng.IRootScopeService, selectionManager: ISelectionManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22,
                predefinedType: ItemTypePredefined.BaselineFolder
            });

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            expect(vm.isAnyPanelVisible).toBe(true);
        }));

    it("should hide all tabs for collections folder",
        inject(($rootScope: ng.IRootScopeService, selectionManager: ISelectionManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22,
                predefinedType: ItemTypePredefined.CollectionFolder
            });

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            expect(vm.isAnyPanelVisible).toBe(false);
        }));

    it("should hide all tabs for Project",
        inject(($rootScope: ng.IRootScopeService, selectionManager: ISelectionManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22,
                predefinedType: ItemTypePredefined.Project
            });

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            expect(vm.isAnyPanelVisible).toBe(false);
        }));

});
