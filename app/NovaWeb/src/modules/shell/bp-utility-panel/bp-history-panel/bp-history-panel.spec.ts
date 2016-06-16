import "../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../util/component.test";
import { BPHistoryPanelController} from "./bp-history-panel";
import { LocalizationServiceMock } from "../../../core/localization.mock";
import { ArtifactHistoryMock } from "./artifact-history.mock";
import {ProjectRepositoryMock} from "../../../main/services/project-repository.mock";
import {ProjectManager, SubscriptionEnum} from "../../../main/managers/project-manager";

describe("Component BPHistoryPanel", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistoryMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    let directiveTest: ComponentTest<BPHistoryPanelController>;
    let template = `<bp-history-panel></bp-history-panel>`;
    let vm: BPHistoryPanelController;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPHistoryPanelController>(template, "bp-history-panel");
        vm = directiveTest.createComponent({});
    });

    afterEach( () => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".history-panel")).toBeDefined();
    });

    it("should load data for a selected artifact", inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {

        //Arrange
        projectManager.notify(SubscriptionEnum.ProjectLoad, { id: 2, name: "Project 2" });
        $rootScope.$digest();

        //Act
        let artifact = projectManager.getArtifact(22);

        //Assert
        expect(artifact).toBeDefined();
        expect(vm.artifactHistoryList.length).toBe(11);
    }));

    it("should get more historical versions along with a draft", inject(($timeout: ng.ITimeoutService) => {

        //Arrange
        vm.artifactHistoryList = [{
            "versionId": 52,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-06-06T13:58:24.557"
        }];
        vm.loadMoreHistoricalVersions();
        $timeout.flush();

        //Assert
        expect(vm.artifactHistoryList.length).toBe(12);
    }));

    it("should get empty list because it already has version 1", inject(($timeout: ng.ITimeoutService) => {

        //Arrange
        vm.artifactHistoryList = [{
            "versionId": 1,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-06-06T13:58:24.557"
        }];
        vm.loadMoreHistoricalVersions();

        //Assert
        expect(vm.artifactHistoryList.length).toBe(1);
    }));

    it("should get list in ascending order if the flag is set", inject(($timeout: ng.ITimeoutService) => {

        //Arrange
        vm.sortAscending = true;
        vm.changeSortOrder();
        $timeout.flush();

        //Assert
        expect(vm.artifactHistoryList.length).toBe(11);
    }));

    it("should select specified artifact version", inject(($timeout: ng.ITimeoutService) => {

        //Arrange
        let artifact = {
            "versionId": 1,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-06-06T13:58:24.557"
        };
        vm.artifactHistoryList = [artifact];
        vm.selectedArtifactVersion = null;
        vm.selectArtifactVersion(artifact);

        //Assert
        expect(vm.selectedArtifactVersion).toBe(artifact);
    }));


});