import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import ".";
import {BpProcessEditorController} from "./bp-process-editor";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {DialogServiceMock} from "../../shared/widgets/bp-dialog/bp-dialog";
import {NavigationServiceMock} from "../../core/navigation/navigation.svc.mock";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {IWindowManager, IMainWindow, ResizeCause} from "../../main/services/window-manager";
import {IArtifactManager} from "../../managers/artifact-manager/artifact-manager";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {IStatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact/sub-artifact";

describe("BpProcessEditor", () => {
    let $compile: ng.ICompileService;
    let $rootScope: ng.IRootScopeService;
    let windowManager: IWindowManager;
    let artifactManager: IArtifactManager;
    let mainWindowSubject: Rx.BehaviorSubject<IMainWindow>;
    let artifactSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    let subArtifactSubject: Rx.BehaviorSubject<IStatefulSubArtifact>;

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
                    getObservable: () => artifactSubject.asObservable()
                 }
            }
        };


        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("windowManager", () => windowManager);
        $provide.service("artifactManager", () => artifactManager);
    }));

    beforeEach(inject((
        _$compile_: ng.ICompileService,
         _$rootScope_: ng.IRootScopeService
        ) => {
        $compile = _$compile_;
        $rootScope = _$rootScope_;
    }));

    describe("initialization", () => {
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

        it("registers subArtifact selection listener", () => {
            // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const subArtifactObservableSpy = spyOn(artifactManager.selection.subArtifactObservable, "subscribeOnNext").and.callThrough();
            
            // act
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;

            // assert
            expect(subArtifactObservableSpy).toHaveBeenCalledTimes(1);
        });
    });

    describe("process loaded", () => {
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

    describe("sub-artifact selection handler", () => {
        it("clears process diagram selection when sub-artifact is not selected", () => {
            // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const clearSpy = spyOn(controller["processDiagram"], "clearSelection");

            // act
            subArtifactSubject.onNext(undefined);

            // assert
            expect(clearSpy).toHaveBeenCalledTimes(1);
        });

        it("doesn't clear process diagram selection when sub-artifact is selected", () => {
            // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const clearSpy = spyOn(controller["processDiagram"], "clearSelection");

            // act
            subArtifactSubject.onNext(<IStatefulSubArtifact>{});

            // assert
            expect(clearSpy).not.toHaveBeenCalled();
        });
    });

    describe("resize handler", () => {
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

    describe("destroy", () => {
        it("destroys sub-artifact editor modal opener", () => {
                        // arrange
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const spy = spyOn(controller["subArtifactEditorModalOpener"], "destroy");

            // act
            controller.$onDestroy();
            
            // assert
            expect(spy).toHaveBeenCalledTimes(1);
        });

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
