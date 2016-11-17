import {IMetaDataService} from "./metadata.svc";
import {IProjectMeta, IItemType, IPropertyType} from "../../../main/models/models";

class ProjectMetaData {
    constructor(public id: number, public data: IProjectMeta) {
    }
}

export class MetaDataServiceMock implements IMetaDataService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public get(projectId: number): ng.IPromise<ProjectMetaData> {
        const deferred: ng.IDeferred<ProjectMetaData> = this.$q.defer<ProjectMetaData>();
        const metaData: ProjectMetaData = {
            id: projectId,
            data: this.metaData
        };
        deferred.resolve(metaData);
        return deferred.promise;
    }

    public remove(projectId: number): void {
        return;
    }

    public refresh(projectId: number): ng.IPromise<ProjectMetaData> {
        const deferred = this.$q.defer();

        deferred.resolve();
        return deferred.promise;
    }

    public getArtifactItemType(projectId: number, itemTypeId: number): ng.IPromise<IItemType> {
        const deferred: ng.IDeferred<IItemType> = this.$q.defer<IItemType>();

        deferred.reject();
        return deferred.promise;
    }

    public getArtifactItemTypeTemp(projectId: number, itemTypeId: number): IItemType {
        return null;
    }

    public getSubArtifactItemType(projectId: number, itemTypeId: number): ng.IPromise<IItemType> {
        const deferred: ng.IDeferred<IItemType> = this.$q.defer<IItemType>();

        deferred.reject();
        return deferred.promise;
    }

    public getArtifactPropertyTypes(projectId: number, itemTypeId: number): ng.IPromise<IPropertyType[]> {
        const deferred: ng.IDeferred<IPropertyType[]> = this.$q.defer<IPropertyType[]>();

        deferred.reject();
        return deferred.promise;
    }

    public getSubArtifactPropertyTypes(projectId: number, itemTypeId: number): ng.IPromise<IPropertyType[]> {
        const deferred: ng.IDeferred<IPropertyType[]> = this.$q.defer<IPropertyType[]>();

        deferred.reject();
        return deferred.promise;
    }

    private metaData: IProjectMeta = {
        artifactTypes: [
            {
                id: 208,
                name: "Collection Folder",
                projectId: 1,
                versionId: 1,
                prefix: "CFL",
                predefinedType: 4609,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 209,
                name: "Collection",
                projectId: 1,
                versionId: 1,
                prefix: "ACO",
                predefinedType: 4610,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 191,
                name: "Glossary",
                projectId: 1,
                versionId: 12,
                prefix: "GL",
                predefinedType: 4099,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 194,
                name: "Business Process Diagram",
                projectId: 1,
                versionId: 7,
                prefix: "BP",
                predefinedType: 4103,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 196,
                name: "Use Case",
                projectId: 1,
                versionId: 7,
                prefix: "UC",
                predefinedType: 4105,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 198,
                name: "UI Mockup",
                projectId: 1,
                versionId: 7,
                prefix: "UM",
                predefinedType: 4107,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 199,
                name: "Generic Diagram",
                projectId: 1,
                versionId: 7,
                prefix: "GD",
                predefinedType: 4108,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 201,
                name: "Storyboard",
                projectId: 1,
                versionId: 7,
                prefix: "SB",
                predefinedType: 4111,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 202,
                name: "Domain Diagram",
                projectId: 1,
                versionId: 7,
                prefix: "DD",
                predefinedType: 4112,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 203,
                name: "Use Case Diagram",
                projectId: 1,
                versionId: 7,
                prefix: "UCD",
                predefinedType: 4113,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 205,
                name: "Baseline & Review Folder",
                projectId: 1,
                versionId: 7,
                prefix: "BFL",
                predefinedType: 4353,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 206,
                name: "Baseline",
                projectId: 1,
                versionId: 7,
                prefix: "ABL",
                predefinedType: 4354,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 207,
                name: "Review",
                projectId: 1,
                versionId: 7,
                prefix: "RVP",
                predefinedType: 4355,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 190,
                name: "ST-User Story",
                projectId: 1,
                versionId: 4,
                instanceItemTypeId: 144,
                prefix: "STUS",
                predefinedType: 4101,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 192,
                name: "Textual Requirement",
                projectId: 1,
                versionId: 39,
                prefix: "RQ",
                predefinedType: 4101,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 193,
                name: "Folder",
                projectId: 1,
                versionId: 38,
                prefix: "PF",
                predefinedType: 4102,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 195,
                name: "Actor",
                projectId: 1,
                versionId: 33,
                prefix: "AC",
                predefinedType: 4104,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 200,
                name: "Document",
                projectId: 1,
                versionId: 41,
                prefix: "DOC",
                predefinedType: 4110,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 204,
                name: "Process",
                projectId: 1,
                versionId: 22,
                prefix: "PRO",
                predefinedType: 4114,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }
        ],
        subArtifactTypes: [
            {
                id: 163,
                name: "Document: Bookmark",
                projectId: 1,
                versionId: 1,
                prefix: "BMK",
                predefinedType: 8213,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 164,
                name: "Use Case: Pre Condition",
                projectId: 1,
                versionId: 1,
                prefix: "PRC",
                predefinedType: 8196,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 165,
                name: "Use Case: Post Condition",
                projectId: 1,
                versionId: 1,
                prefix: "POC",
                predefinedType: 8197,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 167,
                name: "Use Case: Step",
                projectId: 1,
                versionId: 1,
                prefix: "ST",
                predefinedType: 8199,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 175,
                name: "Glossary: Term",
                projectId: 1,
                versionId: 1,
                prefix: "TR",
                predefinedType: 8217,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 177,
                name: "Domain Diagram: Connector",
                projectId: 1,
                versionId: 1,
                prefix: "DDCT",
                predefinedType: 8219,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 178,
                name: "Domain Diagram: Shape",
                projectId: 1,
                versionId: 1,
                prefix: "DDSH",
                predefinedType: 8220,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 179,
                name: "Generic Diagram: Connector",
                projectId: 1,
                versionId: 1,
                prefix: "GDCT",
                predefinedType: 8193,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 180,
                name: "Generic Diagram: Shape",
                projectId: 1,
                versionId: 1,
                prefix: "GDSH",
                predefinedType: 8194,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 181,
                name: "Business Process Diagram: Connector",
                projectId: 1,
                versionId: 1,
                prefix: "BPCT",
                predefinedType: 8195,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 182,
                name: "Business Process Diagram: Shape",
                projectId: 1,
                versionId: 1,
                prefix: "BPSH",
                predefinedType: 8221,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 183,
                name: "Storyboard: Connector",
                projectId: 1,
                versionId: 1,
                prefix: "SBCT",
                predefinedType: 8222,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 184,
                name: "Storyboard: Shape",
                projectId: 1,
                versionId: 1,
                prefix: "SBSH",
                predefinedType: 8223,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 185,
                name: "UI Mockup: Connector",
                projectId: 1,
                versionId: 1,
                prefix: "UICT",
                predefinedType: 8224,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 186,
                name: "UI Mockup: Shape",
                projectId: 1,
                versionId: 1,
                prefix: "UISH",
                predefinedType: 8225,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 187,
                name: "Use Case Diagram: Connector",
                projectId: 1,
                versionId: 1,
                prefix: "UCDC",
                predefinedType: 8226,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 188,
                name: "Use Case Diagram: Shape",
                projectId: 1,
                versionId: 1,
                prefix: "UCDS",
                predefinedType: 8227,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }, {
                id: 189,
                name: "Process: Shape",
                projectId: 1,
                versionId: 1,
                prefix: "PROS",
                predefinedType: 8228,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: []
            }
        ],
        propertyTypes: []
    };
}
