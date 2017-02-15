import * as angular from "angular";
import ".";
import "angular-mocks";
import {ItemInfoServiceMock} from "../../../commonModule/itemInfo/itemInfo.service.mock";
import {ILoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {LoadingOverlayServiceMock} from "../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {NavigationServiceMock} from "../../../commonModule/navigation/navigation.service.mock";
import {CollectionServiceMock} from "../../../editorsModule/collection/collection.service.mock";
import {MetaDataServiceMock} from "../../../managers/artifact-manager/metadata/metadata.svc.mock";
import {MainBreadcrumbServiceMock} from "../bp-page-content/mainbreadcrumb.svc.mock";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {IItemChangeSet} from "../../../managers/artifact-manager/changeset/changeset";
import {IArtifactState} from "../../../managers/artifact-manager/state/state";
import {IProjectManager} from "../../../managers/project-manager/project-manager";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {LockedByEnum} from "../../models/enums";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";
import {IMainWindow, IWindowManager} from "../../services/window-manager";
import {MessageServiceMock} from "../messages/message.mock";
import {OpenImpactAnalysisAction} from "./actions/open-impact-analysis-action";
import {BpArtifactInfoController} from "./bp-artifact-info";
import {ProjectExplorerServiceMock} from "../bp-explorer/project-explorer.service.mock";

xdescribe("BpArtifactInfo", () => {
    let $compile: ng.ICompileService;
    let $q: ng.IQService;
    let $rootScope: ng.IRootScopeService;
    let windowManager: IWindowManager;
    let selectionManager: ISelectionManager;
    let projectManager: IProjectManager;
    let loadingOverlayService: ILoadingOverlayService;
    let mainWindowSubject: Rx.BehaviorSubject<IMainWindow>;
    let artifactSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    let validationSubject: Rx.Subject<number[]>;
    let stateSubject: Rx.BehaviorSubject<IArtifactState>;
    let propertySubject: Rx.BehaviorSubject<IItemChangeSet>;

    beforeEach(angular.mock.module("bp.components.artifactinfo"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        mainWindowSubject = new Rx.BehaviorSubject<IMainWindow>(<IMainWindow>{});
        artifactSubject = new Rx.BehaviorSubject<IStatefulArtifact>(undefined);
        validationSubject = new Rx.Subject<number[]>();
        stateSubject = new Rx.BehaviorSubject<IArtifactState>(undefined);
        propertySubject = new Rx.BehaviorSubject<IItemChangeSet>(undefined);

        windowManager = <IWindowManager>{
            mainWindow: mainWindowSubject.asObservable()
        };

        const artifactObservable = artifactSubject.asObservable();
        const validationObservable =  validationSubject.asObservable();
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
            getValidationObservable: () => validationObservable,
            getPropertyObservable: () => propertyObservable
        };
        selectionManager = <ISelectionManager>{
            artifactObservable: artifactObservable,
            getArtifact: () => artifact
        };



        $provide.service("messageService", MessageServiceMock);
        $provide.service("windowManager", () => windowManager);
        $provide.service("selectionManager", () => selectionManager);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
        $provide.service("metadataService", MetaDataServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
        $provide.service("collectionService", CollectionServiceMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
    }));

    beforeEach(inject((
        _$compile_: ng.ICompileService,
        _$q_: ng.IQService,
        _$rootScope_: ng.IRootScopeService,
        _projectManager_: IProjectManager,
        _loadingOverlayService_: ILoadingOverlayService,
        _itemInfoService_: ItemInfoServiceMock
        ) => {
        $compile = _$compile_;
        $q = _$q_;
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
            selectionManager.getArtifact = () => undefined;
            const artifactSpy = spyOn(selectionManager.artifactObservable, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(artifactSpy).not.toHaveBeenCalled();
        });

        it("registers artifact loaded handler", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            const spy = spyOn(selectionManager.artifactObservable, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("registers artifact state change handler", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            const spy = spyOn(selectionManager.getArtifact().artifactState.onStateChange, "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("registers artifact state change handler", () => {
            // arrange
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            const spy = spyOn(selectionManager.getArtifact().getPropertyObservable(), "subscribeOnNext").and.callThrough();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("adds Open Impact Analysis action for other artifacts", () => {
            // arrange
            const artifact = selectionManager.getArtifact();
            artifact.predefinedType = ItemTypePredefined.Process;
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();

            // act
            const controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;

            // assert
            expect(controller.toolbarActions.filter(action => action instanceof OpenImpactAnalysisAction).length).toBeGreaterThan(0);
        });

        it("shows the artifact toolbar for live artifact", () => {
            // arrange
            const artifact = selectionManager.getArtifact();
            artifact.predefinedType = ItemTypePredefined.TextualRequirement;
            const template = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();

            // act
            const element: ng.IAugmentedJQuery = $compile(template)(scope);
            const controller = element.controller("bpArtifactInfo") as BpArtifactInfoController;
            scope.$digest();

            // assert
            expect(element[0].querySelectorAll(".toolbar__container").length).toBeGreaterThan(0);
        });

        it("doesn't show the artifact toolbar for deleted artifact", () => {
            // arrange
            const artifact = selectionManager.getArtifact();
            artifact.predefinedType = ItemTypePredefined.TextualRequirement;
            artifact.artifactState.deleted = true;
            const template = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();

            // act
            const element: ng.IAugmentedJQuery = $compile(template)(scope);
            const controller = element.controller("bpArtifactInfo") as BpArtifactInfoController;
            scope.$digest();

            // assert
            expect(element[0].querySelectorAll(".toolbar__container").length).toBe(0);
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
                const historicalArtifact = selectionManager.getArtifact();
                historicalArtifact.lastEditedBy = {displayName: "Author"};
                historicalArtifact.lastEditedOn = new Date();
                historicalArtifact.version = 7;
                historicalArtifact.artifactState.historical = true;
                historicalArtifact.artifactState.deleted = false;

                // act
                artifactSubject.onNext(historicalArtifact);

                // assert
                expect(controller.historicalMessage).toBeDefined();
            });

            it("doesn't add historical message for deleted artifact", () => {
                // arrange
                const deletedArtifact = selectionManager.getArtifact();
                deletedArtifact.lastEditedBy = {displayName: "Author"};
                deletedArtifact.lastEditedOn = new Date();
                deletedArtifact.artifactState.historical = true;
                deletedArtifact.artifactState.deleted = true;

                // act
                artifactSubject.onNext(deletedArtifact);

                // assert
                expect(controller.historicalMessage).toBeNull();
            });

            it("doesn't add historical message for live artifact", () => {
                // arrange
                const liveArtifact = selectionManager.getArtifact();

                // act
                artifactSubject.onNext(liveArtifact);

                // assert
                expect(controller.historicalMessage).toBeNull();
            });

            it("updates artifact name", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.name = "Up-to-date";

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactName).toEqual(updatedArtifact.name);
            });

            it("updates artifact type", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.itemTypeName = "Textual Requirement";

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactType).toEqual(updatedArtifact.itemTypeName);
            });

            it("updates artifact type id", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.itemTypeId = 123;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactTypeId).toEqual(updatedArtifact.itemTypeId);
            });

            it("updates artifact type description", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.prefix = "TR";
                updatedArtifact.itemTypeName = "Textual Requirement";

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactTypeDescription).toEqual(`${updatedArtifact.itemTypeName} - ${(updatedArtifact.prefix || "")}${updatedArtifact.id}`);
            });

            it("updates artifact class of collection folder", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.itemTypeId = ItemTypePredefined.Collections;
                updatedArtifact.predefinedType = ItemTypePredefined.CollectionFolder;
                const expectedArtifactClass = `icon-${_.kebabCase(ItemTypePredefined[ItemTypePredefined.Collections])}`;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactClass).toEqual(expectedArtifactClass);
            });

            it("updates artifact class of baselines and reviews folder", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.itemTypeId = ItemTypePredefined.BaselinesAndReviews;
                updatedArtifact.predefinedType = ItemTypePredefined.BaselineFolder;
                const expectedArtifactClass = `icon-${_.kebabCase(ItemTypePredefined[ItemTypePredefined.BaselinesAndReviews])}`;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactClass).toEqual(expectedArtifactClass);
            });

            it("updates artifact class of general artifact", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
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
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.itemTypeIconId = 456;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.artifactTypeIconId).toEqual(updatedArtifact.itemTypeIconId);
            });

            it("sets hasCustomIcon to true when item type icon id is present", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.itemTypeIconId = 456;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.hasCustomIcon).toEqual(true);
            });

            it("sets hasCustomIcon to false when item type icon id is not present", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.hasCustomIcon).toEqual(false);
            });

            it("sets isLegacy to true when artifact is not Process", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.predefinedType = ItemTypePredefined.Glossary;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.isLegacy).toEqual(true);
            });

            it("sets isLegacy to false when artifact is Process", () => {
                // arrange
                const updatedArtifact = selectionManager.getArtifact();
                updatedArtifact.predefinedType = ItemTypePredefined.Process;

                // act
                artifactSubject.onNext(updatedArtifact);

                // assert
                expect(controller.isLegacy).toEqual(false);
            });

            it("sets noPermissions to false if current user has permissions to edit", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.permissions = 8159;

                // act
                artifactSubject.onNext(artifact);

                // assert
                expect(controller.noPermissions).toEqual(false);
            });

            it("sets noPermissions to true if current user has no permissions to edit", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.permissions = 9;

                // act
                artifactSubject.onNext(artifact);

                // assert
                expect(controller.noPermissions).toEqual(true);
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

            it("doesn't add lockedMessage if artifact is not locked", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.None;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isLocked).toEqual(false);
                expect(controller.lockedMessage).toBeNull();
            });

            it("sets selfLocked to true if locked by current user", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.CurrentUser;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isLocked).toEqual(true);
                expect(controller.selfLocked).toEqual(true);
            });

            it("doesn't add lockedMessage if artifact locked by current user", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.CurrentUser;
                updatedState.lockDateTime = new Date();
                updatedState.lockOwner = "Current User";

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockedMessage).toBeNull();
            });

            it("sets selfLocked to false if locked by another user", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.OtherUser;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isLocked).toEqual(true);
                expect(controller.selfLocked).toEqual(false);
            });

            it("adds lockedMessage if artifact locked by another user", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.OtherUser;
                updatedState.lockDateTime = new Date();
                updatedState.lockOwner = "Another User";

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockedMessage).toBeDefined();
                expect(controller.lockedMessage).toBe("Artifact_InfoBanner_LockedByOn");
            });

            it("adds lockedMessage if artifact locked by another user (no datetime info)", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.OtherUser;
                updatedState.lockOwner = "Another User";

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockedMessage).toBeDefined();
                expect(controller.lockedMessage).toBe("Artifact_InfoBanner_LockedBy");
            });

            it("adds lockedMessage if artifact locked by another user (no user info)", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.OtherUser;
                updatedState.lockDateTime = new Date();

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockedMessage).toBeDefined();
                expect(controller.lockedMessage).toBe("Artifact_InfoBanner_LockedOn");
            });

            it("adds lockedMessage if artifact locked by another user (no additional info)", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.lockedBy = LockedByEnum.OtherUser;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.lockedMessage).toBeDefined();
                expect(controller.lockedMessage).toBe("Artifact_InfoBanner_Locked");
            });

            it("sets isReadonly to true if artifact is read-only", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.readonly = true;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isReadonly).toEqual(true);
            });

            it("sets isReadonly to false if artifact is not read-only", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.readonly = false;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isReadonly).toEqual(false);
            });

            it("sets isDeleted to true if artifact has been deleted", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.deleted = true;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isDeleted).toEqual(true);
            });

            it("adds deletedMessage if artifact has been deleted", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.deleted = true;
                updatedState.deletedById = 10;
                updatedState.deletedDateTime = new Date();
                updatedState.deletedByDisplayName = "Another User";

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.deletedMessage).toBeDefined();
                expect(controller.deletedMessage).toBe("Artifact_InfoBanner_DeletedByOn");
            });

            it("sets isDeleted to false if artifact has not been deleted", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.deleted = false;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isDeleted).toEqual(false);
            });

            it("doesn't add deletedMessage if artifact has not been deleted", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.deleted = false;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.deletedMessage).toBeNull();
            });

            it("sets isChanged to true if artifact is dirty", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.dirty = true;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isChanged).toEqual(true);
            });

            it("sets isChanged to false if artifact is not dirty", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedState = artifact.artifactState;
                updatedState.dirty = false;

                // act
                stateSubject.onNext(updatedState);

                // assert
                expect(controller.isChanged).toEqual(false);
            });
        });

        describe("on property change", () => {
            it("sets name to new value when updated", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                const updatedArtifact = _.clone(artifact);
                updatedArtifact.name = "Test";

                // act
                propertySubject.onNext({item: updatedArtifact});

                // assert
                expect(controller.artifactName).toEqual(updatedArtifact.name);
            });

            it("ignores update if no update to name", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
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
                spyOn(selectionManager, "getArtifact").and.returnValue(undefined);

                // act
                const result = controller.canLoadProject;

                // assert
                expect(result).toEqual(false);
            });

            it("returns false when selected artifact doesn't specify project information", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.projectId = undefined;

                // act
                const result = controller.canLoadProject;

                // assert
                expect(result).toEqual(false);
            });

            it("returns false for artifact from open project", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.projectId = 34;
                spyOn(projectManager, "getProject").and.returnValue({});

                // act
                const result = controller.canLoadProject;

                // assert
                expect(result).toEqual(false);
            });

            it("returns true for artifact from closed project", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.projectId = 34;
                spyOn(projectManager, "getProject").and.returnValue(undefined);

                // act
                const result = controller.canLoadProject;

                // assert
                expect(result).toEqual(true);
            });
        });

        describe("loadProject", () => {
            it("does not load project when canLoadProject is false", () => {
                // arrange
                spyOn(controller, "canLoadProjectInternal").and.returnValue(false);
                const spy = spyOn(projectManager, "openProjectAndExpandToNode").and.callThrough();

                // act
                controller.loadProject();

                // assert
                expect(spy).not.toHaveBeenCalled();
            });

            it("load project when canLoadProject is true", () => {
                // arrange
                spyOn(controller, "canLoadProjectInternal").and.returnValue(true);
                const spy = spyOn(projectManager, "openProjectAndExpandToNode").and.callThrough();

                // act
                controller.loadProject();

                // assert
                expect(spy).toHaveBeenCalled();
            });

            it("displays loading overlay when started", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.projectId = 34;
                const spy = spyOn(loadingOverlayService, "beginLoading").and.returnValue(undefined);

                // act
                controller.loadProject();

                // assert
                expect(spy).toHaveBeenCalled();
            });

            it("hides loading overlay when completed successfully", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.projectId = 34;
                spyOn(projectManager, "openProjectAndExpandToNode").and.returnValue($q.resolve());
                const spy = spyOn(loadingOverlayService, "endLoading").and.callThrough();

                // act
                controller.loadProject();
                // resolve the promise
                $rootScope.$digest();

                // assert
                expect(spy).toHaveBeenCalled();
            });

            it("hides loading overlay when completed with failure", () => {
                // arrange
                const artifact = selectionManager.getArtifact();
                artifact.projectId = 34;
                spyOn(projectManager, "openProjectAndExpandToNode").and.returnValue($q.reject(new Error()));
                const spy = spyOn(loadingOverlayService, "endLoading").and.callThrough();

                // act
                controller.loadProject();
                // reject the promise
                $rootScope.$digest();

                // assert
                expect(spy).toHaveBeenCalled();
            });
        });
    });
});
