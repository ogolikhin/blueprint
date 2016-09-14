import { IProjectManager } from "../../project-manager";

export interface IMetaDataService {
    getArtifactItemType(id: number);
}

export class MetaDataService implements IMetaDataService {

    public static $inject = [
        "$q",
        "projectManager"
    ];

    constructor(
        private $q: ng.IQService,
        private projectManager: IProjectManager) {
    }

    public getArtifactItemType(id: number) {

        return this.projectManager.getArtifactItemType(id);
    }
}
