﻿import "../../../";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../../util/component.test";
import {IDiscussion} from "../artifact-discussions.svc";
import {BPArtifactDiscussionItemController} from "./bp-artifact-discussion-item";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {ArtifactDiscussionsMock} from "../artifact-discussions.mock";
import {DialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {HttpStatusCode} from "../../../../commonModule/httpInterceptor/http-status-code";
import {MessageServiceMock} from "../../../../main/components/messages/message.mock";

describe("Component BPArtifactDiscussionItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    let directiveTest: ComponentTest<BPArtifactDiscussionItemController>;
    let vm: BPArtifactDiscussionItemController;
    let template = `<bp-artifact-discussion-item
            discussion-info="null"
            artifact-id="1">
        </bp-artifact-discussion-item>`;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("artifactDiscussions", ArtifactDiscussionsMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogService);
    }));

    beforeEach(() => {
        directiveTest = new ComponentTest<BPArtifactDiscussionItemController>(template, "bp-artifact-discussion-item");
        vm = directiveTest.createComponent({});
        const discussion: IDiscussion = {
            "isClosed": false,
            "status": "",
            "itemId": 1,
            "repliesCount": 1,
            "discussionId": 1,
            "version": 3,
            "userId": 1,
            "lastEditedOn": "2016-05-31T17:19:53.07",
            "userName": "Mehdi",
            "isGuest": false,
            "comment": "This is a test.",
            "canEdit": true,
            "canDelete": false,
            "showAddReply": false,
            "replies": [],
            "expanded": false
        };
        vm.discussionInfo = discussion;
    });

    it("should be visible by default", () => {

        //Assert
        expect(directiveTest.element.find(".button-bar").length).toBe(1);
        expect(directiveTest.element.find("bp-avatar").length).toBe(1);
    });

    it("edit discussion should return default discussion",
        inject(($timeout: ng.ITimeoutService) => {

            //Arrange
            vm.artifactId = 1;
            vm.discussionEdited = () => undefined;

            //Act
            vm.editDiscussion("");
            $timeout.flush();

            //Assert
            expect(vm.discussionInfo.comment).toBe("");
        }));

    it("edit discussion throws exception",
        inject(($timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            vm.artifactId = 1;
            vm.discussionEdited = () => undefined;
            let deferred = $q.defer();
            ArtifactDiscussionsMock.prototype.editDiscussion = jasmine.createSpy("editDiscussion() spy").and.callFake(
                (): ng.IPromise<IDiscussion> => {
                    deferred.reject({
                        statusCode: HttpStatusCode.NotFound,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );

            //Act
            let updatedDiscussion: IDiscussion;
            vm.editDiscussion("").then((result: IDiscussion) => {
                updatedDiscussion = result;
            });
            $timeout.flush();

            //Assert
            expect(updatedDiscussion).toBe(null);
        }));

    it("new reply click shows add reply",
        () => {
            //Arrange
            vm.canCreate = true;
            vm.cancelComment = () => undefined;

            //Act
            vm.newReplyClick();

            //Assert
            expect(vm.discussionInfo.showAddReply).toBe(true);
        });

    it("cancel comment click changes editing mode",
        () => {
            //Arrange
            vm.editing = true;

            //Act
            vm.cancelCommentClick();

            //Assert
            expect(vm.editing).toBe(false);
        });

    it("edit comment click changes editing mode",
        () => {
            //Arrange
            vm.editing = false;
            vm.canCreate = true;

            //Act
            vm.editCommentClick();

            //Assert
            expect(vm.editing).toBe(true);
        });
});
