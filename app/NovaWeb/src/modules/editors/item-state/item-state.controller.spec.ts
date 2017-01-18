import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "../../main";
import {Models} from "../../main/models";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {NavigationServiceMock} from "../../core/navigation/navigation.svc.mock";
import {IItemInfoService, IItemInfoResult} from "../../core/navigation/item-info.svc";
import {ItemInfoServiceMock} from "../../core/navigation/item-info.svc.mock";
import {IProjectManager} from "../../managers/project-manager/project-manager";
import {ProjectManagerMock} from "../../managers/project-manager/project-manager.mock";
import {IStatefulArtifactFactory} from "../../managers/artifact-manager/artifact/artifact.factory";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ItemStateController} from "./item-state.controller";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {MessageType, Message} from "../../core/messages/message";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {SelectionManagerMock} from "../../managers/selection-manager/selection-manager.mock";

describe("Item State Controller tests", () => {
    let $stateParams: ng.ui.IStateParamsService,
        $timeout: ng.ITimeoutService,
        $rootScope: ng.IRootScopeService,
        $q: ng.IQService,
        selectionManager: ISelectionManager,
        projectManager: IProjectManager,
        localization: ILocalizationService,
        messageService: IMessageService,
        navigationService: INavigationService,
        itemInfoService: IItemInfoService,
        statefulArtifactFactory: IStatefulArtifactFactory,
        ctrl: ItemStateController;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("projectManager", ProjectManagerMock);
        $provide.service("itemInfoService", ItemInfoServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((
        _$stateParams_: ng.ui.IStateParamsService,
        _$timeout_: ng.ITimeoutService,
        _$rootScope_: ng.IRootScopeService,
        _$q_: ng.IQService,
        _selectionManager_: ISelectionManager,
        _projectManager_: IProjectManager,
        _localization_: ILocalizationService,
        _messageService_: IMessageService,
        _navigationService_: INavigationService,
        _itemInfoService_: IItemInfoService,
        _statefulArtifactFactory_: IStatefulArtifactFactory) => {

        $stateParams = _$stateParams_;
        $timeout = _$timeout_;
        $rootScope = _$rootScope_;
        $q = _$q_;
        selectionManager = _selectionManager_;
        projectManager = _projectManager_;
        localization = _localization_;
        messageService = _messageService_;
        navigationService = _navigationService_;
        itemInfoService = _itemInfoService_;
        statefulArtifactFactory = _statefulArtifactFactory_;
    }));

    beforeEach(() => {
        selectionManager.setExplorerArtifact = (artifact) => null;
        selectionManager.setArtifact = (artifact) => null;
    });

    function getItemStateController(itemInfo: IItemInfoResult, version?: string): ItemStateController {
        if (version) {
            $stateParams["version"] = version;
        }

        return new ItemStateController(
            $stateParams,
            selectionManager,
            projectManager,
            messageService,
            localization,
            navigationService,
            itemInfoService,
            statefulArtifactFactory,
            $timeout,
            itemInfo);
    }

    it("respond to url", inject(($state: ng.ui.IStateService) => {
        expect($state.href("main.item", {id: 1})).toEqual("#/main/1");
    }));

    it("clears locked messages", () => {
        // arrange
        const itemInfo = {
            id: 10
        } as IItemInfoResult;
        const deleteMessageSpy = spyOn(messageService, "deleteMessageById");
        const message = new Message(MessageType.Deleted, "test");
        message.id = 1;
        messageService.addMessage(message);

        // act
        ctrl = getItemStateController(itemInfo);
        $rootScope.$digest();

        // assert
        expect(deleteMessageSpy).toHaveBeenCalled();
        expect(deleteMessageSpy).toHaveBeenCalledWith(message.id);
    });
});
