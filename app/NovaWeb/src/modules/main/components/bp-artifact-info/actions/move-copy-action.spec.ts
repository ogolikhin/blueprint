import "../../../";
import "angular-mocks";
import {LoadingOverlayServiceMock} from "../../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {Enums, Models} from "../../../../main/models";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ProjectManagerMock} from "../../../../managers/project-manager/project-manager.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {RolePermissions} from "../../../models/enums";
import {ItemTypePredefined} from "../../../models/itemTypePredefined.enum";
import {MoveCopyArtifactInsertMethod, MoveCopyArtifactResult} from "../../dialogs/move-copy-artifact/move-copy-artifact";
import {MessageServiceMock} from "../../messages/message.mock";
import {IMessageService} from "../../messages/message.svc";
import {MoveCopyAction} from "./move-copy-action";
import * as angular from "angular";


describe("MoveCopyAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;
    let $timeout: ng.ITimeoutService;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("projectManager", ProjectManagerMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService, _$timeout_: ng.ITimeoutService) => {
        $scope = $rootScope.$new();
        $q = _$q_;
        $timeout = _$timeout_;
    }));

    it("throws exception when localization is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, projectManager: ProjectManagerMock,
            dialogService: DialogServiceMock, navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager, dialogService, navigationService, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).toBeDefined();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("throws exception when project manager is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, localization: ILocalizationService,
            dialogService: DialogServiceMock, navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        let error: Error = null;

        // act
        try {
            new MoveCopyAction($q, $timeout, artifact, localization, messageService, null, dialogService, navigationService, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).toBeDefined();
        expect(error).toEqual(new Error("Project manager not provided or is null"));
    }));

    it("throws exception when dialog service is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, projectManager: ProjectManagerMock, localization: ILocalizationService,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        let error: Error = null;

        // act
        try {
            new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager, null, navigationService, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).toBeDefined();
        expect(error).toEqual(new Error("Dialog service not provided or is null"));
    }));

    it("is disabled when artifact is null", inject((localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = null;

        // act
        const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
            dialogService, navigationService, loadingOverlayService);

        // assert
        expect(moveAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is historical", inject((
        statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
        messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
        navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                id: 1,
                predefinedType: ItemTypePredefined.TextualRequirement
            });
        artifact.artifactState.historical = true;

        // act
        const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
            dialogService, navigationService, loadingOverlayService);

        // assert
        expect(moveAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is Project",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    itemTypeId: ItemTypePredefined.Project
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    itemTypeId: ItemTypePredefined.Collections
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Baselines and Reviews",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    itemTypeId: ItemTypePredefined.BaselinesAndReviews
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit,
                    version: -1
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(false);
            expect(moveAction.actions[0].disabled).toBe(false);
            expect(moveAction.actions[1].disabled).toBe(false);
        }));

    it("only 'move' is enabled when artifact is a collection folder",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.CollectionFolder,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit,
                    version: -1
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(false);
            expect(moveAction.actions[0].disabled).toBe(false);
            expect(moveAction.actions[1].disabled).toBe(true);
        }));

    it("only 'move' is enabled when artifact is a collection artifact",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.ArtifactCollection,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit,
                    version: -1
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(false);
            expect(moveAction.actions[0].disabled).toBe(false);
            expect(moveAction.actions[1].disabled).toBe(true);
        }));

    it("only 'move' is enabled when artifact is a baseline folder",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.BaselineFolder,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit,
                    version: -1
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(false);
            expect(moveAction.actions[0].disabled).toBe(false);
            expect(moveAction.actions[1].disabled).toBe(true);
        }));

    it("only 'move' is enabled when artifact is a baseline artifact",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.ArtifactBaseline,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit,
                    version: -1
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(false);
            expect(moveAction.actions[0].disabled).toBe(false);
            expect(moveAction.actions[1].disabled).toBe(true);
        }));

    it("only 'move' is enabled when artifact is a review artifact",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.ArtifactReviewPackage,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit,
                    version: -1
                });

            // act
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(false);
            expect(moveAction.actions[0].disabled).toBe(false);
            expect(moveAction.actions[1].disabled).toBe(true);
        }));

    it("calls artifact.copy when executed",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            const copySpy = spyOn(artifact, "copy").and.callFake(() => $q.reject(null));
            const copyAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService,
                projectManager, dialogService, navigationService, loadingOverlayService);
            spyOn(dialogService, "open").and.callFake(() => {
                let result: MoveCopyArtifactResult[] = [
                    {
                        artifacts: [
                            {
                                id: 1,
                                name: "test"
                            }
                        ],
                        insertMethod: MoveCopyArtifactInsertMethod.Inside
                    }
                ];
                return $q.resolve(result);
            });
            //spyOn(artifact, "lock").and.callFake(() => $q.resolve());

            // act
            copyAction.executeCopy();
            $scope.$digest();

            // assert
            expect(copySpy).toHaveBeenCalled();
        }));

    it("calls artifact.move when executed",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            const moveSpy = spyOn(artifact, "move").and.callFake(() => $q.reject(null));
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService,
                projectManager, dialogService, navigationService, loadingOverlayService);
            spyOn(dialogService, "open").and.callFake(() => {
                let result: MoveCopyArtifactResult[] = [
                    {
                        artifacts: [
                            {
                                id: 1,
                                name: "test"
                            }
                        ],
                        insertMethod: MoveCopyArtifactInsertMethod.Inside
                    }
                ];
                return $q.resolve(result);
            });
            spyOn(artifact, "lock").and.callFake(() => $q.resolve());

            // act
            moveAction.executeMove();
            $scope.$digest();

            // assert
            expect(moveSpy).toHaveBeenCalled();
        }));

        it("refresh after move",
            inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: Enums.LockedByEnum.CurrentUser,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            artifact.artifactState.dirty = true;
            spyOn(artifact, "move").and.callFake(() => $q.resolve());
            const moveAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService,
                projectManager, dialogService, navigationService, loadingOverlayService);
            spyOn(dialogService, "open").and.callFake(() => {
                let result: MoveCopyArtifactResult[] = [
                    {
                        artifacts: [
                            {
                                id: 1,
                                name: "test"
                            }
                        ],
                        insertMethod: MoveCopyArtifactInsertMethod.Inside
                    }
                ];
                return $q.resolve(result);
            });
            //spyOn(artifact, "save").and.callFake(() => $q.resolve());
            const refreshSpy = spyOn(projectManager, "refresh").and.callFake(() => $q.resolve());

            // act
            moveAction.executeMove();
            $scope.$digest();

            // assert
            expect(refreshSpy).toHaveBeenCalled();
        }));

        it("navigate to after copy",
            inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
            navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: Enums.LockedByEnum.CurrentUser,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            spyOn(artifact, "copy").and.callFake(() => $q.resolve(<Models.ICopyResultSet>{artifact: {id: 1}}));
            const copyAction = new MoveCopyAction($q, $timeout, artifact, localization, messageService,
                projectManager, dialogService, navigationService, loadingOverlayService);
            spyOn(dialogService, "open").and.callFake(() => {
                let result: MoveCopyArtifactResult[] = [
                    {
                        artifacts: [
                            {
                                id: 1,
                                name: "test"
                            }
                        ],
                        insertMethod: MoveCopyArtifactInsertMethod.Inside
                    }
                ];
                return $q.resolve(result);
            });
            const navigateToSpy = spyOn(navigationService, "navigateTo").and.callFake(() => $q.resolve());

            // act
            copyAction.executeCopy();
            $scope.$digest();
            $timeout.flush();

            // assert
            expect(navigateToSpy).toHaveBeenCalled();
        }));
});
