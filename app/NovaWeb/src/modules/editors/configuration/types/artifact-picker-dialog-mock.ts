import { IDialogSettings, IDialogService } from "../../.";
import { DialogTypeEnum } from "../../../shared/widgets/bp-dialog/bp-dialog";
import { Models } from "../../.";

export class ArtifactPickerDialogServiceMock implements IDialogService {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }

    public open(dialogSettings: IDialogSettings): ng.IPromise<any> {

        const deferred = this.$q.defer<Models.IArtifact>();

        let parentArtifact: any = {
            name: "parent"
        };

        let artifact: Models.IArtifact = {
            artifacts: [],
            createdBy: undefined,
            createdOn: undefined,            
            id: 10,    
            prefix: "prefix",    
            name: "actor name",
            parent: parentArtifact
        };
        deferred.resolve(artifact);
        return deferred.promise;
    }
    public alert(message: string, header?: string): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }
    public confirm(message: string, header?: string): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }
    public dialogSettings: IDialogSettings = {
        type: DialogTypeEnum.Base,
        header: "test",
        message: "test",
        cancelButton: "test",
        okButton: "test",
        template: "test",
        controller: null,
        css: null,
        backdrop: false
    };
}
