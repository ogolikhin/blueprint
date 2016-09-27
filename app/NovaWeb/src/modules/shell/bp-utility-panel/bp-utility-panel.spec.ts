import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../util/component.test";
import { BPUtilityPanelController, PanelType } from "./bp-utility-panel";
import { LocalizationServiceMock } from "../../core/localization/localization.mock";
import { ArtifactHistoryMock } from "./bp-history-panel/artifact-history.mock";
import { SelectionManager } from "./../../managers/selection-manager/selection-manager";
import { IBpAccordionPanelController } from "../../main/components/bp-accordion/bp-accordion";
import {
    IArtifactManager,
    ArtifactManager,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService,
    ArtifactService,
    ArtifactAttachmentsService,
    ArtifactRelationshipsService }
    from "../../managers/artifact-manager";

describe("Component BPUtilityPanel", () => {

    let directiveTest: ComponentTest<BPUtilityPanelController>;
    let template = `<bp-utility-panel></bp-utility-panel>`;
    let vm: BPUtilityPanelController;

    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistoryMock);
        // $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        // $provide.service("artifactAttachments", ArtifactAttachmentsMock);
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
    
    afterEach( () => {
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
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My"});
            
            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            const selectedArtifact = artifactManager.selection.getArtifact();

            // Assert
            expect(selectedArtifact).toBeDefined();
            expect(selectedArtifact.id).toBe(22);
            expect(vm.currentItem).toBe("My22: Artifact");
        }));

    it("should hide files tab for collections",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My", predefinedType: 4609 });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            const accordionCtrl = vm.getAccordionController();

            // Assert
            expect((<IBpAccordionPanelController>accordionCtrl.getPanels()[PanelType.Files]).isVisible).toBe(false);
        }));
});
