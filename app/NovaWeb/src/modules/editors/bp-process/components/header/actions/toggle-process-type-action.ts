import {BPToggleAction, BPToggleItemAction} from "../../../../../shared";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ProcessType} from "../../../models/enums";
import {IToolbarCommunication} from "../toolbar-communication";
import {ILocalizationService} from "../../../../../core/localization/localizationService";

export class ToggleProcessTypeAction extends BPToggleAction {
    private subscribers: Rx.IDisposable[];

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

        super(
            ProcessType.None,
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

        this.subscribers = [];
        this.subscribers.push(
            process.getObservable().subscribeOnNext(
                (process: StatefulProcessArtifact) => {
                    this._currentValue = process.propertyValues["clientType"].value;
                }
            )
        );
    }

    public dispose(): void {
        if (this.subscribers) {
            this.subscribers.forEach(
                (subscriber: Rx.IDisposable) => {
                    subscriber.dispose();
                }
            );

            delete this["subscribers"];
        }
    }
}
