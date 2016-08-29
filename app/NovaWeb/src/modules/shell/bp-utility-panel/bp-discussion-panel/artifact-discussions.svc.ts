import { ILocalizationService } from "../../../core";

export interface IArtifactDiscussions {
    artifactDiscussions: ng.IPromise<IDiscussion[]>;
    getArtifactDiscussions(artifactId: number, subArtifactId?: number, timeout?: ng.IPromise<void>): ng.IPromise<IDiscussionResultSet>;
    getReplies(artifactId: number, discussionId: number, subArtifactId?: number): ng.IPromise<IReply[]>;
    addDiscussion(artifactId: number, comment: string): ng.IPromise<IDiscussion>;
    addDiscussionReply(artifactId: number, discussionId: number, comment: string): ng.IPromise<IReply>;
    editDiscussion(artifactId: number, discussionId: number, comment: string): ng.IPromise<IDiscussion>;
    editDiscussionReply(artifactId: number, discussionId: number, replyId: number, comment: string): ng.IPromise<IReply>;
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
    canEdit: boolean;
    canDelete: boolean;
}

export interface IDiscussionResultSet {
    canCreate: boolean;
    canDelete: boolean;
    discussions: IDiscussion[];
    emailDiscussionsEnabled: boolean;
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
        subArtifactId?: number,
        timeout?: ng.IPromise<void>): ng.IPromise<IDiscussionResultSet> {

        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/${artifactId}/discussions`,
            method: "GET",
            params: {
                subArtifactId: subArtifactId
            },
            timeout: timeout
        };

        this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<IDiscussionResultSet>) => {
                defer.resolve(result.data);
            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
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
            .then((result: ng.IHttpPromiseCallbackArg<IReply[]>) => {
                defer.resolve(result.data);
            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
                defer.reject(error);
            });

        return defer.promise;
    }

    public addDiscussion(artifactId: number, comment: string): ng.IPromise<IDiscussion> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/components/RapidReview/artifacts/${artifactId}/discussions`,
            method: "POST",
            data: angular.toJson(comment)
        };

        this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<IDiscussion>) => {
                defer.resolve(result.data);

            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
                defer.reject(error);
            });

        return defer.promise;
    }

    public addDiscussionReply(artifactId: number, discussionId: number, comment: string): ng.IPromise<IReply> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/components/RapidReview/artifacts/${artifactId}/discussions/${discussionId}/reply`,
            method: "POST",
            data: angular.toJson(comment)
        };

        this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<IReply>) => {
                defer.resolve(result.data);
            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
                defer.reject(error);
            });

        return defer.promise;
    }

    public editDiscussion(artifactId: number, discussionId: number, comment: string): ng.IPromise<IDiscussion> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/components/RapidReview/artifacts/${artifactId}/discussions/${discussionId}`,
            method: "PATCH",
            data: angular.toJson(comment)
        };

        this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<IDiscussion>) => {
                defer.resolve(result);

            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
                defer.reject(error);
            });

        return defer.promise;
    }

    public editDiscussionReply(artifactId: number, discussionId: number, replyId: number, comment: string): ng.IPromise<IReply> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/components/RapidReview/artifacts/${artifactId}/discussions/${discussionId}/reply/${replyId}`,
            method: "PATCH",
            data: angular.toJson(comment)
        };

        this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<IReply>) => {
                defer.resolve(result);

            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: errResult.data ? errResult.data.message : "Artifact_NotFound"
                };
                defer.reject(error);
            });

        return defer.promise;
    }

    public deleteReply(itemId: number, replyId: number): ng.IPromise<boolean> {
        const defer = this.$q.defer<any>();
        const requestObj: ng.IRequestConfig = {
            url: `/svc/components/RapidReview/artifacts/${itemId}/deletecomment/${replyId}`,
            method: "DELETE"
        };
        this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.resolve(true);
            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                let msg: string;
                if (!errResult) {
                    defer.reject();
                    return;
                }
                if (errResult.status === 404) {
                    msg = this.localization.get("Error_Comment_Deleted", "Error");
                } else {
                    msg = (errResult.data ? errResult.data.message : "");
                }
                const error = {
                    statusCode: errResult.status,
                    message: msg
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
            method: "DELETE"
        };
        this.$http(requestObj)
            .then((result: ng.IHttpPromiseCallbackArg<any>) => {
                defer.resolve(true);
            }, (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                let msg: string;
                if (!errResult) {
                    defer.reject();
                    return;
                }
                if (errResult.status === 404) {
                    msg = this.localization.get("Error_Comment_Deleted", "Error");
                } else {
                    msg = (errResult.data ? errResult.data.message : "");
                }
                const error = {
                    statusCode: errResult.status,
                    message: msg
                };
                this.$log.error(error);
                defer.reject(error);
            });
        return defer.promise;
    }
}
