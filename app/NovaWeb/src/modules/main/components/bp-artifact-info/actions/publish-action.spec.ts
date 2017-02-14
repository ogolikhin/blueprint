import "../../../";
import "angular-mocks";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {DialogService} from "../../../../shared/widgets/bp-dialog";
import {RolePermissions} from "../../../models/enums";
import {ItemTypePredefined} from "../../../models/itemTypePredefined.enum";
import {MessageServiceMock} from "../../messages/message.mock";
import {IMessageService} from "../../messages/message.svc";
import {PublishAction} from "./publish-action";
import * as angular from "angular";

describe("PublishAction", () => {
    let $q: ng.IQService;
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("dialogService", DialogService);
    }));

    beforeEach(inject((_$q_: ng.IQService) => {
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
            new PublishAction(artifact, localization, messageService, loadingOverlayService);
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
        const publishAction = new PublishAction(artifact, localization, messageService, loadingOverlayService);

        // assert
        expect(publishAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is read-only",
        inject((statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            artifact.artifactState.readonly = true;

            // act
            const publishAction = new PublishAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(publishAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Project",
        inject((statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const publishAction = new PublishAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(publishAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections",
        inject((statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const publishAction = new PublishAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(publishAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject((statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
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
            const publishAction = new PublishAction(artifact, localization, messageService, loadingOverlayService);

            // assert
            expect(publishAction.disabled).toBe(false);
        }));

    it("calls artifact.discard when executed",
        inject((statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
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
            const publishSpy = spyOn(artifact, "publish").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject(null);
                    return deferred.promise;
                });
            const publishAction = new PublishAction(artifact, localization, messageService, loadingOverlayService);

            // act
            publishAction.execute();

            // assert
            expect(publishSpy).toHaveBeenCalled();
        }));
});
