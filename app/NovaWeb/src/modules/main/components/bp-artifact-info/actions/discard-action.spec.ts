import "../../../";
import "angular-mocks";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {IProjectManager} from "../../../../managers/project-manager/project-manager";
import {ProjectManagerMock} from "../../../../managers/project-manager/project-manager.mock";
import {RolePermissions} from "../../../models/enums";
import {ItemTypePredefined} from "../../../models/item-type-predefined";
import {MessageServiceMock} from "../../messages/message.mock";
import {IMessageService} from "../../messages/message.svc";
import {DiscardAction} from "./discard-action";
import * as angular from "angular";

describe("DiscardAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;
    let statefulArtifactFactory: IStatefulArtifactFactory;
    let localization: ILocalizationService;
    let messageService: IMessageService;
    let projectManager: IProjectManager;
    let loadingOverlayService: ILoadingOverlayService;
    let navigationService: INavigationService;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("projectManager", ProjectManagerMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("navigationService", NavigationServiceMock);
    }));

    beforeEach(inject((
        $rootScope: ng.IRootScopeService,
        _$q_: ng.IQService,
        _statefulArtifactFactory_: IStatefulArtifactFactory,
        _localization_: ILocalizationService,
        _messageService_: IMessageService,
        _projectManager_: IProjectManager,
        _loadingOverlayService_: ILoadingOverlayService,
        _navigationService_: INavigationService
    ) => {
        $scope = $rootScope.$new();
        $q = _$q_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        localization = _localization_;
        messageService = _messageService_;
        projectManager = _projectManager_;
        loadingOverlayService = _loadingOverlayService_;
        navigationService = _navigationService_;
    }));

    it("throws exception when localization is null", () => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new DiscardAction(artifact, localization, messageService, projectManager, loadingOverlayService, navigationService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    });

    it("is disabled when artifact is null", () => {
        // arrange
        const artifact: IStatefulArtifact = null;

        // act
        const discardAction = new DiscardAction(artifact, localization, messageService, projectManager, loadingOverlayService, navigationService);

        // assert
        expect(discardAction.disabled).toBe(true);
    });

    it("is disabled when artifact is read-only", () => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        artifact.artifactState.readonly = true;

        // act
        const discardAction = new DiscardAction(artifact, localization, messageService, projectManager, loadingOverlayService, navigationService);

        // assert
        expect(discardAction.disabled).toBe(true);
    });

    it("is disabled when artifact is Project", () => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
        {
            id: 1,
            predefinedType: ItemTypePredefined.Project
        });

        // act
        const discardAction = new DiscardAction(artifact, localization, messageService, projectManager, loadingOverlayService, navigationService);

        // assert
        expect(discardAction.disabled).toBe(true);
    });

    it("is disabled when artifact is Collections", () => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
        {
            id: 1,
            predefinedType: ItemTypePredefined.Collections
        });

        // act
        const discardAction = new DiscardAction(artifact, localization, messageService, projectManager, loadingOverlayService, navigationService);

        // assert
        expect(discardAction.disabled).toBe(true);
    });

    it("is enabled when artifact is valid", () => {
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
        const discardAction = new DiscardAction(artifact, localization, messageService, projectManager, loadingOverlayService, navigationService);

        // assert
        expect(discardAction.disabled).toBe(false);
    });

    it("calls artifact.discard when executed", () => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
            {
                id: 1,
                predefinedType: ItemTypePredefined.TextualRequirement,
                lockedByUser: null,
                lockedDateTime: null,
                permissions: RolePermissions.Edit
            });
        const discardSpy = spyOn(artifact, "discardArtifact").and.callFake(() => {
                const deferred = $q.defer();
                deferred.reject(null);
                return deferred.promise;
            });
        const discardAction = new DiscardAction(artifact, localization, messageService, projectManager, loadingOverlayService, navigationService);

        // act
        discardAction.execute();

        // assert
        expect(discardSpy).toHaveBeenCalled();
    });
});
