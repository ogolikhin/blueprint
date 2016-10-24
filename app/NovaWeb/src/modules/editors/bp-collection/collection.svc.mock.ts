import {ICollectionService} from "./collection.svc";
import {IArtifact} from "../../main/models/models";
import {ICollection, ICollectionArtifact} from "./models";
import {Models} from "../../main";

export class CollectionServiceMock implements ICollectionService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public getCollection(id: number): ng.IPromise<ICollection> {
        const defer = this.$q.defer<ICollection>();

        if (id > 0) {
            defer.resolve(CollectionServiceMock.createCollection(id));
        } else {
            defer.reject("Error");
        }

        return defer.promise;
    }

    public static createCollection(id: number): ICollection {
        /* tslint:disable:max-line-length */
        const collection: ICollection = {            
            id: id,
            projectId: 1,
            reviewName: "Review1",
            isCreated: true,
            itemTypeId: 5,
            name: "Collection1",
            artifacts: [
                <ICollectionArtifact>{
                    id: 264,
                    name: "fleek",
                    description: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">on point</p></div></body></html>",
                    prefix: "TR",
                    itemTypeId: 5,
                    itemTypePredefined: Models.ItemTypePredefined.Actor,
                    artifactPath: "Path1"                   
                },
                <ICollectionArtifact>{
                    id: 386,
                    name: "google",
                    description: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a href=\"http://www.google.com/\" style=\"color: Blue; text-decoration: underline\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px\">google.com</span></a><span style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; color: Black\">&#x200b;</span></p></div></body></html>",
                    prefix: "TR",
                    itemTypeId: 5,
                    itemTypePredefined: Models.ItemTypePredefined.Actor,
                    artifactPath: "Path1"  
                },
                <ICollectionArtifact>{
                    id: 382,
                    name: "pokemon",
                    description: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">cat thing</p></div></body></html>",
                    prefix: "TR",
                    itemTypeId: 5,
                    itemTypePredefined: Models.ItemTypePredefined.Actor,
                    artifactPath: "Path1"  
                },
                <ICollectionArtifact>{
                    id: 385,
                    name: "snorlax",
                    description: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">a kind of&nbsp;<span style=\"font-weight: bold\">pokemon</span></p></div></body></html>",
                    prefix: "TR",
                    itemTypeId: 5,
                    itemTypePredefined: Models.ItemTypePredefined.Actor,
                    artifactPath: "Path1"  
                }
            ]
        };

        return collection;
        /* tslint:enable:max-line-length */
    }
}
