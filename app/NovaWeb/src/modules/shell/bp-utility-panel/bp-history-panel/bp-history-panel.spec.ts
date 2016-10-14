// import "angular";
// import "angular-mocks";
// import "angular-sanitize";
// import { ComponentTest } from "../../../util/component.test";
// import { BPHistoryPanelController} from "./bp-history-panel";
// import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
// import { ArtifactHistoryMock } from "./artifact-history.mock";
// import { Models } from "../../../main/services/project-manager";
// import { SelectionManager, SelectionSource } from "../../../main/services/selection-manager";
// import { StateManager } from "../../../core/services/state-manager";
// import { DialogService } from "../../../shared/widgets/bp-dialog";

// describe("Component BPHistoryPanel", () => {

//     let directiveTest: ComponentTest<BPHistoryPanelController>;
//     let template = `<bp-history-panel></bp-history-panel>`;
//     let vm: BPHistoryPanelController;
//     let bpAccordionPanelController = {
//         isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
//     };

//     beforeEach(angular.mock.module("app.shell"));

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("artifactHistory", ArtifactHistoryMock);
//         $provide.service("localization", LocalizationServiceMock);
//         $provide.service("selectionManager", SelectionManager);
//         $provide.service("stateManager", StateManager);
//         $provide.service("dialogService", DialogService);
//     }));

//     beforeEach(inject((selectionManager: SelectionManager) => {
//         directiveTest = new ComponentTest<BPHistoryPanelController>(template, "bp-history-panel");
//         vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
//     }));

//     afterEach( () => {
//         vm = null;
//     });

//     it("should be visible by default", () => {
//         //Assert
//         expect(directiveTest.element.find(".filter-bar").length).toBe(0);
//         expect(directiveTest.element.find(".empty-state").length).toBe(1);
//     });

//     it("should load data for a selected artifact",
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager) => {

//             //Arrange
//             const artifact = { id: 22, name: "Artifact" } as Models.IArtifact;

//             //Act
//             selectionManager.selection = { artifact: artifact, source:  SelectionSource.Explorer };
//             $rootScope.$digest();
//             const selectedArtifact = selectionManager.selection.artifact;

//             //Assert
//             expect(selectedArtifact).toBeDefined();
//             expect(vm.artifactHistoryList.length).toBe(11);
//     }));

//     it("should get more historical versions along with a draft", inject(($timeout: ng.ITimeoutService) => {

//        //Arrange
//        vm.artifactHistoryList = [{
//            "versionId": 52,
//            "userId": 1,
//            "displayName": "admin",
//            "hasUserIcon": false,
//            "timestamp": "2016-06-06T13:58:24.557",
//            "artifactState" : Models.ArtifactStateEnum.Published
//        }];
//        vm.loadMoreHistoricalVersions();
//        $timeout.flush();

//        //Assert
//        expect(vm.artifactHistoryList.length).toBe(12);
//     }));

//     it("should get empty list because it already has version 1", inject(($timeout: ng.ITimeoutService) => {

//        //Arrange
//        vm.artifactHistoryList = [{
//            "versionId": 1,
//            "userId": 1,
//            "displayName": "admin",
//            "hasUserIcon": false,
//            "timestamp": "2016-06-06T13:58:24.557",
//            "artifactState" : Models.ArtifactStateEnum.Published
//        }];
//        vm.loadMoreHistoricalVersions();

//        //Assert
//        expect(vm.artifactHistoryList.length).toBe(1);
//     }));

//     it("should get list in ascending order if the flag is set", inject(($timeout: ng.ITimeoutService) => {

//        //Arrange
//        vm.sortAscending = true;
//        vm.changeSortOrder();
//        $timeout.flush();

//        //Assert
//        expect(vm.artifactHistoryList.length).toBe(11);
//     }));

//     it("should select specified artifact version", inject(($timeout: ng.ITimeoutService) => {

//        //Arrange
//        let artifact = {
//            "versionId": 1,
//            "userId": 1,
//            "displayName": "admin",
//            "hasUserIcon": false,
//            "timestamp": "2016-06-06T13:58:24.557",
//            "artifactState" : Models.ArtifactStateEnum.Published
//        };
//        vm.artifactHistoryList = [artifact];
//        vm.selectedArtifactVersion = null;
//        vm.selectArtifactVersion(artifact);

//        //Assert
//        expect(vm.selectedArtifactVersion).toBe(artifact);
//     }));

// });
