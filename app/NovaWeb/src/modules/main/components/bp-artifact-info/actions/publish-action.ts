import {BPButtonAction, IDialogSettings, IDialogService} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {ILocalizationService, IMessageService} from "../../../../core";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {Models} from "../../../../main/models";
import {ILoadingOverlayService} from "../../../../core/loading-overlay";
import {ConfirmPublishController, IConfirmPublishDialogData} from "../../../../main/components/dialogs/bp-confirm-publish";

export class PublishAction extends BPButtonAction {
    constructor($q: ng.IQService,
                artifact: IStatefulArtifact,
                localization: ILocalizationService,
                messageService: IMessageService,
                loadingOverlayService: ILoadingOverlayService,
                dialogService: IDialogService) {
        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }
        
        super(
            (): void => {
                let overlayId: number = loadingOverlayService.beginLoading();

                try {
                    let savePromise = $q.defer<any>();
                    if (artifact.canBeSaved()) {
                        savePromise.promise = artifact.save();
                    } else {
                        savePromise.resolve();
                    }

                    savePromise.promise.then(() => {
                        artifact.publish([])
                        .then(() => {
                            messageService.addInfo("Published artifact succesfully");
                        })
                        .catch((err) => {
                            if (err && err.statusCode === 409) {
                                let data: Models.IPublishResultSet = err.data;
                                dialogService.open(<IDialogSettings>{
                                    okButton: localization.get("App_Button_Publish"),
                                    cancelButton: localization.get("App_Button_Cancel"),
                                    message: localization.get("Publish_All_Dialog_Message"),
                                    template: require("../../dialogs/bp-confirm-publish/bp-confirm-publish.html"),
                                    controller: ConfirmPublishController,
                                    css: "nova-messaging" // removed modal-resize-both as resizing the modal causes too many artifacts with ag-grid
                                },
                                <IConfirmPublishDialogData>{
                                    artifactList: data.artifacts,
                                    projectList: data.projects,
                                    selectedProject: artifact.projectId
                                })
                                .then(() => {
                                    artifact.publish(data.artifacts.map((d: Models.IArtifact) => {return d.id; }))
                                    .then(() => {
                                        messageService.addInfo("Published artifact succesfully");
                                    })
                                    .catch((err) => {
                                        messageService.addError(err);
                                    });
                                });
                            }
                            messageService.addError(err);
                        })
                        .finally(() => {
                            loadingOverlayService.endLoading(overlayId);
                        });
                    })
                    .catch(() => {
                        loadingOverlayService.endLoading(overlayId);
                    });
                } catch (err) {
                    loadingOverlayService.endLoading(overlayId);

                    if (err) {
                        messageService.addError(err);
                        throw err;
                    }
                }
            },
            (): boolean => {
                if (!artifact) {
                    return false;
                }

                const invalidTypes = [
                    ItemTypePredefined.Project,
                    ItemTypePredefined.Collections
                ];

                if (invalidTypes.indexOf(artifact.predefinedType) >= 0) {
                    return false;
                }

                if (artifact.artifactState.readonly) {
                    return false;
                }

                return true;
            },
            "fonticon2-publish-line",
            localization.get("App_Toolbar_Publish")
        );
    }
}
