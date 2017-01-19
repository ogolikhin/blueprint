import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import ".";
import "rx/dist/rx.lite.js";

import {BpProcessEditorController} from "./bp-process-editor";
import {LocalizationServiceMock} from "../../core/localization/localization.service.mock";
import {DialogServiceMock} from "../../shared/widgets/bp-dialog/bp-dialog.mock";
import {NavigationServiceMock} from "../../core/navigation/navigation.svc.mock";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {IWindowManager, IMainWindow, ResizeCause} from "../../main/services/window-manager";
import {IArtifactManager} from "../../managers/artifact-manager/artifact-manager";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {IStatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact/sub-artifact";
import {IUtilityPanelService} from "../../shell/bp-utility-panel/utility-panel.svc";
import {IFileUploadService} from "../../core/fileUpload/";
import {ILoadingOverlayService} from "../../core/loadingOverlay/";
import {MessageServiceMock} from "../../main/components/messages/message.mock";

describe("BpProcessEditor", () => {
    let $q: ng.IQService;
    let $compile: ng.ICompileService;
    let $rootScope: ng.IRootScopeService;
    let windowManager: IWindowManager;
    let artifactManager: IArtifactManager;
    let mainWindowSubject: Rx.BehaviorSubject<IMainWindow>;
    let artifactSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    let subArtifactSubject: Rx.BehaviorSubject<IStatefulSubArtifact>;
    let utilityPanelService:IUtilityPanelService;
    let fileUploadService:IFileUploadService;
    let loadingOverlayService:ILoadingOverlayService;

    beforeEach(angular.mock.module("bp.editors.process"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        mainWindowSubject = new Rx.BehaviorSubject<IMainWindow>(<IMainWindow>{});
        artifactSubject = new Rx.BehaviorSubject<IStatefulArtifact>(<IStatefulArtifact>{});
        subArtifactSubject = new Rx.BehaviorSubject<IStatefulSubArtifact>(<IStatefulSubArtifact>{});

        windowManager = <IWindowManager>{
            mainWindow: mainWindowSubject.asObservable()
        };

        artifactManager = <IArtifactManager>{
            selection: {
                subArtifactObservable: subArtifactSubject.asObservable(),
                getArtifact: () => <IStatefulArtifact>{
                    id: 1,
                    getObservable: () => artifactSubject.asObservable(),
                    subArtifactCollection: {get: (id: number) => { /* no op */ }}
                },
                setSubArtifact: (subArtifact: IStatefulSubArtifact) => { /* no op */ },
                clearSubArtifact: () => { /* no op */ }
            }
        };
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("windowManager", () => windowManager);
        $provide.service("artifactManager", () => artifactManager);
        $provide.service("utilityPanelService", () => utilityPanelService);
        $provide.service("fileUploadService", () => fileUploadService);
        $provide.service("loadingOverlayService", () => loadingOverlayService);


    }));

    beforeEach(inject((
        _$q_: ng.IQService,
        _$compile_: ng.ICompileService,
         _$rootScope_: ng.IRootScopeService
        ) => {
        $q = _$q_;
        $compile = _$compile_;
        $rootScope = _$rootScope_;
    }));

    describe("on initialization", () => {
        it("registers mainWindow listener", () => {
            // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const mainWindowSpy = spyOn(windowManager.mainWindow, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;

            // assert
            expect(mainWindowSpy).toHaveBeenCalledTimes(1);
        });

    });

    describe("on process loaded/reloaded", () => {
        it("destroys previous process diagram", () => {
            // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const spy = spyOn(controller["processDiagram"], "destroy");

            // act
            artifactSubject.onNext(<IStatefulArtifact>{id: 2});

            // assert
            expect(spy).toHaveBeenCalledTimes(1);
        });
    });

    describe("on resize", () => {
        it("resizes process diagram due to sidebar toggle", () => {
            // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const spy = spyOn(controller["processDiagram"], "resize");
            const height: number = 100;
            const width: number = 50;

            // act
            mainWindowSubject.onNext(<IMainWindow>{
                causeOfChange: ResizeCause.sidebarToggle,
                contentHeight: height,
                contentWidth: width
            });

            // assert
            expect(spy).toHaveBeenCalledWith(width, height);
        });

        it("resizes process diagram due to browser resize", () => {
            // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const spy = spyOn(controller["processDiagram"], "resize");
            const height: number = 0;
            const width: number = 0;

            // act
            mainWindowSubject.onNext(<IMainWindow>{
                causeOfChange: ResizeCause.browserResize
            });

            // assert
            expect(spy).toHaveBeenCalledWith(width, height);
        });
    });

    describe("on destroy", () => {

        it("destroys process diagram", () => {
                        // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const spy = spyOn(controller["processDiagram"], "destroy");

            // act
            controller.$onDestroy();

            // assert
            expect(spy).toHaveBeenCalledTimes(1);
        });
    });
});
