import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../util/component.test";
import { BPRelationshipsPanelController} from "./bp-relationships-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactRelationshipsMock } from "./artifact-relationships.mock";
import { ProjectRepositoryMock } from "../../../main/services/project-repository.mock";
import { ProjectManager, Models } from "../../../main/services/project-manager";

describe("Component BPRelationshipsPanel", () => {

    let directiveTest: ComponentTest<BPRelationshipsPanelController>;
    let template = `<bp-relationships-panel></bp-relationships-panel>`;
    let vm: BPRelationshipsPanelController;
    let bpAccordionPanelController = {
        isOpenObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    beforeEach(inject((projectManager: ProjectManager) => {
        projectManager.initialize();
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

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {

            //Arrange
            projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
            $rootScope.$digest();

            //Act
            let artifact = projectManager.getArtifact(22);

            //Assert
            expect(artifact).toBeDefined();
            expect(vm.artifactList.manualTraces.length).toBe(2);
            expect(vm.artifactList.otherTraces.length).toBe(3);
        }));


});
