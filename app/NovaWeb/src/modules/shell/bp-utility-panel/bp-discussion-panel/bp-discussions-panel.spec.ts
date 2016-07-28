import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../util/component.test";
import { BPDiscussionPanelController} from "./bp-discussions-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactDiscussionsMock } from "./artifact-discussions.mock";
import { ProjectRepositoryMock } from "../../../main/services/project-repository.mock";
import { ProjectManager, Models } from "../../../main/services/project-manager";
import {IReply} from "./artifact-discussions.svc";
import {MessageServiceMock} from "../../../core/messages/message.mock";

describe("Component BPDiscussionPanel", () => {
    
    let directiveTest: ComponentTest<BPDiscussionPanelController>;
    let template = `<bp-discussion-panel></bp-discussion-panel>`;
    let vm: BPDiscussionPanelController;
    let bpAccordionPanelController = {
        isOpenObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactDiscussions", ArtifactDiscussionsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(inject((projectManager: ProjectManager) => {
        projectManager.initialize();
        directiveTest = new ComponentTest<BPDiscussionPanelController>(template, "bp-discussion-panel");
        vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
    }));

    afterEach(() => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".filter-bar").length).toBe(0);
        expect(directiveTest.element.find(".empty-state").length).toBe(1);
        expect(directiveTest.element.find(".scrollable-content").length).toBe(0);
    });

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {
            //Arrange
            projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
            $rootScope.$digest();

            //Act
            let artifact = projectManager.getArtifact(22);
            projectManager.loadArtifact(artifact);
            artifact.prefix = "PRO";
            $rootScope.$digest();

            //Assert
            expect(artifact).toBeDefined();
            expect(vm.artifactDiscussionList.length).toBe(2);
        }));

    it("should load replies for expanded discussion",
        inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, $timeout: ng.ITimeoutService) => {
            //Arrange
            projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
            $rootScope.$digest();

            //Act
            let artifact = projectManager.getArtifact(22);
            projectManager.loadArtifact(artifact);
            artifact.prefix = "PRO";
            $rootScope.$digest();
            vm.artifactDiscussionList[0].expanded = false;
            vm.expandCollapseDiscussion(vm.artifactDiscussionList[0]);
            $timeout.flush();

            //Assert
            expect(vm.artifactDiscussionList[0].expanded).toBe(true);
            expect(vm.artifactDiscussionList[0].replies.length).toBe(1);
        }));

    it("should throw exception for expanded discussion",
        inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
            $rootScope.$digest();
            let deferred = $q.defer();
            //MessageService.prototype.addError = jasmine.createSpy("addError() spy").and.callFake(() => {});
            ArtifactDiscussionsMock.prototype.getReplies = jasmine.createSpy("getReplies() spy").and.callFake(
                (): ng.IPromise <IReply[] > => {
                    deferred.reject({
                        statusCode: 404,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );

            //Act
            let artifact = projectManager.getArtifact(22);
            projectManager.loadArtifact(artifact);
            artifact.prefix = "PRO";
            $rootScope.$digest();
            vm.artifactDiscussionList[0].expanded = false;
            vm.expandCollapseDiscussion(vm.artifactDiscussionList[0]);
            $timeout.flush();

            //Assert
            expect(vm.artifactDiscussionList[0].expanded).toBe(true);
            expect(vm.artifactDiscussionList[0].replies.length).toBe(0);
        }));

    it("expanded should be false for collapsed discussion",
        inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager, $timeout: ng.ITimeoutService) => {
            //Arrange
            projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
            $rootScope.$digest();

            //Act
            let artifact = projectManager.getArtifact(22);
            projectManager.loadArtifact(artifact);
            artifact.prefix = "PRO";
            $rootScope.$digest();
            vm.artifactDiscussionList[0].expanded = true;
            vm.expandCollapseDiscussion(vm.artifactDiscussionList[0]);
            $timeout.flush();

            //Assert
            expect(vm.artifactDiscussionList[0].expanded).toBe(false);
        }));

});
