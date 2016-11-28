import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "rx/dist/rx.lite";
import "../../";
import {ComponentTest} from "../../../util/component.test";
import {BPHistoryPanelController} from "./bp-history-panel";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";
import {ArtifactManagerMock} from "../../../managers/artifact-manager/artifact-manager.mock";
import {ArtifactHistoryMock} from "./artifact-history.mock";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {IStatefulArtifact, StatefulArtifact} from "../../../managers/artifact-manager";
import {IArtifactHistoryVersion} from "./artifact-history.svc";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {ItemTypePredefined} from "../../../main/models/enums";
import {StatefulArtifactServices} from "../../../managers/artifact-manager/services";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {Models} from "../../../main";
import {StatefulSubArtifact} from "../../../managers/artifact-manager/sub-artifact";
import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";

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
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((artifactManager: ArtifactManagerMock, selectionManager: ISelectionManager) => {
        artifactManager.selection = selectionManager;
        const template = `<bp-history-panel></bp-history-panel>`;
        directiveTest = new ComponentTest<BPHistoryPanelController>(template, "bp-history-panel");
        vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
    }));

    afterEach( () => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".filter-bar").length).toBe(0);
        expect(directiveTest.element.find(".empty-state").length).toBe(1);
    });

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService,
            selectionManager: ISelectionManager,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService) => {

            //Arrange
            const services = new StatefulArtifactServices($q, null, null, null, null, null, artifactService, null, null, null, null, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", predefinedType: ItemTypePredefined.Collections, version: 1}, services);

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            const selectedArtifact = selectionManager.getArtifact();

            //Assert
            expect(selectedArtifact).toBeDefined();
            expect(vm.artifactHistoryList.length).toBe(11);
        }));

    it("should get more historical versions along with a draft", inject(($timeout: ng.ITimeoutService) => {
        //Arrange
        vm.artifactHistoryList = [{
            "versionId": 52,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-06-06T13:58:24.557",
            "artifactState": Models.ArtifactStateEnum.Published
        }];
        vm.loadMoreHistoricalVersions();
        $timeout.flush();

        //Assert
        expect(vm.artifactHistoryList.length).toBe(12);
    }));

    it("should get empty list because it already has version 1", inject(($timeout: ng.ITimeoutService) => {
        //Arrange
        vm.artifactHistoryList = [{
            "versionId": 1,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-06-06T13:58:24.557",
            "artifactState": Models.ArtifactStateEnum.Published
        }];
        vm.loadMoreHistoricalVersions();

        //Assert
        expect(vm.artifactHistoryList.length).toBe(1);
    }));

    it("should get list in ascending order if the flag is set", inject(($timeout: ng.ITimeoutService) => {
        //Arrange
        vm.sortAscending = true;
        vm.changeSortOrder();
        $timeout.flush();

        //Assert
        expect(vm.artifactHistoryList.length).toBe(11);
    }));

    it("should select specified artifact version", inject(($timeout: ng.ITimeoutService) => {
       //Arrange
       const artifact = {
           "versionId": 1,
           "userId": 1,
           "displayName": "admin",
           "hasUserIcon": false,
           "timestamp": "2016-06-06T13:58:24.557",
           "artifactState" : Models.ArtifactStateEnum.Published
       };
       vm.artifactHistoryList = [artifact];
       vm.selectArtifactVersion(artifact);

       //Assert
       expect(vm.selectedVersionId).toBe(artifact.versionId);
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
