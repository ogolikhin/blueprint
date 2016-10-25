import {IPublishService} from "./publish.svc";
import { Models, Enums } from "../../../main/models";

export class PublishServiceMock implements IPublishService {
    public publishAll(): ng.IPromise<Models.IPublishResultSet> {
        return null;
    }
    public getUnpublishedArtifacts(): ng.IPromise<Models.IPublishResultSet> {
        return null;
    }
    public publishArtifacts(artifactIds: number[]): ng.IPromise<Models.IPublishResultSet> {
        return null;
    }
}
