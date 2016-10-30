import {BPToggleAction, BPToggleItemAction} from "../../../../../shared";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ProcessType} from "../../../models/enums";
import {IToolbarCommunication} from "../toolbar-communication";
import {ILocalizationService} from "../../../../../core";

export class ToggleProcessTypeAction extends BPToggleAction {
    constructor(
        process: StatefulProcessArtifact,
        toolbarCommunication: IToolbarCommunication,
        localization: ILocalizationService
    ) {
        if (!process) {
            throw new Error("Process is not provided or is null");
        }

        if (!toolbarCommunication) {
            throw new Error("Toolbar communication is not provided or is null");
        }

        if (!localization) {
            throw new Error("Localization service is not provided or is null");
        }

        const processType: ProcessType = process.propertyValues["clientType"].value;

        super(
            processType,
            (value: ProcessType) => {
                toolbarCommunication.toggleProcessType(value);
            },
            () => process && process.artifactState && !process.artifactState.readonly,
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
