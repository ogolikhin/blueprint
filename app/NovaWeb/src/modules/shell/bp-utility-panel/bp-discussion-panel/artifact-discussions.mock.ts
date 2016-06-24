import { IArtifactDiscussions, IDiscussion, IReply, IDiscussionResultSet } from "./artifact-discussions.svc";
import { Models } from "../../../main";

export class ArtifactDiscussionsMock implements IArtifactDiscussions {

    public static $inject = ["$q"];

    public artifactDiscussions;

    constructor(private $q: ng.IQService) { }

    public getArtifactDiscussions(
        artifactId: number,
        subArtifactId?: number): ng.IPromise<IDiscussionResultSet> {
        const deferred = this.$q.defer<any>();

        let artifactDiscussions =
            {
                "canCreate": true,
                "canDelete": true,
                "discussions": [
                    {
                        "isClosed": false,
                        "status": "",
                        "itemId": 1,
                        "discussionId": 1,
                        "userId": 1,
                        "lastEditedOn": "2016-05-31T17:19:53.07",
                        "userName": "Mehdi",
                        "isGuest": false,
                        "comment": "This is a test."
                    },
                    {
                        "isClosed": true,
                        "status": "",
                        "itemId": 2,
                        "discussionId": 1,
                        "userId": 2,
                        "lastEditedOn": "",
                        "userName": "Mehdi",
                        "isGuest": false,
                        "comment": "flakdj alkdjf lajd f."
                    }
                ]
            };

        //if (asc) {
        //    artifactHistories = artifactHistories.reverse();
        //}

        deferred.resolve(artifactDiscussions);
        return deferred.promise;
    }

    public getReplies(
        artifactId: number,
        discussionId: number,
        subArtifactId?: number): ng.IPromise<IReply[]> {
        const deferred = this.$q.defer<any[]>();

        let artifactReplies = [
            {
                "replyId": 1,
                "itemId": 1,
                "discussionId": 1,
                "userId": 1,
                "lastEditedOn": "",
                "userName": "Mehdi",
                "isGuest": false,
                "comment": "This is a test."
            }
        ];

        //if (asc) {
        //    artifactHistories = artifactHistories.reverse();
        //}

        deferred.resolve(artifactReplies);
        return deferred.promise;
    }
}

