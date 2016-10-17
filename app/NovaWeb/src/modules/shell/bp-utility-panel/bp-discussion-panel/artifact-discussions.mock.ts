import {IArtifactDiscussions, IDiscussion, IReply, IDiscussionResultSet} from "./artifact-discussions.svc";

export class ArtifactDiscussionsMock implements IArtifactDiscussions {

    public static $inject = ["$q"];

    public artifactDiscussions;

    constructor(private $q: ng.IQService) {
    }

    public getArtifactDiscussions(artifactId: number,
                                  subArtifactId?: number): ng.IPromise<IDiscussionResultSet> {
        const deferred = this.$q.defer<any>();

        let artifactDiscussions = {
            "canCreate": true,
            "canDelete": true,
            "discussions": [
                {
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
                    "showAddReply": false
                },
                {
                    "isClosed": true,
                    "status": "",
                    "itemId": 2,
                    "repliesCount": 0,
                    "discussionId": 2,
                    "version": 4,
                    "userId": 2,
                    "lastEditedOn": "",
                    "userName": "Mehdi",
                    "isGuest": false,
                    "comment": "flakdj alkdjf lajd f.",
                    "canEdit": true,
                    "canDelete": false,
                    "showAddReply": true
                }
            ]
        };

        //if (asc) {
        //    artifactHistories = artifactHistories.reverse();
        //}

        deferred.resolve(artifactDiscussions);
        return deferred.promise;
    }

    public getReplies(artifactId: number,
                      discussionId: number,
                      subArtifactId?: number): ng.IPromise<IReply[]> {
        const deferred = this.$q.defer<any[]>();

        let artifactReplies = [
            {
                "replyId": 1,
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

        //if (asc) {
        //    artifactHistories = artifactHistories.reverse();
        //}

        deferred.resolve(artifactReplies);
        return deferred.promise;
    }

    public addDiscussion(artifactId: number, comment: string): ng.IPromise<IDiscussion> {
        const deferred = this.$q.defer<any>();

        let discussion = {
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
            "comment": comment,
            "canEdit": true,
            "canDelete": false
        };

        deferred.resolve(discussion);
        return deferred.promise;
    }

    public addDiscussionReply(artifactId: number, discussionId: number, comment: string): ng.IPromise<IReply> {
        const deferred = this.$q.defer<any>();

        let reply = {
            "replyId": 1,
            "itemId": 1,
            "discussionId": discussionId,
            "version": 3,
            "userId": 1,
            "lastEditedOn": "",
            "userName": "Mehdi",
            "isGuest": false,
            "comment": comment,
            "canEdit": true,
            "canDelete": false
        };

        deferred.resolve(reply);
        return deferred.promise;
    }

    public editDiscussion(artifactId: number, discussionId: number, comment: string): ng.IPromise<IDiscussion> {
        const deferred = this.$q.defer<any>();

        let discussion = {
            "isClosed": false,
            "status": "",
            "itemId": 1,
            "repliesCount": 1,
            "discussionId": discussionId,
            "version": 3,
            "userId": 1,
            "lastEditedOn": "2016-05-31T17:19:53.07",
            "userName": "Mehdi",
            "isGuest": false,
            "comment": comment,
            "canEdit": true,
            "canDelete": false
        };

        deferred.resolve(discussion);
        return deferred.promise;
    }

    public editDiscussionReply(artifactId: number, discussionId: number, replyId: number, comment: string): ng.IPromise<IReply> {
        const deferred = this.$q.defer<any>();

        let reply = {
            "replyId": replyId,
            "itemId": 1,
            "discussionId": discussionId,
            "version": 3,
            "userId": 1,
            "lastEditedOn": "",
            "userName": "Mehdi",
            "isGuest": false,
            "comment": comment,
            "canEdit": true,
            "canDelete": false
        };

        deferred.resolve(reply);
        return deferred.promise;
    }

    public deleteCommentThread(itemId: number, discussionId: number): ng.IPromise<boolean> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }

    public deleteReply(itemId: number, replyId: number): ng.IPromise<boolean> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }
}
