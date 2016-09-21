import "angular";
import { ILocalizationService, IMessageService, ISettingsService } from "../../../core";
import { Helper } from "../../../shared/utils/helper";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { Models } from "../../../main/models";
// import { FiletypeParser } from "../../../shared/utils/filetypeParser";

actorImageController.$inject = ["localization", "$window", "messageService", "dialogService", "settingsService"];
export function actorImageController(
    $scope: any,
    localization: ILocalizationService,
    $window: ng.IWindowService,
    messageService: IMessageService,
    dialogService: IDialogService,
    settingsService: ISettingsService
    ) {
    let currentModelVal = <Models.IActorImagePropertyValue>$scope.model[$scope.options.key]; 
    if (!currentModelVal) {
        currentModelVal = <Models.IActorImagePropertyValue>{};
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
                    reader.readAsDataURL(image.file);                    

                    reader.onload = function (e) {                       
                        currentModelVal.url = e.target['result'];
                        currentModelVal.guid = image.guid;                        
                        $scope.model[$scope.options.key] = currentModelVal;        
                        $scope.to.onChange(image.file, getImageField(), $scope);
                    }
                }
            }).finally(() => {
                if (callback) {
                    callback();
                }
            });
    }

    function getImageField(): any {
        if (!$scope.fields) {
            return null;
        }
        return $scope.fields.find((field: any) => field.key === "image");
    }

    $scope.onFileSelect = (files: File[], callback?: Function) => {
        chooseActorImage(files, callback);
    };

    $scope.onActorImageDelete = (isReadOnly: boolean) => {
        if(isReadOnly && isReadOnly === true){
            return ;
        }
        $scope.to.onChange(null, getImageField(), $scope);
        $scope.model.image = null;
    };

}