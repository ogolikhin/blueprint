import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "../../";
import { ComponentTest } from "../../../util/component.test";
import { BPDiscussionPanelController} from "./bp-discussions-panel";
import { HttpStatusCode } from "../../../core/http";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactDiscussionsMock } from "./artifact-discussions.mock";
import { SelectionManager } from "./../../../managers/selection-manager/selection-manager";
import { IReply, IDiscussion } from "./artifact-discussions.svc";
import { MessageServiceMock } from "../../../core/messages/message.mock";
import { DialogServiceMock } from "../../../shared/widgets/bp-dialog/bp-dialog";
import { ProcessServiceMock } from "../../../editors/bp-process/services/process.svc.mock";
import { ItemTypePredefined } from "../../../main/models/enums";
import {
    IArtifactManager,
    ArtifactManager,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService,
    ArtifactService,
    ArtifactAttachmentsService,
    ArtifactRelationshipsService }
    from "../../../managers/artifact-manager";

describe("Component BPDiscussionPanel", () => {

    let directiveTest: ComponentTest<BPDiscussionPanelController>;
    let template = `<bp-discussion-panel></bp-discussion-panel>`;
    let vm: BPDiscussionPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactDiscussions", ArtifactDiscussionsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactService", ArtifactService);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("metadataService", MetaDataService);
        $provide.service("artifactRelationships", ArtifactRelationshipsService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("processService", ProcessServiceMock);
    }));

    beforeEach(inject(() => {
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
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 2, 
                name: "Artifact 2", 
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();

            //Assert
            expect(artifact).toBeDefined();
            expect(vm.artifactDiscussionList.length).toBe(2);
        }));

    it("should not load data for a artifact of incorrect type",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 2, 
                name: "Collection", 
                predefinedType: ItemTypePredefined.Collections, 
                version: 1
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();

            //Assert
            expect(vm.artifactDiscussionList.length).toBe(0);
            expect(vm.canCreate).toBe(false);
            expect(vm.canDelete).toBe(false);
        }));

    it("should load replies for expanded discussion",
        inject(($rootScope: ng.IRootScopeService,
                artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            vm.artifactDiscussionList[0].expanded = false;
            vm.expandCollapseDiscussion(vm.artifactDiscussionList[0]);
            $timeout.flush();

            //Assert
            expect(vm.artifactDiscussionList[0].expanded).toBe(true);
            expect(vm.artifactDiscussionList[0].replies.length).toBe(1);
        }));

    it("should throw exception for expanded discussion",
        inject(($rootScope: ng.IRootScopeService,
                artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });
            
            $rootScope.$digest();
            let deferred = $q.defer();
            ArtifactDiscussionsMock.prototype.getReplies = jasmine.createSpy("getReplies() spy").and.callFake(
                (): ng.IPromise <IReply[] > => {
                    deferred.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();

            vm.artifactDiscussionList[0].expanded = false;
            vm.expandCollapseDiscussion(vm.artifactDiscussionList[0]);
            $timeout.flush();

            //Assert
            expect(vm.artifactDiscussionList[0].expanded).toBe(true);
            expect(vm.artifactDiscussionList[0].replies.length).toBe(0);
        }));

    it("expanded should be false for collapsed discussion",
        inject(($rootScope: ng.IRootScopeService,
                artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            vm.artifactDiscussionList[0].expanded = true;
            vm.expandCollapseDiscussion(vm.artifactDiscussionList[0]);
            $timeout.flush();

            //Assert
            expect(vm.artifactDiscussionList[0].expanded).toBe(false);
        }));

    it("add discussion should return default discussion",
        inject(($rootScope: ng.IRootScopeService,
                artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            let newDiscussion: IDiscussion;
            vm.addArtifactDiscussion("test").then((result: IDiscussion) => { newDiscussion = result; });
            $timeout.flush();

            //Assert
            expect(newDiscussion).toBeDefined;
        }));

    it("add discussion throws exception",
        inject(($rootScope: ng.IRootScopeService,
                artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            let deferred = $q.defer();
            ArtifactDiscussionsMock.prototype.addDiscussion = jasmine.createSpy("addDiscussion() spy").and.callFake(
                (): ng.IPromise<IDiscussion> => {
                    deferred.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            let newDiscussion: IDiscussion;
            vm.addArtifactDiscussion("test").then((result: IDiscussion) => { newDiscussion = result; });
            $timeout.flush();

            //Assert
            expect(newDiscussion).toBeUndefined;
        }));

    it("add discussion reply should return default reply",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager,
                statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            let newReply: IReply;
            vm.addDiscussionReply(vm.artifactDiscussionList[0], "test").then((result: IReply) => { newReply = result; });
            $timeout.flush();

            //Assert
            expect(newReply).toBeDefined;
            expect(newReply).not.toBeNull;
        }));

    it("add discussion reply throws exception",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager,
                statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            let deferred = $q.defer();
            ArtifactDiscussionsMock.prototype.addDiscussionReply = jasmine.createSpy("addDiscussionReply() spy").and.callFake(
                (): ng.IPromise<IReply> => {
                    deferred.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            let newReply: IReply;
            vm.addDiscussionReply(vm.artifactDiscussionList[0], "test").then((result: IReply) => { newReply = result; });
            $timeout.flush();

            //Assert
            expect(newReply).toBeUndefined;
        }));

    it("Clicking new comment shows add comment",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager,
                statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            vm.newCommentClick();

            //Assert
            expect(vm.showAddComment).toBe(true);
            expect(vm.artifactDiscussionList[1].showAddReply).toBe(false);
        }));

    it("Clicking cancel comment hides add comment",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager,
                statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });

            vm.showAddComment = true;

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            vm.cancelCommentClick();

            //Assert
            expect(vm.showAddComment).toBe(false);
        }));

    it("Editing discussion should move it to the first",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager,
                statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22, 
                name: "Process 22",
                predefinedType: ItemTypePredefined.Process, 
                version: 1
            });
            
            vm.showAddComment = true;

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            const secondDiscussionId = vm.artifactDiscussionList[1].discussionId;
            vm.discussionEdited(vm.artifactDiscussionList[1]);

            //Assert
            expect(vm.artifactDiscussionList[0].discussionId).toBe(secondDiscussionId);
        }));
    it("delete comment success, replies reloaded.",
        inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager,
                statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "PRO", version: 1});
            vm.showAddComment = true;
            let reply: IReply = {
                itemId: 100,
                discussionId: 1,
                version: 1,
                userId: 1,
                lastEditedOn: "1/1/1/1",
                userName: "Test User",
                isGuest: false,
                comment: "test comment",
                canEdit: true,
                canDelete: true,
                replyId: 2
            };
            let discussion: IDiscussion = {
                itemId: 100,
                discussionId: 1,
                version: 1,
                userId: 1,
                lastEditedOn: "1/1/1/1",
                userName: "Test User",
                isGuest: false,
                comment: "test comment",
                canEdit: true,
                canDelete: true,
                status: "teststatus",
                isClosed: false,
                repliesCount: 1,
                replies: [reply],
                expanded: true,
                showAddReply: true
            };
            ArtifactDiscussionsMock.prototype.getReplies = (
                artifactId: number,
                discussionId: number,
                subArtifactId?: number): ng.IPromise<IReply[]> => {
                const deferred = $q.defer<any[]>();
                let artifactReplies = [
                    {
                        "replyId": 2,
                        "itemId": 1,
                        "discussionId": 1,
                        "version": 3,
                        "userId": 1,
                        "lastEditedOn": "",
                        "userName": "Mehdi",
                        "isGuest": false,
                        "comment": "This is a test.",
                        "canEdit": true,
                        "canDelete": false
                    }
                ];
                deferred.resolve(artifactReplies);
                return deferred.promise;
            };
            //Act
            artifactManager.selection.setArtifact(artifact);
            vm.deleteReply(discussion, reply);
            $rootScope.$digest();
            //Assert
            expect(discussion.replies[0].replyId).toBe(2);
        }));

    it("delete comment thread success, discussions reloaded.",
        inject(($rootScope: ng.IRootScopeService,
                artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "PRO", version: 1});
            vm.showAddComment = true;
            let reply: IReply = {
                itemId: 100,
                discussionId: 1,
                version: 1,
                userId: 1,
                lastEditedOn: "1/1/1/1",
                userName: "Test User",
                isGuest: false,
                comment: "test comment",
                canEdit: true,
                canDelete: true,
                replyId: 2
            };
            let discussion: IDiscussion = {
                itemId: 100,
                discussionId: 1,
                version: 1,
                userId: 1,
                lastEditedOn: "1/1/1/1",
                userName: "Test User",
                isGuest: false,
                comment: "test comment",
                canEdit: true,
                canDelete: true,
                status: "teststatus",
                isClosed: false,
                repliesCount: 1,
                replies: [reply],
                expanded: true,
                showAddReply: true
            };
            //Act
            artifactManager.selection.setArtifact(artifact);
            vm.deleteCommentThread(discussion);
            $rootScope.$digest();
            //Assert
            expect(vm.artifactDiscussionList[0].itemId).toBe(1);
            expect(vm.artifactDiscussionList[0].discussionId).toBe(1);
            expect(vm.artifactDiscussionList[0].lastEditedOn).toBe("2016-05-31T17:19:53.07");
        }));
});

