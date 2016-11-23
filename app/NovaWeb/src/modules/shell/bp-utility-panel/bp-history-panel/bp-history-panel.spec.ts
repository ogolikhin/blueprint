import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "rx/dist/rx.lite";
import "../../";
import {ComponentTest} from "../../../util/component.test";
import {BPHistoryPanelController} from "./bp-history-panel";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";
import {ArtifactManagerMock} from "../../../managers/artifact-manager/artifact-manager.mock";
import {SelectionManager} from "./../../../managers/selection-manager/selection-manager";
import {ArtifactHistoryMock} from "./artifact-history.mock";
import {ISelectionManager, ISelection} from "../../../managers/selection-manager/selection-manager";
import {DialogService} from "../../../shared/widgets/bp-dialog";
import {IStatefulArtifact,
        IStatefulArtifactFactory,
        StatefulArtifactFactory,
        StatefulArtifact}
from "../../../managers/artifact-manager";
import {IArtifactHistory, IArtifactHistoryVersion} from "./artifact-history.svc";
// import {StateManager} from "../../../core/services/state-manager";
// import {Models} from "../../../main/services/project-manager";

describe("Component BPHistoryPanel", () => {

    let directiveTest: ComponentTest<BPHistoryPanelController>;
    let vm: BPHistoryPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("dialogService", DialogServiceMock);
    }));

    beforeEach(inject((
        artifactManager: ArtifactManagerMock,
        selectionManager: ISelectionManager) => {
        artifactManager.selection = selectionManager;
    }));

    beforeEach(() => {
        const template = `<bp-history-panel></bp-history-panel>`;
        directiveTest = new ComponentTest<BPHistoryPanelController>(template, "bp-history-panel");
        vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
    });

    afterEach( () => {
        vm = null;
    });

    xit("should be visible by default", () => {
//         //Assert
//         expect(directiveTest.element.find(".filter-bar").length).toBe(0);
//         expect(directiveTest.element.find(".empty-state").length).toBe(1);
    });

    xit("should load data for a selected artifact", (() => {//
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager) => {

//             //Arrange
//             const artifact = {id: 22, name: "Artifact"} as Models.IArtifact;

//             //Act
//             selectionManager.selection = {artifact: artifact, source:  SelectionSource.Explorer};
//             $rootScope.$digest();
//             const selectedArtifact = selectionManager.selection.artifact;

//             //Assert
//             expect(selectedArtifact).toBeDefined();
//             expect(vm.artifactHistoryList.length).toBe(11);
    }));

    xit("should get more historical versions along with a draft", inject(($timeout: ng.ITimeoutService) => {

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
    }));

    xit("should get empty list because it already has version 1", inject(($timeout: ng.ITimeoutService) => {

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
    }));

    xit("should get list in ascending order if the flag is set", inject(($timeout: ng.ITimeoutService) => {

//        //Arrange
//        vm.sortAscending = true;
//        vm.changeSortOrder();
//        $timeout.flush();

//        //Assert
//        expect(vm.artifactHistoryList.length).toBe(11);
    }));

    xit("should select specified artifact version", inject(($timeout: ng.ITimeoutService) => {

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
    }));

    it("should navigate to head version on click", inject((
        $rootScope: ng.IRootScopeService,
        navigationService: INavigationService,
        selectionManager: ISelectionManager) => {
        //Arrange
        const historyVersion = {
            versionId: 2147483647 //head version
        } as IArtifactHistoryVersion;
        const artifact: IStatefulArtifact = <any>{id: 1};
        artifact.getObservable = () => {
            return new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        };
        const navigateToSpy = spyOn(navigationService, "navigateTo");

        //Act
        selectionManager.setArtifact(artifact);
        vm.selectArtifactVersion(historyVersion);
        $rootScope.$digest();
        
        //Assert
        expect(navigateToSpy).toHaveBeenCalledWith({id: 1});
    }));

    it("should navigate to historical version on click", inject((
        $rootScope: ng.IRootScopeService,
        navigationService: INavigationService,
        selectionManager: ISelectionManager) => {
        //Arrange
        const historyVersion = {
            versionId: 10
        } as IArtifactHistoryVersion;
        const artifact: IStatefulArtifact = <any>{id: 1};
        artifact.getObservable = () => {
            return new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        };
        const navigateToSpy = spyOn(navigationService, "navigateTo");

        //Act
        selectionManager.setArtifact(artifact);
        vm.selectArtifactVersion(historyVersion);
        $rootScope.$digest();
        
        //Assert
        expect(navigateToSpy).toHaveBeenCalledWith({id: 1, version: 10});
    }));

    it("should set selected version from navigation state", inject((
        $rootScope: ng.IRootScopeService,
        navigationService: INavigationService,
        selectionManager: ISelectionManager) => {
        //Arrange
        spyOn(navigationService, "getNavigationState").and.returnValue({id: 1, version: 10});
        const artifact: IStatefulArtifact = <any>{id: 1};
        artifact.getObservable = () => {
            return new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        };

        //Act
        selectionManager.setArtifact(artifact);
        $rootScope.$digest();
        
        //Assert
        expect(vm.selectedVersionId).toBe(10);
    }));

});
