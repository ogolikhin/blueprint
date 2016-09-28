import "../../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPDiscussionReplyItemController } from "./bp-discussion-reply-item";
import { IReply } from "../artifact-discussions.svc";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import { ArtifactDiscussionsMock } from "../artifact-discussions.mock";
import { MessageServiceMock } from "../../../../core/messages/message.mock";
import { DialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";

describe("Component BPDiscussionReplyItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("artifactDiscussions", ArtifactDiscussionsMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("dialogService", DialogService);
    }));

    let vm: BPDiscussionReplyItemController;
    let directiveTest: ComponentTest<BPDiscussionReplyItemController>;
    let template = `<bp-discussion-reply-item 
                        reply-info="null"
                        artifact-id="1">
                    </bp-discussion-reply-item>`;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPDiscussionReplyItemController>(template, "bp-discussion-reply-item");
        vm = directiveTest.createComponent({});
        let reply: IReply = {
            "replyId": 1,
            "itemId": 1,
            "discussionId": 2,
            "version": 3,
            "userId": 1,
            "lastEditedOn": "",
            "userName": "Mehdi",
            "isGuest": false,
            "comment": "test comment",
            "canEdit": true,
            "canDelete": false
        };
        vm.replyInfo = reply;
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find(".button-bar").length).toBe(1);
        expect(directiveTest.element.find("bp-avatar").length).toBe(1);
    });

    it("edit reply should return default reply",
        inject(($timeout: ng.ITimeoutService) => {

            //Arrange
            vm.artifactId = 1;

            //Act
            vm.editReply("");
            $timeout.flush();

            //Assert
            expect(vm.replyInfo.comment).toBe("");
        }));

    it("edit reply throws exception",
        inject(($timeout: ng.ITimeoutService, $q: ng.IQService) => {
            //Arrange
            vm.artifactId = 1;
            let deferred = $q.defer();
            ArtifactDiscussionsMock.prototype.editDiscussionReply = jasmine.createSpy("editDiscussionReply() spy").and.callFake(
                (): ng.IPromise<IReply> => {
                    deferred.reject({
                        statusCode: 404,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );

            //Act
            let updatedReply: IReply;
            vm.editReply("").then((result: IReply) => { updatedReply = result; });
            $timeout.flush();

            //Assert
            expect(updatedReply).toBe(null);
        }));

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
            vm.discussionClosed = false;

            //Act
            vm.editCommentClick();

            //Assert
            expect(vm.editing).toBe(true);
        });
});