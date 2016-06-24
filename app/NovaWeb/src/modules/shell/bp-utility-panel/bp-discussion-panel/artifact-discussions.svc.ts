import { ILocalizationService } from "../../../core";

export interface IArtifactDiscussions {
    artifactDiscussions: ng.IPromise<IDiscussion[]>;
    getArtifactDiscussions(artifactId: number, subArtifactId?: number): ng.IPromise<IDiscussionResultSet>;
    getReplies(artifactId: number, discussionId: number, subArtifactId?: number): ng.IPromise<IReply[]>;
}

export interface IDiscussion extends ICommentBase {
    isClosed: boolean;
    status: string;
}

export interface ICommentBase {
    itemId: number;
    discussionId: number;
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
                    message: (err ? err.Message : "") || this.localization.get("Artifact_NotFound", "Error")
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
            url: `/svc/ArtifactStore/artifacts/${artifactId}/discussions/${discussionId}/replies`,
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
                    message: (err ? err.Message : "") || this.localization.get("Artifact_NotFound", "Error")
                };
                this.$log.error(error);
                defer.reject(error);
            });

        return defer.promise;
    }
}
