import "../../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { IDiscussion } from "../artifact-discussions.svc";
import { BPArtifactDiscussionItemController } from "./bp-artifact-discussion-item";
import { HttpStatusCode } from "../../../../core/http";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import { ArtifactDiscussionsMock } from "../artifact-discussions.mock";
import { MessageServiceMock } from "../../../../core/messages/message.mock";
import { DialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";

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
            vm.discussionEdited = () => { };

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
            vm.discussionEdited = () => { };
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
            vm.editDiscussion("").then((result: IDiscussion) => { updatedDiscussion = result; });
            $timeout.flush();

            //Assert
            expect(updatedDiscussion).toBe(null);
        }));

    it("new reply click shows add reply",
        () => {
            //Arrange
            vm.canCreate = true;
            vm.cancelComment = () => { };

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