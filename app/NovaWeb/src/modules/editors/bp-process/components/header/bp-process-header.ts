import { IProjectManager, IWindowManager, IArtifactService } from "../../../../main/services";
import { BpArtifactInfoController } from "../../../../main/components/bp-artifact-info/bp-artifact-info";
import { IMessageService, ILocalizationService, IStateManager } from "../../../../core";
import { IDialogService } from "../../../../shared";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: Function = BpProcessHeaderController;
    public transclude: boolean = true;
}

export class BpProcessHeaderController extends BpArtifactInfoController {
    constructor(
        projectManager: IProjectManager,
        localization: ILocalizationService,
        stateManager: IStateManager,
        messageService: IMessageService,
        dialogService: IDialogService,
        $element: ng.IAugmentedJQuery,
        windowManager: IWindowManager,
        artifactService: IArtifactService
    ) {
        super(
            projectManager,
            localization,
            stateManager,
            messageService,
            dialogService,
            $element,
            windowManager,
            artifactService
        );
    }
}