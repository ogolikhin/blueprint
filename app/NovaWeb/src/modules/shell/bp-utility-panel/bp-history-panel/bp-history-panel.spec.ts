import "angular-mocks";
import "angular-sanitize";
import "angular-ui-router";
import "rx/dist/rx.lite";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {ArtifactStateEnum} from "../../../main/models/models";
import {IStatefulArtifact, StatefulArtifact} from "../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {StatefulArtifactServices} from "../../../managers/artifact-manager/services";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {Helper} from "../../../shared/utils/helper";
import {ComponentTest} from "../../../util/component.test";
import {IOnPanelChangesObject, PanelType} from "../utility-panel.svc";
import {ArtifactHistoryMock} from "./artifact-history.mock";
import {IArtifactHistory} from "./artifact-history.svc";
import {BPHistoryPanelController} from "./bp-history-panel";
import * as angular from "angular";

describe("Component BPHistoryPanel", () => {
    let $q: ng.IQService;
    let component: ComponentTest<BPHistoryPanelController>;
    let vm: BPHistoryPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };
    let onChangesObj: IOnPanelChangesObject;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((selectionManager: ISelectionManager,
                       _$q_: ng.IQService) => {

        $q = _$q_;
        const template = `<bp-history-panel></bp-history-panel>`;
        component = new ComponentTest<BPHistoryPanelController>(template, "bp-history-panel");
        vm = component.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
        onChangesObj = {
            context: {
                currentValue: {
                    panelType: PanelType.History
                },
                previousValue: undefined,
                isFirstChange: () => true
            }
        };
    }));

    afterEach( () => {
        vm = undefined;
        onChangesObj = undefined;
    });

    it("should be visible by default", () => {
        //Assert
        expect(component.element.find(".filter-bar").length).toBe(0);
        expect(component.element.find(".empty-state").length).toBe(1);
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

    it("should get more historical versions along with a draft",
        inject(($timeout: ng.ITimeoutService, artifactHistory: IArtifactHistory) => {

        //Arrange
        const historySpy = spyOn(artifactHistory, "getArtifactHistory")
            .and.returnValue($q.resolve([{
                versionId: 1
            }]));
        vm.artifactHistoryList = [{
            versionId: 2,
            userId: 1,
            displayName: "admin",
            hasUserIcon: false,
            timestamp: "2016-06-06T13:58:24.557",
            artifactState: ArtifactStateEnum.Published
        }];

        // Act
        vm.loadMoreHistoricalVersions();
        $timeout.flush();

        //Assert
        expect(vm.artifactHistoryList.length).toBe(2);
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

    it("should navigate to head version on click", inject((
        $rootScope: ng.IRootScopeService,
        $state: ng.ui.IStateService,
        artifactHistory: IArtifactHistory,
        $timeout: ng.ITimeoutService) => {

        //Arrange
        const artifact = <IStatefulArtifact>{id: 1};
        artifact.getObservable = () => new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        onChangesObj.context.currentValue.artifact = artifact;
        const historySpy = spyOn(artifactHistory, "getArtifactHistory")
            .and.returnValue($q.resolve([{
                versionId: Helper.draftVersion
            }]));
        const stateSpy = spyOn($state, "go");
        $state.current.name = "";

        //Act
        vm.$onChanges(onChangesObj);
        $rootScope.$digest();
        component.element.find("a").click();
        $timeout.flush();

        //Assert
        expect(stateSpy).toHaveBeenCalledWith("main.item", {id: 1, version: undefined}, jasmine.any(Object));
    }));

    it("should navigate to historical version on click", inject((
        $rootScope: ng.IRootScopeService,
        $state: ng.ui.IStateService,
        artifactHistory: IArtifactHistory,
        $timeout: ng.ITimeoutService) => {

        //Arrange
        const artifact = <IStatefulArtifact>{id: 1};
        artifact.getObservable = () =>  new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        const historySpy = spyOn(artifactHistory, "getArtifactHistory")
            .and.returnValue($q.resolve([{
                versionId: 10
            }]));
        onChangesObj.context.currentValue.artifact = artifact;
        const stateSpy = spyOn($state, "go");
        $state.current.name = "";

        //Act
        vm.$onChanges(onChangesObj);
        $rootScope.$digest();
        component.element.find("a").click();
        $timeout.flush();

        //Assert
        expect(stateSpy).toHaveBeenCalledWith("main.item", {id: 1, version: 10}, jasmine.any(Object));
    }));

    xit("should set selected version from navigation state", inject((
        $rootScope: ng.IRootScopeService,
        $state: ng.ui.IStateService,
        artifactHistory: IArtifactHistory) => {

        //Arrange
        const artifact = <IStatefulArtifact>{id: 1};
        artifact.getObservable = () => new Rx.BehaviorSubject<IStatefulArtifact>(artifact);
        const historySpy = spyOn(artifactHistory, "getArtifactHistory")
            .and.returnValue($q.resolve([{
                id: 1,
                versionId: 10
            }]));
        onChangesObj.context.currentValue.artifact = artifact;
        $state.current.name = "main";

        //Act
        $state.go("main.item", {id: 1, version: 10});
        vm.$onChanges(onChangesObj);
        $rootScope.$digest();
        const selectedHistoryItem = component.element.find(".history-item--selected");

        //Assert
        expect(selectedHistoryItem.length).toBe(1);
    }));
});
