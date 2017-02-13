import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {DeleteAction} from "./delete-action";
import {
    IStatefulArtifact,
    IStatefulArtifactFactory
} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {IDialogService} from "../../../../shared";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ItemTypePredefined, RolePermissions} from "../../../models/enums";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {MessageServiceMock} from "../../messages/message.mock";
import {SelectionManager, ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {IMessageService} from "../../messages/message.svc";
import {ProjectExplorerServiceMock} from "../../bp-explorer/project-explorer.service.mock";
import {IProjectExplorerService} from "../../bp-explorer/project-explorer.service";

xdescribe("DeleteAction", () => {
    let $q_: ng.IQService;
    let $scope: ng.IScope;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("navigationService", NavigationServiceMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        $scope = $rootScope.$new();
        $q_ = $q;
    }));

    it("throws exception when localization is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService
    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new DeleteAction(artifact, localization, messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("throws exception when dialogService is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        navigationService: INavigationService
    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const dialogService: IDialogService = null;
        let error: Error = null;

        // act
        try {
            new DeleteAction(artifact, localization, messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Dialog service not provided or is null"));
    }));


    it("is disabled when artifact is null", inject((localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService
    ) => {
        // arrange
        const artifact: IStatefulArtifact = null;

        // act
        const deleteAction = new DeleteAction(artifact,
            localization, messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        // assert
        expect(deleteAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is read-only", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService

    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        artifact.artifactState.readonly = true;

        // act
        const deleteAction = new DeleteAction(artifact, localization,
            messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        // assert
        expect(deleteAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Project", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService

    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                id: 1,
                predefinedType: ItemTypePredefined.Project
            });

        // act
        const deleteAction = new DeleteAction(artifact, localization,
            messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        // assert
        expect(deleteAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Collections", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService

    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                id: 1,
                predefinedType: ItemTypePredefined.Collections
            });

        // act
        const deleteAction = new DeleteAction(artifact, localization,
            messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        // assert
        expect(deleteAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Baselines and Reviews", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService

    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                id: 1,
                predefinedType: ItemTypePredefined.BaselinesAndReviews
            });

        // act
        const deleteAction = new DeleteAction(artifact, localization,
            messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        // assert
        expect(deleteAction.disabled).toBe(true);
    }));

    it("is disabled when artifact has no delete permissions", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService

    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                id: 1,
                predefinedType: ItemTypePredefined.TextualRequirement,
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit
            });

        // act
        const deleteAction = new DeleteAction(artifact, localization,
            messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        // assert
        expect(deleteAction.disabled).toBe(true);
    }));

    it("is enabled when artifact is valid", inject((statefulArtifactFactory: IStatefulArtifactFactory,
        localization: ILocalizationService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        messageService: IMessageService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService

    ) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                id: 1,
                predefinedType: ItemTypePredefined.TextualRequirement,
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit | RolePermissions.Delete
            });

        // act
        const deleteAction = new DeleteAction(artifact, localization,
            messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        // assert
        expect(deleteAction.disabled).toBe(false);
    }));

    describe("when executed", () => {
        let deleteAction: DeleteAction;
        let beginLoadingSpy: jasmine.Spy;
        let deleteSpy: jasmine.Spy;
        let refreshSpy: jasmine.Spy;
        let getDescendantsSpy: jasmine.Spy;
        let endLoadingSpy: jasmine.Spy;
        let dialogOpenSpy: jasmine.Spy;
        let completeDeleteSpy: jasmine.Spy;
        let error: any;

        beforeEach(inject((statefulArtifactFactory: IStatefulArtifactFactory,
            localization: ILocalizationService,
            selectionManager: ISelectionManager,
            projectExplorerService: IProjectExplorerService,
            messageService: IMessageService,
            loadingOverlayService: ILoadingOverlayService,
            dialogService: IDialogService,
            navigationService: INavigationService

        ) => {
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            deleteAction = new DeleteAction(artifact, localization, messageService,
                projectExplorerService, loadingOverlayService, dialogService, navigationService);
            beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading").and.callThrough();
            endLoadingSpy = spyOn(loadingOverlayService, "endLoading").and.callThrough();
            dialogOpenSpy = spyOn(dialogService, "open");
            deleteSpy = spyOn(artifact, "delete");
            completeDeleteSpy = spyOn(deleteAction, "complete").and.callFake(() => {
                return true;
            });
            spyOn(messageService, "addError").and.callFake((err) => {
                error = err;
            });
            refreshSpy = spyOn(artifact, "refresh").and.callFake(() => {
                const deferred = $q_.defer();
                deferred.resolve(true);
                return deferred.promise;
            });
            getDescendantsSpy = spyOn(projectExplorerService, "getDescendantsToBeDeleted").and.callFake(() => {
                const deferred = $q_.defer();
                deferred.resolve({
                    id: 1, name: "Test", children: null
                });
                return deferred.promise;
            });

        }));

        it("confirm and delete", () => {
            // assert
            dialogOpenSpy.and.callFake(() => {
                const deferred = $q_.defer();
                deferred.resolve(true);
                return deferred.promise;
            });
            deleteSpy.and.callFake(() => {
                const deferred = $q_.defer();
                deferred.resolve();
                return deferred.promise;
            });

            deleteAction.execute();
            $scope.$digest();

            expect(getDescendantsSpy).toHaveBeenCalled();
            expect(deleteSpy).toHaveBeenCalled();

        });

        it("shows/hides loading screen", () => {
            // assert
            dialogOpenSpy.and.callFake(() => {
                const deferred = $q_.defer();
                deferred.resolve(true);
                return deferred.promise;
            });
            deleteSpy.and.callFake(() => {
                const deferred = $q_.defer();
                deferred.resolve();
                return deferred.promise;
            });

            deleteAction.execute();
            $scope.$digest();

            expect(beginLoadingSpy).toHaveBeenCalledTimes(2);
            expect(endLoadingSpy).toHaveBeenCalledTimes(2);
        });

        it("rejects delete", () => {
            // assert
            dialogOpenSpy.and.callFake(() => {
                const deferred = $q_.defer();
                deferred.reject();
                return deferred.promise;
            });


            deleteAction.execute();
            $scope.$digest();


            expect(deleteSpy).not.toHaveBeenCalled();
        });

        it("failed delete", () => {
            // assert
            dialogOpenSpy.and.callFake(() => {
                const deferred = $q_.defer();
                deferred.resolve(true);
                return deferred.promise;
            });
            deleteSpy.and.callFake(() => {
                const deferred = $q_.defer();
                deferred.reject({});
                return deferred.promise;
            });


            deleteAction.execute();
            $scope.$digest();


            expect(completeDeleteSpy).not.toHaveBeenCalled();
            expect(error).toBeDefined();

        });

    });
});
