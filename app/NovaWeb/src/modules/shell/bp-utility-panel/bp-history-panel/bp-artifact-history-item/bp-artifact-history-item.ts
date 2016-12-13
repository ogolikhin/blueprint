import {ILocalizationService} from "../../../../core/localization/localizationService";
import {ArtifactStateEnum} from "../../../../main/models/models";

export class BPArtifactHistoryItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-history-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPArtifactHistoryItemController;
    public bindings: any = {
        artifactInfo: "<"
    };
}

export class BPArtifactHistoryItemController {
    public static $inject: [string] = [
        "$log",
        "localization"
    ];

    public getArtifactState: Function = (state: ArtifactStateEnum) => ArtifactStateEnum[state];

    constructor(private $log: ng.ILogService,
                private localization: ILocalizationService) {
    }
}
