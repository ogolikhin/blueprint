import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {PublishAction} from "./publish-action";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService, IMessageService} from "../../../../core";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../core/loading-overlay";
import {IDialogService, DialogService} from "../../../../shared/widgets/bp-dialog";

describe("PublishAction", () => {
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("dialogService", LoadingOverlayService);
    }));

    it("throws exception when localization is null", inject(($q: ng.IQService, statefulArtifactFactory: IStatefulArtifactFactory,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService, dialogService: IDialogService) => {
        // arrange
        const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
        const localization: ILocalizationService = null;
        let error: Error = null;

        // act
        try {
            new PublishAction($q, artifact, localization, messageService, loadingOverlayService, dialogService);
        } catch (exception) {
            error = exception;
        }

        // assert
        expect(error).not.toBeNull();
        expect(error).toEqual(new Error("Localization service not provided or is null"));
    }));

    it("is disabled when artifact is null", inject(($q: ng.IQService, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService, dialogService: IDialogService) => {
        // arrange
        const artifact: IStatefulArtifact = null;

        // act
        const publishAction = new PublishAction($q, artifact, localization, messageService, loadingOverlayService, dialogService);

        // assert
        expect(publishAction.disabled).toBe(true);
    }));

    it("is disabled when artifact is read-only",
        inject(($q: ng.IQService, statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService, dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact({id: 1});
            artifact.artifactState.readonly = true;

            // act
            const publishAction = new PublishAction($q, artifact, localization, messageService, loadingOverlayService, dialogService);

            // assert
            expect(publishAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Project",
        inject(($q: ng.IQService, statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService, dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Project
                });

            // act
            const publishAction = new PublishAction($q, artifact, localization, messageService, loadingOverlayService, dialogService);

            // assert
            expect(publishAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collections",
        inject(($q: ng.IQService, statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService, dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Collections
                });

            // act
            const publishAction = new PublishAction($q, artifact, localization, messageService, loadingOverlayService, dialogService);

            // assert
            expect(publishAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is valid",
        inject(($q: ng.IQService, statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService, dialogService: IDialogService) => {
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
            const publishAction = new PublishAction($q, artifact, localization, messageService, loadingOverlayService, dialogService);

            // assert
            expect(publishAction.disabled).toBe(false);
        }));

    it("calls artifact.discard when executed",
        inject(($q: ng.IQService, statefulArtifactFactory: IStatefulArtifactFactory, localization: ILocalizationService,
            messageService: IMessageService, loadingOverlayService: ILoadingOverlayService, dialogService: IDialogService) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.TextualRequirement,
                    lockedByUser: null,
                    lockedDateTime: null,
                    permissions: RolePermissions.Edit
                });
            const publishSpy = spyOn(artifact, "publish");
            const publishAction = new PublishAction($q, artifact, localization, messageService, loadingOverlayService, dialogService);

            // act
            publishAction.execute();

            // assert
            expect(publishSpy).toHaveBeenCalled();
        }));
});
