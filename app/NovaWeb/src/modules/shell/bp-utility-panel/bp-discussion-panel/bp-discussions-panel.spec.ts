import "../..";
import "angular-mocks";
import "angular-sanitize";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {MessageServiceMock} from "../../../main/components/messages/message.mock";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {IStatefulArtifactFactory} from "../../../managers/artifact-manager";
import {IStatefulArtifact, StatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {StatefulArtifactServices} from "../../../managers/artifact-manager/services";
import {StatefulSubArtifact} from "../../../managers/artifact-manager/sub-artifact";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {Helper} from "../../../shared/utils/helper";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ComponentTest} from "../../../util/component.test";
import {LicenseServiceMock} from "../../license/license.svc.mock";
import {SessionSvcMock} from "../../login/session.svc.mock";
import {IOnPanelChangesObject, PanelType} from "../utility-panel.svc";
import {ArtifactDiscussionsMock} from "./artifact-discussions.mock";
import {IDiscussion, IReply} from "./artifact-discussions.svc";
import {BPDiscussionPanelController} from "./bp-discussions-panel";

let setInitialArtifact = ($q: ng.IQService, artifactService: ArtifactServiceMock): IStatefulArtifact => {
    const services = new StatefulArtifactServices($q, null, null, null, null, null, artifactService, null, null, null, null, null, null, null);
    const artifact = new StatefulArtifact({id: 2, name: "Artifact 2", predefinedType: ItemTypePredefined.Process, version: 1}, services);
    spyOn(Helper, "canUtilityPanelUseSelectedArtifact").and.callFake((): boolean => { return true; });
    return artifact;
};

