import * as angular from "angular";
import "angular-mocks";
import "rx";
import * as _ from "lodash";
import ".";
import {BpArtifactInfoController} from "./bp-artifact-info";
import {IWindowManager, IMainWindow, ResizeCause} from "../../../main/services/window-manager";
import {IArtifactManager} from "../../../managers/artifact-manager/artifact-manager";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {MessageServiceMock} from "../../../core/messages/message.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog";
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
import {ItemTypePredefined} from "../../../main/models/enums";

describe("BpArtifactInfo", () => {
    let $compile: ng.ICompileService;
    let $rootScope: ng.IRootScopeService;
    let windowManager: IWindowManager;
    let artifactManager: IArtifactManager;
    let analytics: IAnalyticsProvider;
    let mainWindowSubject: Rx.BehaviorSubject<IMainWindow>;
    let artifactSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    let stateSubject: Rx.BehaviorSubject<IArtifactState>;
    let propertySubject: Rx.BehaviorSubject<any>;

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
        const stateObservable = stateSubject.filter(state => !!state).asObservable();
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
        _$rootScope_: ng.IRootScopeService
        ) => {
        $compile = _$compile_;
        $rootScope = _$rootScope_;
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

    describe("on artifact loaded/reloaded", () => {
        let controller: BpArtifactInfoController;

        beforeEach(() => {
            const element = "<bp-artifact-info></bp-artifact-info>";
            const scope = $rootScope.$new();
            controller = $compile(element)(scope).controller("bpArtifactInfo") as BpArtifactInfoController;
        });

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
});
