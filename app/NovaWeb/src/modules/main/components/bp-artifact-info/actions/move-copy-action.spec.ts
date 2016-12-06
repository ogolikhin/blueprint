import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {MoveCopyAction} from "./move-copy-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {ProjectManagerMock} from "../../../../managers/project-manager/project-manager.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {MoveCopyArtifactResult, MoveCopyArtifactInsertMethod} from "../../../../main/components/dialogs/move-copy-artifact/move-copy-artifact";
import {Enums} from "../../../../main/models";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";
import {LoadingOverlayServiceMock} from "../../../../core/loading-overlay/loading-overlay.svc.mock";


describe("MoveAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;

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

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService) => {
        $scope = $rootScope.$new();
        $q = _$q_;
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
            new MoveCopyAction($q, artifact, localization, messageService, projectManager, dialogService, navigationService, loadingOverlayService);
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
            new MoveCopyAction($q, artifact, localization, messageService, null, dialogService, navigationService, loadingOverlayService);
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
            new MoveCopyAction($q, artifact, localization, messageService, projectManager, null, navigationService, loadingOverlayService);
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
        const moveAction = new MoveCopyAction($q, artifact, localization, messageService, projectManager,
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
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const moveAction = new MoveCopyAction($q, artifact, localization, messageService, projectManager,
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
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const moveAction = new MoveCopyAction($q, artifact, localization, messageService, projectManager,
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
            const moveAction = new MoveCopyAction($q, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            // assert
            expect(moveAction.disabled).toBe(false);
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
            const moveAction = new MoveCopyAction($q, artifact, localization, messageService,
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
            const moveAction = new MoveCopyAction($q, artifact, localization, messageService,
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
            spyOn(artifact, "save").and.callFake(() => $q.resolve());
            const refreshSpy = spyOn(projectManager, "refresh").and.callFake(() => $q.resolve());

            // act
            moveAction.executeMove();
            $scope.$digest();

            // assert
            expect(refreshSpy).toHaveBeenCalled();
        }));
});