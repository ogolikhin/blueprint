import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../util/component.test";
import {BPUtilityPanelController} from "./bp-utility-panel";
import {PanelType} from "./utility-panel.svc";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {ArtifactHistoryMock} from "./bp-history-panel/artifact-history.mock";
import {SelectionManager} from "../../managers/selection-manager/selection-manager";
import {ItemTypePredefined, ReuseSettings} from "../../main/models/enums";
import {ArtifactService} from "../../managers/artifact-manager/artifact/artifact.svc";
import {ArtifactManager, IArtifactManager} from "../../managers/artifact-manager/artifact-manager";
import {ArtifactAttachmentsService} from "../../managers/artifact-manager/attachments/attachments.svc";
import {MetaDataService} from "../../managers/artifact-manager/metadata/metadata.svc";
import {ArtifactRelationshipsService} from "../../managers/artifact-manager/relationships/relationships.svc";
import {
    StatefulArtifactFactory,
    IStatefulArtifactFactory
} from "../../managers/artifact-manager/artifact/artifact.factory";

describe("Component BPUtilityPanel", () => {

    let directiveTest: ComponentTest<BPUtilityPanelController>;
    let template = `<bp-utility-panel></bp-utility-panel>`;
    let vm: BPUtilityPanelController;

    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactService", ArtifactService);
        $provide.service("artifactManager", ArtifactManager);
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
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifactModel = {id: 22, name: "Artifact", predefinedType: ItemTypePredefined.CollectionFolder, prefix: "My"};
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            const selectedArtifact = artifactManager.selection.getArtifact();

            // Assert
            expect(selectedArtifact).toBeDefined();
            expect(selectedArtifact.id).toBe(22);
            expect(vm.itemDisplayName).toBe("My22: Artifact");
        }));

    it("should hide all tabs for collections folder",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22,
                predefinedType: ItemTypePredefined.CollectionFolder
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            expect(vm.isAnyPanelVisible).toBe(false);
        }));

    it("should hide all tabs for Project",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22,
                predefinedType: ItemTypePredefined.Project
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            expect(vm.isAnyPanelVisible).toBe(false);
        }));
        
});
