import {BPButtonAction} from "../../../../../shared";
import {IStatefulArtifact} from "../../../../../managers/artifact-manager";
import {ItemTypePredefined} from "../../../../../main/models/enums";
import {IMessageService} from "../../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ProcessType} from "../../../models/enums";
import {IToolbarCommunication} from "../toolbar-communication";

export class CopyAction extends BPButtonAction {
    private subscribers: Rx.IDisposable[];
    private loaded: boolean;

    constructor(
        process: StatefulProcessArtifact,
        toolbarCommunication: IToolbarCommunication,
        localization: ILocalizationService) {

        if (!process) {
            throw new Error("Process is not provided or is null");
        }

        if (!toolbarCommunication) {
            throw new Error("Toolbar communication is not provided or is null");
        }

        if (!localization) {
            throw new Error("Localization service is not provided or is null");
        }
        super(
            (): void => {
          
                if (process.hasSelection) {
                    toolbarCommunication.copySelection();
                }
            },
            (): boolean => {

                if (process.hasSelection) {
                    return true;
                }

                return false;
            },
            "fonticon2-copy-shapes",
            localization.get("App_Toolbar_Copy_Shapes")
        );
    }
    
}
