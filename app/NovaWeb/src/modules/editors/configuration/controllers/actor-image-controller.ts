import "angular";
import { IArtifactAttachments, IArtifactAttachmentsResultSet } from "../../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
import { ILocalizationService, IMessageService, ISettingsService } from "../../../core";
import { Helper } from "../../../shared/utils/helper";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";

actorImageController.$inject = ["localization", "artifactAttachments", "$window", "messageService", "dialogService", "settingsService"];
export function actorImageController(
    $scope: any,
    localization: ILocalizationService,
    artifactAttachments: IArtifactAttachments,
    $window: ng.IWindowService,
    messageService: IMessageService,
    dialogService: IDialogService,
    settingsService: ISettingsService
    ) {
    let currentModelVal = $scope.model[$scope.options.key];
    if (currentModelVal) {
        $scope.actorId =  currentModelVal["actorId"];
    }

    const maxAttachmentFilesizeDefault: number = 1048576; // 1 MB
    const maxNumberAttachmentsDefault: number = 1;
    const allowedExtensions = ['png', 'jpg', 'jpeg'];

    function chooseActorImage(files: File[], callback?: Function) {
        const dialogSettings = <IDialogSettings>{
            okButton: localization.get("App_Button_Ok", "OK"),
            template: require("../../../shared/widgets/bp-file-upload-status/bp-file-upload-status.html"),
            controller: BpFileUploadStatusController,
            css: "nova-file-upload-status",
            header: localization.get("App_UP_Attachments_Upload_Dialog_Header", "File Upload"),
            backdrop: false
        };

        let maxAttachmentFilesize: number = settingsService.getNumber("MaxAttachmentFilesize", maxAttachmentFilesizeDefault);
        let maxNumberAttachments: number = maxNumberAttachmentsDefault;

        if (maxNumberAttachments < 0 || !Helper.isInt(maxNumberAttachments)) {
            maxNumberAttachments = maxNumberAttachmentsDefault;
        }

        if (maxAttachmentFilesize < 0 || !Helper.isInt(maxAttachmentFilesize)) {
            maxAttachmentFilesize = maxAttachmentFilesizeDefault;
        }

        const dialogData: IUploadStatusDialogData = {
            files: files,
            maxAttachmentFilesize: maxAttachmentFilesizeDefault,
            maxNumberAttachments: maxNumberAttachmentsDefault,
            allowedExtentions: allowedExtensions
        };


        let ds = dialogService.open(dialogSettings, dialogData).then((uploadList: any[]) => {
                // TODO: add state manager handling

                if (uploadList && uploadList.length > 0) {
                    let image = uploadList[0];
                    var reader = new FileReader();
                    reader.readAsDataURL(image.file)

                    reader.onload = function(e) {
                        $scope.model.image = e.target['result'];
                    }
                }
            }).finally(() => {
                if (callback) {
                    callback();
                }
            });
    }

    $scope.onFileSelect = (files: File[], callback?: Function) => {
        chooseActorImage(files, callback);
    };

    $scope.onActorImageDelete = (isReadOnly: boolean) => {
        if(isReadOnly && isReadOnly === true){
            return ;
        }
        $scope.model.image = null;
    };

}