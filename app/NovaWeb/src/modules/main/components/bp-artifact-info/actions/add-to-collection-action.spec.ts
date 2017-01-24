import * as angular from "angular";
import "angular-mocks";
import "../../../";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {ItemTypePredefined} from "../../../models/enums";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {ProjectManagerMock} from "../../../../managers/project-manager/project-manager.mock";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {NavigationServiceMock} from "../../../../commonModule/navigation/navigation.service.mock";
import {LoadingOverlayServiceMock} from "../../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {AddToCollectionAction} from "./add-to-collection-action";
import {BPDropdownItemAction} from "../../../../shared/widgets/bp-toolbar/actions/bp-dropdown-action";
import {CollectionServiceMock} from "../../../../editors/bp-collection/collection.svc.mock";
import {ItemInfoServiceMock} from "../../../../commonModule/itemInfo/itemInfo.service.mock";
import {MessageServiceMock} from "../../messages/message.mock";
import {IMessageService} from "../../messages/message.svc";


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
        $provide.service("collectionService", CollectionServiceMock);
        $provide.service("itemInfoServiceMock", ItemInfoServiceMock);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService) => {
        $scope = $rootScope.$new();
        $q = _$q_;
    }));

    it("is disabled when artifact is Collection Folder",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
                navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock,
                collectionService: CollectionServiceMock,
                itemInfoServiceMock: ItemInfoServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.CollectionFolder
                });

            // act
            const addToCollectionAction = new AddToCollectionAction($q, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService, collectionService, itemInfoServiceMock);

            const menuAction = _.find(addToCollectionAction.actions, o => o instanceof BPDropdownItemAction);

            // assert
            expect(menuAction.disabled).toBe(true);
        }));

    it("is disabled when artifact is Collection Artifact",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
                navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock,
                collectionService: CollectionServiceMock,
                itemInfoServiceMock: ItemInfoServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.ArtifactCollection
                });

            // act
            const addToCollectionAction = new AddToCollectionAction($q, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService, collectionService, itemInfoServiceMock);

            const menuAction = _.find(addToCollectionAction.actions, o => o instanceof BPDropdownItemAction);

            // assert
            expect(menuAction.disabled).toBe(true);
        }));

    it("is enabled when artifact is Actor",
        inject((statefulArtifactFactory: IStatefulArtifactFactory,
                localization: ILocalizationService,
                messageService: IMessageService, projectManager: ProjectManagerMock, dialogService: DialogServiceMock,
                navigationService: NavigationServiceMock, loadingOverlayService: LoadingOverlayServiceMock,
                collectionService: CollectionServiceMock,
                itemInfoServiceMock: ItemInfoServiceMock) => {
            // arrange
            const artifact: IStatefulArtifact = statefulArtifactFactory.createStatefulArtifact(
                {
                    id: 1,
                    predefinedType: ItemTypePredefined.Actor
                });

            // act
            const addToCollectionAction = new AddToCollectionAction($q, artifact, localization, messageService, projectManager,
                dialogService, navigationService, loadingOverlayService, collectionService, itemInfoServiceMock);

            const menuAction = _.find(addToCollectionAction.actions, o => o instanceof BPDropdownItemAction);

            // assert
            expect(menuAction.disabled).toBe(false);
        }));
});
