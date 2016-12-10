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
import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {PanelType, IOnPanelChangesObject} from "../utility-panel.svc";
import {ArtifactStateEnum} from "../../../main/models/models";

describe("Component BPHistoryPanel", () => {

    let directiveTest: ComponentTest<BPHistoryPanelController>;
    let vm: BPHistoryPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };
    let onChangesObj: IOnPanelChangesObject;

    beforeEach(angular.mock.module("ui.router"));
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
        onChangesObj = {
            context: {
                currentValue: {
                    panelType: PanelType.History
                },
                previousValue: undefined,
                isFirstChange: () => { return true; }
            }
        };
    }));

    afterEach( () => {
        vm = undefined;
        onChangesObj = undefined;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".filter-bar").length).toBe(0);
        expect(directiveTest.element.find(".empty-state").length).toBe(1);
    });

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService) => {

            //Arrange
            const services = new StatefulArtifactServices($q, null, null, null, null, null, artifactService, null, null, null, null, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", predefinedType: ItemTypePredefined.Collections, version: 1}, services);
            onChangesObj.context.currentValue.artifact = artifact;
            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();

            //Assert
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
            "artifactState": ArtifactStateEnum.Published
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
            "artifactState": ArtifactStateEnum.Published
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

    xit("should navigate to head version on click", inject((
        $rootScope: ng.IRootScopeService,
        $state: ng.ui.IStateService,
        $timeout: ng.ITimeoutService,
        navigationService: INavigationService) => {
        //Arrange
        const historyVersion = {
            versionId: 2147483647 //head version
        } as IArtifactHistoryVersion;
        const artifact: IStatefulArtifact = <any>{id: 1};
        artifact.getObservable = () => {
            return new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        };
        const navigateToSpy = spyOn(navigationService, "navigateTo");
        onChangesObj.context.currentValue.artifact = artifact;

        const stateSpy = spyOn($state, "go");
        $state.current.name = "";

        //Act
        vm.$onChanges(onChangesObj);
        // vm.selectArtifactVersion(historyVersion);
        $rootScope.$digest();

        //Assert
        expect(stateSpy).toHaveBeenCalledWith({id: 1});
    }));

    xit("should navigate to historical version on click", inject((
        $rootScope: ng.IRootScopeService,
        navigationService: INavigationService) => {
        //Arrange
        const historyVersion = {
            versionId: 10
        } as IArtifactHistoryVersion;
        const artifact: IStatefulArtifact = <any>{id: 1};
        artifact.getObservable = () => {
            return new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        };
        const navigateToSpy = spyOn(navigationService, "navigateTo");
        onChangesObj.context.currentValue.artifact = artifact;

        //Act
        vm.$onChanges(onChangesObj);
        // vm.selectArtifactVersion(historyVersion);
        $rootScope.$digest();

        //Assert
        expect(navigateToSpy).toHaveBeenCalledWith({id: 1, version: 10});
    }));

    xit("should set selected version from navigation state", inject((
        $rootScope: ng.IRootScopeService,
        navigationService: INavigationService) => {
        //Arrange
        spyOn(navigationService, "getNavigationState").and.returnValue({id: 1, version: 10});
        const artifact: IStatefulArtifact = <any>{id: 1};
        artifact.getObservable = () => {
            return new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        };
        onChangesObj.context.currentValue.artifact = artifact;

        //Act
        vm.$onChanges(onChangesObj);

        //Assert
        // expect(vm.selectedVersionId).toBe(10);
    }));
});
