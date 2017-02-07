import "rx";
import {IPropertyValue} from "../../../../../main/models/models";
import {IItemChangeSet} from "../../../../../managers/artifact-manager/changeset";
import {IArtifactState} from "../../../../../managers/artifact-manager";
import {BPToggleAction, BPToggleItemAction} from "../../../../../shared";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ProcessType} from "../../../models/enums";
import {PropertyTypePredefined, ReuseSettings} from "../../../../../main/models/enums";
import {IToolbarCommunication} from "../toolbar-communication";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";

export class ToggleProcessTypeAction extends BPToggleAction {
    private subscribers: Rx.IDisposable[];
    private loaded: boolean;

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
            () => {
                return this.loaded &&
                        process &&
                        process.artifactState &&
                        !process.artifactState.readonly &&
                        //artifact is selected and selective readonly is set
                        !process.isReuseSettingSRO(ReuseSettings.Subartifacts);
            },
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
            process.getObservable().subscribeOnNext(this.onArtifactLoaded, this),
            process.getPropertyObservable().subscribeOnNext(this.onPropertyChanged, this)
        );
    }

    public dispose(): void {
        if (this.subscribers) {
            this.subscribers.forEach(
                (subscriber: Rx.IDisposable) => {
                    subscriber.dispose();
                }
            );

            this.subscribers = undefined;
        }
    }

    private onPropertyChanged(changeSet: IItemChangeSet): void {
        if (changeSet && changeSet.change) {
            const value = <IPropertyValue>changeSet.change.value;

            if (value && value.propertyTypePredefined === PropertyTypePredefined.ClientType) {
                this.onProcessTypeChanged(<StatefulProcessArtifact>changeSet.item);
            }
        }
    }

    private onArtifactLoaded(process: StatefulProcessArtifact): void {
        if (process) {
            this.onProcessTypeChanged(process);
            this.loaded = true;
        }
    }

    private onProcessTypeChanged(process: StatefulProcessArtifact): void {
        if (!process || !process.propertyValues) {
            return;
        }

        this._currentValue = process.propertyValues["clientType"].value;
    }
}
