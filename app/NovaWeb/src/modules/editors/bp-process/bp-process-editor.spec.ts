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
import {IDiagramNode} from "./components/diagram/presentation/graph/models/";

describe("BpProcessEditor", () => {
    let $q: ng.IQService;
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

    describe("on selection manager sub-artifact selection change", () => {
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

    describe("on diagram selection change", () => {
        it("sets sub-artifact selection when a shape is selected in the diagram", () => {
            // arrange
            const shape = <IDiagramNode>{model: {id: 345}};
            const selectedShapes = [shape];
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const subArtifact = {id: 345, loadProperties: () => {
                return $q.resolve(subArtifact);
            }};

            spyOn(controller.artifact.subArtifactCollection, "get").and.returnValue(subArtifact);
            const spy = spyOn(artifactManager.selection, "setSubArtifact").and.callThrough();

            // act
            controller["processDiagram"]["selectionListeners"][0](selectedShapes);
            $rootScope.$digest(); // resolve a promise

            // assert
            expect(spy).toHaveBeenCalledWith(subArtifact);
        });

        it("clears sub-artifact selection when no shapes are selected in the diagram", () => {
            // arrange
            const selectedShapes = [];
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;

            const spy = spyOn(artifactManager.selection, "clearSubArtifact").and.callThrough();

            // act
            controller["processDiagram"]["selectionListeners"][0](selectedShapes);

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("doesn't change sub-artifact selection if sub-artifact is no corresponding sub-artifact exists", () => {
            // arrange
            const shape = <IDiagramNode>{model: {id: 345}};
            const selectedShapes = [shape];
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;

            spyOn(controller.artifact.subArtifactCollection, "get").and.returnValue(undefined);
            const spy = spyOn(artifactManager.selection, "setSubArtifact").and.callThrough();

            // act
            controller["processDiagram"]["selectionListeners"][0](selectedShapes);
            controller.$onDestroy();
            $rootScope.$digest(); // resolve a promise

            // assert
            expect(spy).not.toHaveBeenCalled();
        });

        it("doesn't change sub-artifact selection if editor is already destroyed", () => {
            // arrange
            const shape = <IDiagramNode>{model: {id: 345}};
            const selectedShapes = [shape];
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const listener = controller["processDiagram"]["selectionListeners"][0];
            controller.$onDestroy();

            const setSpy = spyOn(artifactManager.selection, "setSubArtifact").and.callThrough();
            const clearSpy = spyOn(artifactManager.selection, "clearSubArtifact").and.callThrough();

            // act
            listener(selectedShapes);

            // assert
            expect(setSpy).not.toHaveBeenCalled();
            expect(clearSpy).not.toHaveBeenCalled();
        });

        it("doesn't change sub-artifact selection if editor is destroyed while properties are loaded", () => {
            // arrange
            const shape = <IDiagramNode>{model: {id: 345}};
            const selectedShapes = [shape];
            const element = "<bp-process-editor></bp-process-editor>";
            const scope = $rootScope.$new();
            const controller = $compile(element)(scope).controller("bpProcessEditor") as BpProcessEditorController;
            const subArtifact = {id: 345, loadProperties: () => {
                return $q.resolve(subArtifact);
            }};

            spyOn(controller.artifact.subArtifactCollection, "get").and.returnValue(subArtifact);
            const spy = spyOn(artifactManager.selection, "setSubArtifact").and.callThrough();

            // act
            controller["processDiagram"]["selectionListeners"][0](selectedShapes);
            controller.$onDestroy();
            $rootScope.$digest(); // resolve a promise

            // assert
            expect(spy).not.toHaveBeenCalled();
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
