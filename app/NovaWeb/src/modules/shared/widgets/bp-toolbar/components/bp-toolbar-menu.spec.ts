require("script!mxClient");
import * as angular from "angular";
import "angular-mocks";
import "../";
import "rx";
import {createUserTask} from "../../../../editorsModule/bp-process/models/test-shape-factory";
import {UserTask} from "../../../../editorsModule/bp-process/components/diagram/presentation/graph/shapes/user-task";
import {IDiagramNode} from "../../../../editorsModule/bp-process/components/diagram/presentation/graph/models";
import {ProcessEvents} from "../../../../editorsModule/bp-process/components/diagram/process-diagram-communication";
import {ILocalizationService} from "../../../../commonModule/localization";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {CommunicationManager, ICommunicationManager} from "../../../../editorsModule/bp-process/services/communication-manager";
import {IArtifact} from "../../../../main/models/models";
import {StatefulProcessArtifact} from "../../../../managers/artifact-manager/artifact";
import {CopyAction} from "../../../../editorsModule/bp-process/components/header/actions";
import {BPToolbarMenuController} from "./bp-toolbar-menu";

describe("BPToolbar", () => {
    let $compile: ng.ICompileService;
    let $scope: ng.IScope;
    let $rootScope: ng.IRootScopeService;
    let communicationManager: ICommunicationManager;
    let localization: ILocalizationService;

    beforeEach(angular.mock.module("bp.widgets.toolbar"));
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject((
        _$compile_: ng.ICompileService,
        _$rootScope_: ng.IRootScopeService,
        _communicationManager_: ICommunicationManager,
        _localization_: ILocalizationService) => {
        $compile = _$compile_;
        $scope = _$rootScope_.$new();
        $rootScope = _$rootScope_;
        communicationManager = _communicationManager_;
        localization = _localization_;

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
    }));

    afterEach(() => {
        $compile = null;
        $scope = null;
    });

    it("correctly initializes the bound properties and events", () => {
        // arrange
        const template = `<bp-toolbar-menu actions="actions"></bp-toolbar-menu>`;

        $scope["actions"] = [];

        // act
        const controller = <BPToolbarMenuController>$compile(template)($scope).controller("bpToolbarMenu");

        // assert
        expect(controller.actions).toEqual([]);
    });

    describe("Copy Shapes action button in menu for processes", () => {

        it("correctly compiles copy shape action given process artifact", () => {
            // arrange
            const template = `<bp-toolbar-menu actions="actions"></bp-toolbar-menu>`;
            const artifact: IArtifact = { id: 3 };
            const statefulArtifact = new StatefulProcessArtifact(artifact, null);
            const actions = [
                new CopyAction(statefulArtifact, communicationManager, localization)
            ];
            $scope["actions"] = actions;

            // act
            const controller = <BPToolbarMenuController>$compile(template)($scope).controller("bpToolbarMenu");

            // assert
            expect(controller.actions.length).toEqual(1);
        });

        it("disables copy action initially, but keeps action in list of actions in the menu", () => {
            // arrange
            const template = `<bp-toolbar-menu actions="actions"></bp-toolbar-menu>`;
            const artifact: IArtifact = { id: 3 };
            const statefulArtifact = new StatefulProcessArtifact(artifact, null);
            const actions = [
                new CopyAction(statefulArtifact, communicationManager, localization)
            ];
            $scope["actions"] = actions;

            // act
            const controller = <BPToolbarMenuController>$compile(template)($scope).controller("bpToolbarMenu");

            // assert
            expect(controller.actions.length).toEqual(1);
            expect(controller.actions[0].disabled).toBe(true);
        });

        it("changes disabled copy shapes button to be enabled when user task has been selected", () => {
            // arrange
            const template = `<bp-toolbar-menu actions="actions"></bp-toolbar-menu>`;
            const artifact: IArtifact = { id: 3 };
            const statefulArtifact = new StatefulProcessArtifact(artifact, null);
            const actions = [
                new CopyAction(statefulArtifact, communicationManager, localization)
            ];
            $scope["actions"] = actions;
            const userTask = createUserTask(1, $rootScope);

            // act
            const controller = <BPToolbarMenuController>$compile(template)($scope).controller("bpToolbarMenu");
            expect(controller.actions[0].disabled).toBe(true);
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask])

            // assert
            expect(controller.actions.length).toEqual(1);
            expect(controller.actions[0].disabled).toBe(false);
        });
    });
});
