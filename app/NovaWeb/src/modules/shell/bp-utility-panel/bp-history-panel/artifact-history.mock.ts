import { IArtifactHistory, IArtifactHistoryVersion } from "./artifact-history.svc"

export class ArtifactHistoryMock implements IArtifactHistory {

    public static $inject = ["$q"];

    public artifactHistory;

    constructor(private $q: ng.IQService) { }

    public getArtifactHistory(artifactId: number, limit?: number, offset?: number, userId?: string, asc?: boolean): ng.IPromise<IArtifactHistoryVersion[]> {
        const deferred = this.$q.defer<any[]>();
        
        let artifactHistories = [
            {
            "versionId": 2147483647,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": null
            },
            {
            "versionId": 52,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-06-06T13:58:24.557"
            },
            {
            "versionId": 51,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-05-31T17:19:53.07"
            },
            {
            "versionId": 50,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-05-30T20:06:23.377"
            },
            {
            "versionId": 49,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-05-30T20:06:05.17"
            },
            {
            "versionId": 48,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-05-30T17:31:32.573"
            },
            {
            "versionId": 47,
            "userId": 5,
            "displayName": "pavlo",
            "hasUserIcon": false,
            "timestamp": "2016-05-27T18:37:39.92"
            },
            {
            "versionId": 46,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-05-27T18:37:14.243"
            },
            {
            "versionId": 45,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-05-27T18:36:57.93"
            },
            {
            "versionId": 44,
            "userId": 1,
            "displayName": "admin",
            "hasUserIcon": false,
            "timestamp": "2016-05-27T18:36:47.443"
            },
            {
            "versionId": 43,
            "userId": 5,
            "displayName": "pavlo",
            "hasUserIcon": false,
            "timestamp": "2016-05-27T15:16:00.037"
            }
        ];

        if (asc) {
            artifactHistories = artifactHistories.reverse();
        }

        deferred.resolve(artifactHistories);
        return deferred.promise;
    }
}

