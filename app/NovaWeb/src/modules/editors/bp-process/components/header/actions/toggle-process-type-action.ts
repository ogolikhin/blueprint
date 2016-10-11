import {BPToggleAction, BPToggleItemAction} from "../../../../../shared";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ProcessType} from "../../../models/enums";
import {IToolbarCommunication} from "../toolbar-communication";
import {ILocalizationService} from "../../../../../core";

export class ToggleProcessTypeAction extends BPToggleAction {
    constructor(
        processArtifact: StatefulProcessArtifact,
        toolbarCommunication: IToolbarCommunication,
        localization: ILocalizationService
    ) {
        const processType: ProcessType = processArtifact.propertyValues["clientType"].value;

        super(
            processType,
            (value: ProcessType) => {
                toolbarCommunication.toggleProcessType(value);
            },
            () => !processArtifact.artifactState.readonly,
            new BPToggleItemAction(
                "fonticon fonticon2-user-user",
                ProcessType.BusinessProcess,
                false,
                localization.get("ST_ProcessType_BusinessProcess_Label")
            ),
            new BPToggleItemAction(
                "fonticon fonticon2-user-system",
                ProcessType.UserToSystemProcess,
                false,
                localization.get("ST_ProcessType_UserToSystemProcess_Label") 
            )
        );
    }
}