describe("Component BPDiscussionPanel", () => {

    let directiveTest: ComponentTest<BPDiscussionPanelController>;
    let template = `<bp-discussion-panel></bp-discussion-panel>`;
    let vm: BPDiscussionPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };
    let onChangesObj: IOnPanelChangesObject;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactDiscussions", ArtifactDiscussionsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("licenseService", LicenseServiceMock);
        $provide.service("session", SessionSvcMock);
    }));

    beforeEach(inject((selectionManager: SelectionManagerMock) => {
        selectionManager = selectionManager;
        directiveTest = new ComponentTest<BPDiscussionPanelController>(template, "bp-discussion-panel");
        vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
        onChangesObj = {
            context: {
                currentValue: {
                    panelType: PanelType.History
                },
                previousValue: undefined,
                isFirstChange: () => { return true; }
            }
        };
    }));

    afterEach(() => {
        vm = undefined;
        onChangesObj = undefined;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".filter-bar").length).toBe(0);
        expect(directiveTest.element.find(".empty-state").length).toBe(1);
        expect(directiveTest.element.find(".scrollable-content").length).toBe(0);
    });

    it("should be read-only for newly created sub-artifacts",
        inject(($rootScope: ng.IRootScopeService, statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 2,
                name: "Artifact 2",
                predefinedType: ItemTypePredefined.Process,
                version: 1
            });
            const processShape = {
                projectId: 1,
                parentId: 2,
                id: -2,
                name: "SubArtifact 2",
                baseItemTypePredefined: ItemTypePredefined.PROShape,
                typePrefix: "PROS",
                propertyValues: {},
                associatedArtifact: null,
                personaReference: null,
                flags: null

            };
            const subArtifact = statefulArtifactFactory.createStatefulProcessSubArtifact(artifact, processShape);
            onChangesObj.context.currentValue.artifact = artifact;
            onChangesObj.context.currentValue.subArtifact = subArtifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();

            //Assert
            expect(vm.canCreate).toBeFalsy();
            expect(vm.canDelete).toBeFalsy();
        }));

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            selectionManager: SelectionManagerMock) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            $timeout.flush();

            //Assert
            expect(artifact).toBeDefined();
            expect(vm.artifactDiscussionList.length).toBe(2);
        }));

    it("should load data for a selected subArtifact",
        inject(($rootScope: ng.IRootScopeService,
            statefulArtifactFactory: IStatefulArtifactFactory,
            $timeout: ng.ITimeoutService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService) => {
            //Arrange
            const services = new StatefulArtifactServices($q, null, null, null, null, null, artifactService, null, null, null, null, null, null, null);
            const artifact = new StatefulArtifact({id: 2, name: "Artifact 2", predefinedType: ItemTypePredefined.Process, version: 1}, services);
            spyOn(Helper, "canUtilityPanelUseSelectedArtifact").and.callFake((): boolean => { return true; });
            const processShape = {
                projectId: 1,
                parentId: 2,
                id: 4,
                name: "SubArtifact 2",
                baseItemTypePredefined: ItemTypePredefined.PROShape,
                typePrefix: "PROS",
                propertyValues: {},
                associatedArtifact: null,
                personaReference: null,
                flags: null
            };
            const subArtifact = new StatefulSubArtifact(artifact, processShape, services);
            onChangesObj.context.currentValue.artifact = artifact;
            onChangesObj.context.currentValue.subArtifact = subArtifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            $timeout.flush();

            //Assert
            expect(artifact).toBeDefined();
            expect(vm.artifactDiscussionList.length).toBe(2);
        }));

    it("should not load data for a artifact of incorrect type",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService) => {
            //Arrange
            const services = new StatefulArtifactServices($q, null, null, null, null, null, artifactService, null, null, null, null, null, null, null);
            const artifact = new StatefulArtifact({id: 2, name: "Collection", predefinedType: ItemTypePredefined.Collections, version: 1}, services);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();

            //Assert
            expect(vm.artifactDiscussionList.length).toBe(0);
            expect(vm.canCreate).toBe(false);
            expect(vm.canDelete).toBe(false);
        }));

    it("should load replies for expanded discussion",
        inject(($rootScope: ng.IRootScopeService,
            $timeout: ng.ITimeoutService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
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
            $timeout: ng.ITimeoutService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            artifactDiscussions: ArtifactDiscussionsMock) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);

            $rootScope.$digest();
            let deferred = $q.defer();
            spyOn(artifactDiscussions, "getReplies").and.callFake(
                (): ng.IPromise<IReply[]> => {
                    deferred.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
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
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            vm.artifactDiscussionList[0].expanded = true;
            vm.expandCollapseDiscussion(vm.artifactDiscussionList[0]);
            $timeout.flush();

            //Assert
            expect(vm.artifactDiscussionList[0].expanded).toBe(false);
        }));

    it("add discussion should return default discussion",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            let newDiscussion: IDiscussion;
            vm.addArtifactDiscussion("test").then((result: IDiscussion) => { newDiscussion = result; });
            $timeout.flush();

            //Assert
            expect(newDiscussion).toBeDefined;
        }));

    it("add discussion throws exception",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            artifactDiscussions: ArtifactDiscussionsMock) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);

            let deferred = $q.defer();
            spyOn(artifactDiscussions, "addDiscussion").and.callFake(
                (): ng.IPromise<IDiscussion> => {
                    deferred.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            let newDiscussion: IDiscussion;
            vm.addArtifactDiscussion("test").then((result: IDiscussion) => { newDiscussion = result; });
            $timeout.flush();

            //Assert
            expect(newDiscussion).toBeUndefined;
        }));

    it("add discussion reply should return default reply",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            let newReply: IReply;
            vm.addDiscussionReply(vm.artifactDiscussionList[0], "test").then((result: IReply) => { newReply = result; });
            $timeout.flush();

            //Assert
            expect(newReply).toBeDefined;
            expect(newReply.comment).toBe("test");
        }));

    it("add discussion reply throws exception",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            artifactDiscussions: ArtifactDiscussionsMock) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);

            let deferred = $q.defer();
            spyOn(artifactDiscussions, "addDiscussionReply").and.callFake(
                (): ng.IPromise<IReply> => {
                    deferred.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            let newReply: IReply;
            vm.addDiscussionReply(vm.artifactDiscussionList[0], "test").then((result: IReply) => { newReply = result; });
            $timeout.flush();

            //Assert
            expect(newReply).toBeUndefined;
        }));

    it("Clicking new comment shows add comment",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            vm.newCommentClick();

            //Assert
            expect(vm.showAddComment).toBe(true);
            expect(vm.artifactDiscussionList[1].showAddReply).toBe(false);
        }));

    it("Clicking cancel comment hides add comment",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            onChangesObj.context.currentValue.artifact = artifact;

            vm.showAddComment = true;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            vm.cancelCommentClick();

            //Assert
            expect(vm.showAddComment).toBe(false);
        }));

    it("Editing discussion should move it to the first",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
            vm.showAddComment = true;
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            const secondDiscussionId = vm.artifactDiscussionList[1].discussionId;
            vm.discussionEdited(vm.artifactDiscussionList[1]);

            //Assert
            expect(vm.artifactDiscussionList[0].discussionId).toBe(secondDiscussionId);
        }));

    it("delete comment success, replies reloaded.",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
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
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            vm.deleteReply(discussion, reply);
            //$rootScope.$digest();
            //Assert
            expect(discussion.replies[0].replyId).toBe(2);
        }));

    it("delete comment thread success, discussions reloaded.",
        inject(($rootScope: ng.IRootScopeService,
            artifactService: ArtifactServiceMock,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = setInitialArtifact($q, artifactService);
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
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            vm.deleteCommentThread(discussion);
            $rootScope.$digest();

            //Assert
            expect(vm.artifactDiscussionList[0].itemId).toBe(1);
            expect(vm.artifactDiscussionList[0].discussionId).toBe(1);
            expect(vm.artifactDiscussionList[0].lastEditedOn).toBe("2016-05-31T17:19:53.07");
        }));
});
