﻿import * as angular from "angular";
import "angular-mocks";
import "rx";
import * as _ from "lodash";
import ".";
import {BpArtifactInfoController} from "./bp-artifact-info";
import {IWindowManager, IMainWindow, ResizeCause} from "../../../main/services/window-manager";
import {IArtifactManager} from "../../../managers/artifact-manager/artifact-manager";
import {IProjectManager} from "../../../managers/project-manager/project-manager";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../core/messages/message.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {LoadingOverlayServiceMock} from "../../../core/loading-overlay/loading-overlay.svc.mock";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";
import {ProjectManagerMock} from "../../../managers/project-manager/project-manager.mock";
import {MetaDataServiceMock} from "../../../managers/artifact-manager/metadata/metadata.svc.mock";
import {MainBreadcrumbServiceMock} from "../../../main/components/bp-page-content/mainbreadcrumb.svc.mock";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {IAnalyticsProvider} from "../analytics/analyticsProvider";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {IArtifactState} from "../../../managers/artifact-manager/state/state";
import {IItemChangeSet} from "../../../managers/artifact-manager/changeset/changeset";
import {ItemTypePredefined, LockedByEnum} from "../../../main/models/enums";

describe("BpArtifactInfo", () => {
    let $compile: ng.ICompileService;
    let $rootScope: ng.IRootScopeService;
    let windowManager: IWindowManager;
    let artifactManager: IArtifactManager;
    let projectManager: IProjectManager;
    let loadingOverlayService: ILoadingOverlayService;
    let analytics: IAnalyticsProvider;
    let mainWindowSubject: Rx.BehaviorSubject<IMainWindow>;
    let artifactSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    let stateSubject: Rx.BehaviorSubject<IArtifactState>;
    let propertySubject: Rx.BehaviorSubject<IItemChangeSet>;

    beforeEach(angular.mock.module("bp.components.artifactinfo"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        mainWindowSubject = new Rx.BehaviorSubject<IMainWindow>(<IMainWindow>{});
        artifactSubject = new Rx.BehaviorSubject<IStatefulArtifact>(undefined);
        stateSubject = new Rx.BehaviorSubject<IArtifactState>(undefined);
        propertySubject = new Rx.BehaviorSubject<IItemChangeSet>(undefined);

        windowManager = <IWindowManager>{
            mainWindow: mainWindowSubject.asObservable()
        };

        const artifactObservable = artifactSubject.asObservable();
        const stateObservable = stateSubject.asObservable();
        stateObservable.debounce = () => stateObservable;
        const propertyObservable = propertySubject.filter(changeSet => !!changeSet).asObservable();
        propertyObservable.distinctUntilChanged = () => propertyObservable;

        const artifact = <IStatefulArtifact>{
            id: 1,
            artifactState: <IArtifactState>{
                readonly: false,
                published: true,
                everPublished: true,
                onStateChange: stateObservable,
                initialize: undefined,
                setState: undefined,
                lock: undefined,
                unlock: undefined,
                dispose: undefined
            },
            getObservable: () => artifactObservable,
            getProperyObservable: () => propertyObservable
        };
        artifactManager = <IArtifactManager>{
            selection: {
                artifactObservable: artifactObservable,
                getArtifact: () => artifact
            }
        };

        analytics = <IAnalyticsProvider>{};

        $provide.service("messageService", MessageServiceMock);
        $provide.service("windowManager", () => windowManager);
        $provide.service("artifactManager", () => artifactManager);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("projectManager", ProjectManagerMock);
        $provide.service("metadataService", MetaDataServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("analytics", () => analytics);
    }));

    beforeEach(inject((
        _$compile_: ng.ICompileService,
        _$rootScope_: ng.IRootScopeService,
        _projectManager_: IProjectManager,
        _loadingOverlayService_: ILoadingOverlayService
        ) => {
        $compile = _$compile_;
        $rootScope = _$rootScope_;
        projectManager = _projectManager_;
        loadingOverlayService = _loadingOverlayService_;
    }));

    describe("on initialization", () => {
        it("registers window resize handler", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            const spy = spyOn(windowManager.mainWindow, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("doesn't register artifact-related handlers if no artifact is selected", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            artifactManager.selection.getArtifact = () => undefined;
            const artifactSpy = spyOn(artifactManager.selection.artifactObservable, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(artifactSpy).not.toHaveBeenCalled();
        });

        it("registers artifact loaded handler", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            const spy = spyOn(artifactManager.selection.artifactObservable, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("registers artifact state change handler", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            const spy = spyOn(artifactManager.selection.getArtifact().artifactState.onStateChange, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("registers artifact state change handler", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            const spy = spyOn(artifactManager.selection.getArtifact().getProperyObservable(), "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(spy).toHaveBeenCalled();
        });
    });

    describe("once initialized", () => {
        let controller: BpArtifactInfoController;

        beforeEach(() => {
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;
        });

        describe("on artifact loaded/reloaded", () => {
            it("adds historical message for historical artifact", () => {
                // arrange
                const historicalArtifact = artifactManager.selection.getArtifact();
                historicalArtifact.lastEditedBy = {displayName: "Author"};
                historicalArtifact.lastEditedOn = new Date();
                historicalArtifact.artifactState.historical = true;
                historicalArtifact.artifactState.deleted = false;

                // act
                artifactSubject.onNext(historicalArtifact);

                // assert
                expect(controller.historicalMessage).toBeDefined();
            });

            it("doesn't add historical message for deleted artifact", () => {
                // arrange
                const deletedArtifact = artifactManager.selection.getArtifact();
                deletedArtifact.lastEditedBy = {displayName: "Author"};
                deletedArtifact.lastEditedOn = new Date();
                deletedArtifact.artifactState.historical = true;
                deletedArtifact.artifactState.deleted = true;

                // act
                artifactSubject.onNext(deletedArtifact);

                // assert
                expect(controller.historicalMessage).not.toBeDefined();
            });

            it("doesn't add historical message for live artifact", () => {
                // arrange
                const liveArtifact = artifactManager.selection.getArtifact();

                // act
                artifactSubject.onNext(liveArtifact);

                // assert
                expect(controller.historicalMessage).not.toBeDefined();
            });

            it("updates artifact name", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.name = "Up-to-date";

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactName).toEqual(updatedArtifact.name);
            });

            it("updates artifact type", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.itemTypeName = "Textual Requirement";

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactType).toEqual(updatedArtifact.itemTypeName);
            });

            it("updates artifact type id", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.itemTypeId = 123;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactTypeId).toEqual(updatedArtifact.itemTypeId);
            });

            it("updates artifact type description", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.prefix = "TR";
                updatedArtifact.itemTypeName = "Textual Requirement";

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactTypeDescription).toEqual(`${updatedArtifact.itemTypeName} - ${(updatedArtifact.prefix || "")}${updatedArtifact.id}`);
            });

            it("updates artifact class of collection folder", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.itemTypeId = ItemTypePredefined.Collections;
                updatedArtifact.predefinedType = ItemTypePredefined.CollectionFolder;
                const expectedArtifactClass = `icon-${_.kebabCase(ItemTypePredefined[ItemTypePredefined.Collections])}`;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactClass).toEqual(expectedArtifactClass);
            });

            it("updates artifact class of general artifact", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.itemTypeId = ItemTypePredefined.TextualRequirement;
                updatedArtifact.predefinedType = ItemTypePredefined.TextualRequirement;
                const expectedArtifactClass = `icon-${_.kebabCase(ItemTypePredefined[updatedArtifact.predefinedType])}`;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactClass).toEqual(expectedArtifactClass);
            });

            it("updates artifact type icon id", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.itemTypeIconId = 456;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactTypeIconId).toEqual(updatedArtifact.itemTypeIconId);
            });

            it("sets hasCustomIcon to true when item type icon id is present", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.itemTypeIconId = 456;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.hasCustomIcon).toEqual(true);
            });

            it("sets hasCustomIcon to false when item type icon id is not present", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.hasCustomIcon).toEqual(false);
            });

            it("sets isLegacy to true when artifact is not Process", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.predefinedType = ItemTypePredefined.Glossary;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.isLegacy).toEqual(true);
            });

            it("sets isLegacy to false when artifact is Process", () => {
                // arrange
                const updatedArtifact = artifactManager.selection.getArtifact();
                updatedArtifact.predefinedType = ItemTypePredefined.Process;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.isLegacy).toEqual(false);
            });
        });

        describe("on artifact state changed", () => {
            it("doesn't update state properties if state is falsy", () => {
                // arrange
                const spy = spyOn(controller, "updateStateProperties");

                // act
                stateSubject.onNext(undefined);

                // assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("sets selfLocked to true if locked by current user", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.CurrentUser;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.selfLocked).toEqual(true);
            });

            it("sets selfLocked to false if locked by another user", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.OtherUser;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.selfLocked).toEqual(false);
            });

            it("sets isReadonly to true if artifact is read-only", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.readonly = true;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isReadonly).toEqual(true);
            });

            it("sets isReadonly to false if artifact is not read-only", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.readonly = false;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isReadonly).toEqual(false);
            });

            it("sets isChanged to true if artifact is dirty", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.dirty = true;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isChanged).toEqual(true);
            });

            it("sets isChanged to false if artifact is not dirty", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.dirty = false;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isChanged).toEqual(false);
            });

            it("adds lockMessage if artifact locked by another user", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.OtherUser;
                updatedState.lockDateTime = new Date();
                updatedState.lockOwner = "Another User";

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockMessage).toBeDefined();
            });

            it("doesn't add lockMessage if artifact locked by current user", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.CurrentUser;
                updatedState.lockDateTime = new Date();
                updatedState.lockOwner = "Current User";

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockMessage).not.toBeDefined();
            });

            it("doesn't add lockMessage if artifact is not locked", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.None;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockMessage).not.toBeDefined();
            });
        });

        describe("on property change", () => {
            it("sets name to new value when updated", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedArtifact = _.clone(artifact);
                updatedArtifact.name = "Test";

                // act
                propertySubject.onNext({item: updatedArtifact});

                // assert
                expect(controller.artifactName).toEqual(updatedArtifact.name);
            });

            it("ignores update if no update to name", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                const updatedArtifact = _.clone(artifact);
                const spy = spyOn(controller, "onArtifactPropertyChanged");

                // act
                propertySubject.onNext({item: updatedArtifact});

                // assert
                expect(spy).not.toHaveBeenCalled();
            });
        });

        describe("canLoadProject", () => {
            it("returns false when no artifact is selected", () => {
                // arrange
                spyOn(artifactManager.selection, "getArtifact").and.returnValue(undefined);

                // act
                const result = controller.canLoadProject;

                // assert
                expect(result).toEqual(false);
            });

            it("returns false when selected artifact doesn't specify project information", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                artifact.projectId = undefined;

                // act
                const result = controller.canLoadProject;

                // assert
                expect(result).toEqual(false);
            });

            it("returns false for artifact from open project", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                artifact.projectId = 34;
                spyOn(projectManager, "getProject").and.returnValue({});

                // act
                const result = controller.canLoadProject;
                
                // assert
                expect(result).toEqual(false);
            });

            it("returns true for artifact from closed project", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                artifact.projectId = 34;
                spyOn(projectManager, "getProject").and.returnValue(undefined);

                // act
                const result = controller.canLoadProject;
                
                // assert
                expect(result).toEqual(true);
            });
        });

        describe("loadProject", () => {
            it("does not load project when no artifact is selected", () => {
                // arrange
                spyOn(artifactManager.selection, "getArtifact").and.returnValue(undefined);
                const spy = spyOn(loadingOverlayService, "beginLoading").and.returnValue(undefined);

                // act
                controller.loadProject();

                // assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("does not load project when selected artifact doesn't specify project information", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                artifact.projectId = undefined;
                const spy = spyOn(loadingOverlayService, "beginLoading").and.returnValue(undefined);

                // act
                controller.loadProject();

                // assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("displays loading overlay", () => {
                // arrange
                const artifact = artifactManager.selection.getArtifact();
                artifact.projectId = 34;
                const spy = spyOn(loadingOverlayService, "beginLoading").and.returnValue(undefined);

                // act
                controller.loadProject();

                // assert
                expect(spy).toHaveBeenCalled();
            });
        });
    });
});
