import "angular";
import { IArtifactAttachmentsService, IArtifactAttachmentsResultSet } from "../../../managers/artifact-manager";
import { Helper } from "../../../shared/utils/helper";
import { ILocalizationService, IMessageService, ISettingsService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";

documentController.$inject = ["localization", "artifactAttachments", "$window", "messageService"];
export function documentController(
    $scope: any,
    localization: ILocalizationService,
    artifactAttachments: IArtifactAttachmentsService,
    $window: ng.IWindowService,
    messageService: IMessageService,
    dialogService: IDialogService,
    settingsService: ISettingsService
) {
    let currentModelVal = $scope.model[$scope.options.key];
    const maxAttachmentFilesize: number = 1048576; // 1 MB
    const maxNumberAttachments: number = 1;
    let setFields = (model: any) => {
        if (model) {
        $scope.hasFile = true;
            $scope.fileName = model.fileName;
            $scope.extension = FiletypeParser.getFiletypeClass(model.fileName);
        }
    }
    let clearFields = () => {
        $scope.hasFile = false;
        $scope.fileName = null;
        $scope.extension = null;
    }
    let chooseDocumentFile = (files: File[], callback?: Function) => {
        const dialogSettings = <IDialogSettings>{
            okButton: localization.get("App_Button_Ok", "OK"),
            template: require("../../../shared/widgets/bp-file-upload-status/bp-file-upload-status.html"),
            controller: BpFileUploadStatusController,
            css: "nova-file-upload-status",
            header: localization.get("App_UP_Attachments_Upload_Dialog_Header", "File Upload"),
            backdrop: false
        };
        const dialogData: IUploadStatusDialogData = {
            files: files,
            maxAttachmentFilesize: maxAttachmentFilesize,
            maxNumberAttachments: maxNumberAttachments
        };
        let ds = dialogService.open(dialogSettings, dialogData).then((uploadList: any[]) => {
            if (uploadList && uploadList.length > 0) {
                let uploadedFile = uploadList[0];
                const fileExt: RegExpMatchArray = uploadedFile.name.match(/([^.]*)$/);
                let newFileObject = {
                    fileName: uploadedFile.name,
                    fileExtension: fileExt[0] ? fileExt[0] : "",
                    fileGuid: uploadedFile.guid,
                    filePath: uploadedFile.url
                };
                $scope.to.onChange(newFileObject, $scope.fields[0], $scope);
                setFields(newFileObject);
            }
        }).finally(() => {
            if (callback) {
                callback();
            }
        });
    }

    setFields(currentModelVal);
    $scope.onFileSelect = (files: File[], callback?: Function) => {
        chooseDocumentFile(files, callback);
    };
        $scope.downloadFile = () => {
            return artifactAttachments.getArtifactAttachments($scope.fields[0].templateOptions.artifactId)
                .then((attachmentResultSet: IArtifactAttachmentsResultSet) => {
                    if (attachmentResultSet.attachments.length) {
                        $window.open(
                            "/svc/components/RapidReview/artifacts/" + attachmentResultSet.artifactId
                            + "/files/" + attachmentResultSet.attachments[0].attachmentId + "?includeDraft=true",
                            "_blank");
                    } else {
                        messageService.addError(localization.get("App_UP_Attachments_Download_No_Attachment"));
                    }
                });
        };

    $scope.deleteFile = () => {
        const dialogSettings = <IDialogSettings> {
            okButton: localization.get("App_Button_Ok", "OK"),
            header: localization.get("App_UP_Attachments_Delete_Attachment", "Delete Attachment"),
            message: localization.get("App_UP_Attachments_Delete_Attachment", "Attachment will be deleted. Continue?")
        };
        dialogService.open(dialogSettings).then(() => {
            $scope.to.onChange(null, $scope.fields[0], $scope);
            clearFields();
        });
    }
}