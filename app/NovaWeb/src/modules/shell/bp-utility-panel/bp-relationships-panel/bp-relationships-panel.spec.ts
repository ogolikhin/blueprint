import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../util/component.test";
import { BPRelationshipsPanelController } from "./bp-relationships-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactRelationshipsMock } from "./artifact-relationships.mock";
import { SelectionManager } from "../../../main/services/selection-manager";

describe("Component BPRelationshipsPanel", () => {

    let directiveTest: ComponentTest<BPRelationshipsPanelController>;
    let template = `<bp-relationships-panel></bp-relationships-panel>`;
    let vm: BPRelationshipsPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
    }));

    beforeEach(inject(() => {
        directiveTest = new ComponentTest<BPRelationshipsPanelController>(template, "bp-relationships-panel");
        vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
    }));

    afterEach(() => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".filter-bar").length).toBe(0);
        expect(directiveTest.element.find(".empty-state").length).toBe(1);
    });

    // it("should load data for a selected artifact",
    //     inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager) => {

    //        //Arrange
    //         const project = { id: 2, name: "Project 2" } as Models.IProject;
    //         const artifact = { id: 22, name: "Artifact" } as Models.IArtifact;
            
    //         //Act
    //         selectionManager.selection = { project: project, artifact: artifact, source:  SelectionSource.Explorer };
    //         $rootScope.$digest();
    //         const selectedArtifact = selectionManager.selection.artifact;

    //         //Assert
    //         expect(selectedArtifact).toBeDefined();
    //         expect(vm.artifactList.manualTraces.length).toBe(2);
    //         expect(vm.artifactList.otherTraces.length).toBe(3);
    //     }));
});
