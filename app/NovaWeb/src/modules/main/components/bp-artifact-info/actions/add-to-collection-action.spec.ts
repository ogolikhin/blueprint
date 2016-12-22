import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ItemTypePredefined, RolePermissions} from "../../../../main/models/enums";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {ProjectManagerMock} from "../../../../managers/project-manager/project-manager.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";
import {LoadingOverlayServiceMock} from "../../../../core/loading-overlay/loading-overlay.svc.mock";
import {AddToCollectionAction} from "./add-to-collection-action";
import {BPDropdownItemAction} from "../../../../shared/widgets/bp-toolbar/actions/bp-dropdown-action";


describe("AddToCollectionAction", () => {
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

    it("is disabled when artifact is Collection Folder",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
                navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.CollectionFolder
                });

            // act
            const addToCollectionAction = new AddToCollectionAction($q, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            const menuAction = _.find(addToCollectionAction.actions, o => o instanceof BPDropdownItemAction);

            // assert
            expect(menuAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collection Artifact",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
                navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.ArtifactCollection
                });

            // act
            const addToCollectionAction = new AddToCollectionAction($q, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            const menuAction = _.find(addToCollectionAction.actions, o => o instanceof BPDropdownItemAction);

            // assert
            expect(menuAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is Actor",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
                navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Actor
                });

            // act
            const addToCollectionAction = new AddToCollectionAction($q, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService);

            const menuAction = _.find(addToCollectionAction.actions, o => o instanceof BPDropdownItemAction);

            // assert
            expect(menuAction.disabled).toBe(false);
        }));
});
