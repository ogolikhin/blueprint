import { IMessageService } from "../../core/";
import { Models } from "../../main/models";
import { IArtifactManager, IStatefulArtifact, ISession, IStatefulArtifactServices, IArtifactAttachmentsService } from "../models";
import { StatefulArtifact } from "./artifact";


export class ArtifactManager  implements IArtifactManager {

    public static $inject = [
        "$http", 
        "$q",
        "session",
        "messageService",
        "attachmentService"
    ];

    private artifactList: IStatefulArtifact[];
    private services: IStatefulArtifactServices;

    constructor(
        private $http: ng.IHttpService, 
        private $q: ng.IQService,
        private session: ISession,
        private messageService: IMessageService,
        private attachmentService: IArtifactAttachmentsService) {

        this.services = {
            $q : this.$q,
            messageService : this.messageService,
            attachmentService: this.attachmentService
        } as IStatefulArtifactServices;

        this.artifactList = [];
    }

    public get currentUser(): Models.IUserGroup {
        return this.session.currentUser;
    }

    public get messages(): IMessageService {
        return this.messageService;
    }

    public list(): IStatefulArtifact[] {
        return this.artifactList;
    }

    public get(id: number): IStatefulArtifact {
        return this.artifactList.filter((artifact: IStatefulArtifact) => artifact.id === id)[0] || null;
    }
    
    public add(artifact: Models.IArtifact): IStatefulArtifact {
        let length = this.artifactList.push(new StatefulArtifact(this, artifact, this.services));
        return this.artifactList[length - 1];
    }

    public remove(id: number): IStatefulArtifact {
        let stateArtifact: IStatefulArtifact;
        this.artifactList = this.artifactList.filter((artifact: IStatefulArtifact) => {
            if (artifact.id === id) {
                stateArtifact = artifact;
                return false;
            }
            return true;
        });
        return stateArtifact;
    }

    public update(id: number) {
        // TODO: 
    }


    public request<T>(request: ng.IRequestConfig): ng.IPromise<T> {
        var defer = this.services.$q.defer<T>();
        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<T>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                };
                defer.reject(error);
            }
        );
        return defer.promise;

    }

}
