import * as angular from "angular";
import "angular-mocks";
import {MoveAction} from "./move-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {ProjectManagerMock} from "../../../../managers/project-manager/project-manager.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {MoveArtifactResult, MoveArtifactInsertMethod} from "../../../../main/components/dialogs/move-artifact/move-artifact";
import {Enums} from "../../../../main/models";


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
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService) => {
        $scope = $rootScope.$new();
        $q = _$q_;
    }));

    it("throws exception when localization is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).toBeDefined();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("throws exception when project manager is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, localization: ILocalizationService, dialogService: DialogServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        let error: Error = null;

        // act
        try {
            new MoveAction($q, artifact, localization, messageService, null, dialogService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).toBeDefined();
        expect(error).toEqual(new Error("App_Error_No_Project_Manager"));
    }));

    it("throws exception when dialog service is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, projectManager: ProjectManagerMock, localization: ILocalizationService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        let error: Error = null;

        // act
        try {
            new MoveAction($q, artifact, localization, messageService, projectManager, null);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).toBeDefined();
        expect(error).toEqual(new Error("App_Error_No_Dialog_Service"));
    }));

    it("is disabled when artifact is null", inject((localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
        // arrange
        const artifact: IStatefulArtifact = null;

        // act
        const moveAction = new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);

        // assert
        expect(moveAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is read-only",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            artifact.artifactState.readonly = true;

            // act
            const moveAction = new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);

            // assert
            expect(moveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Project",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const moveAction = new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);

            // assert
            expect(moveAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const moveAction = new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);

            // assert
            expect(moveAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
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
            const moveAction = new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);

            // assert
            expect(moveAction.disabled).toBe(false);
        }));

    it("calls artifact.move when executed",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            const moveSpy = spyOn(artifact, "move").and.callFake(() => {
                return $q.reject(null);
            });
            const moveAction = new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);
            const dialogSpy = spyOn(dialogService, "open").and.callFake(() => {
                let result: MoveArtifactResult[] = [
                    {
                        artifacts: [
                            {
                                id: 1,
                                name: "test"
                            }
                        ],
                        insertMethod: MoveArtifactInsertMethod.Inside
                    }
                ];
                return $q.resolve(result);
            });
            const artifactSpy = spyOn(artifact, "lock").and.callFake(() => {
                return $q.resolve();
            });

            // act
            moveAction.execute();
            $scope.$digest();

            // assert
            expect(moveSpy).toHaveBeenCalled();
        }));

        it("refresh after move",
            inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock) => {
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
            const moveSpy = spyOn(artifact, "move").and.callFake(() => {
                return $q.resolve();
            });
            const moveAction = new MoveAction($q, artifact, localization, messageService, projectManager, dialogService);
            const dialogSpy = spyOn(dialogService, "open").and.callFake(() => {
                let result: MoveArtifactResult[] = [
                    {
                        artifacts: [
                            {
                                id: 1,
                                name: "test"
                            }
                        ],
                        insertMethod: MoveArtifactInsertMethod.Inside
                    }
                ];
                return $q.resolve(result);
            });
            const artifactSpy = spyOn(artifact, "save").and.callFake(() => {
                return $q.resolve();
            });
            const refreshSpy = spyOn(projectManager, "refresh").and.callFake(() => {
                return $q.resolve();
            });

            // act
            moveAction.execute();
            $scope.$digest();

            // assert
            expect(refreshSpy).toHaveBeenCalled();
        }));
});
