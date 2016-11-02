import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {DiscardAction} from "./discard-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService, IMessageService} from "../../../../core";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ItemTypePredefined, RolePermissions, LockedByEnum} from "../../../../main/models/enums";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../core/loading-overlay";


describe("DiscardAction", () => {
    let $scope: ng.IScope;
    let $q: ng.IQService;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
          $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService) => {
        $scope = $rootScope.$new();
        $q = _$q_;
    }));

    it("throws exception when localization is null", inject((statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new DiscardAction(artifact, localization, messageService, loadingOverlayService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("is disabled when artifact is null", inject((localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
        // arrange
        const artifact: IStatefulArtifact = null;

        // act
        const discardAction = new DiscardAction(artifact, localization, messageService, loadingOverlayService);

        // assert
        expect(discardAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is read-only",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            artifact.artifactState.readonly = true;

            // act
            const discardAction = new DiscardAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(discardAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Project",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const discardAction = new DiscardAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(discardAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const discardAction = new DiscardAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(discardAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
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
            const discardAction = new DiscardAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(discardAction.disabled).toBe(false);
        }));

    it("calls artifact.discard when executed",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
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
            const discardAction = new DiscardAction(artifact, localization, messageService, loadingOverlayService);

            // act
            discardAction.execute();

            // assert
            expect(discardSpy).toHaveBeenCalled();
        }));
});
