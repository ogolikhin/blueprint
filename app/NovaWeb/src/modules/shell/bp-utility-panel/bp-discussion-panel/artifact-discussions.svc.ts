import { ILocalizationService } from "../../../core";

export interface IArtifactDiscussions {
    artifactDiscussions: ng.IPromise<IDiscussion[]>;
    getArtifactDiscussions(artifactId: number, subArtifactId?: number): ng.IPromise<IDiscussionResultSet>;
    getReplies(artifactId: number, discussionId: number, subArtifactId?: number): ng.IPromise<IReply[]>;
    deleteReply(itemId: number, replyId: number): ng.IPromise<boolean>;
    deleteCommentThread(itemId: number, discussionId: number): ng.IPromise<boolean>;
}

export interface IDiscussion extends ICommentBase {
    isClosed: boolean;
    status: string;
    repliesCount: number;
    replies: IReply[];
    expanded: boolean;
    showAddReply: boolean;
}

export interface ICommentBase {
    itemId: number;
    discussionId: number;
    version: number;
    userId: number;
    lastEditedOn: string;
    userName: string;
    isGuest: boolean;
    comment: string;
}

export interface IDiscussionResultSet {
    canCreate: boolean;
    canDelete: boolean;
    discussions: IDiscussion[];
}

export interface IReply extends ICommentBase {
    replyId: number;
}

export class ArtifactDiscussions implements IArtifactDiscussions {
    static $inject: [string] = [
        "$q",
        "$http",
        "$log",
        "localization"];

    public artifactDiscussions: ng.IPromise<IDiscussion[]>;

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private $log: ng.ILogService,
        private localization: ILocalizationService) {
    }

    public getArtifactDiscussions(
        artifactId: number,
        subArtifactId?: number): ng.IPromise<IDiscussionResultSet> {

        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/discussions`,
            method: "GET",
            params: {
                subArtifactId: subArtifactId
            }
        };

        this.$http(requestObj)
            .success((result: IDiscussionResultSet) => {
                defer.resolve(result);

            }).error((err: any, statusCode: number) => {
                const error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });

        return defer.promise;
    }

    public getReplies(
        artifactId: number,
        discussionId: number,
        subArtifactId?: number): ng.IPromise<IReply[]> {

        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/discussions/${discussionId}/replies`,
            method: "GET",
            params: {
                subArtifactId: subArtifactId
            }
        };

        this.$http(requestObj)
            .success((result: IReply[]) => {
                defer.resolve(result);

            }).error((err: any, statusCode: number) => {
                const error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });

        return defer.promise;
    }

    public deleteReply(itemId: number, replyId: number): ng.IPromise<boolean> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/components/RapidReview/artifacts/${itemId}/deletecomment/${replyId}`,
            method: "POST"
        };
        this.$http(requestObj)
            .success(() => {
                defer.resolve(true);
            }).error((err: any, statusCode: number) => {
                const error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "")
                };
                this.$log.error(error);
                defer.reject(error);
            });
        return defer.promise;
    }

    public deleteCommentThread(itemId: number, discussionId: number): ng.IPromise<boolean> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/components/RapidReview/artifacts/${itemId}/deletethread/${discussionId}`,
            method: "POST"
        };
        this.$http(requestObj)
            .success(() => {
                defer.resolve(true);
            }).error((err: any, statusCode: number) => {
                const error = {
                    statusCode: statusCode,
                    message: (err ? err.message : "")
                };
                this.$log.error(error);
                defer.reject(error);
            });
        return defer.promise;
    }
}
