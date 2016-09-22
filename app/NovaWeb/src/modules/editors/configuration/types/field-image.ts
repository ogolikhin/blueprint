﻿import "angular";
import { IArtifactAttachmentsService, IArtifactAttachmentsResultSet } from "../../../managers/artifact-manager";
import { Helper } from "../../../shared/utils/helper";
import { ILocalizationService, IMessageService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { BPFieldBaseController } from "./base-controller";
import { Models } from "../../../main/models";

export class BPFieldImage implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldImage";
    public template: string = require("./field-image.template.html");
    public controller: Function = BPFieldImageController;
    public defaultOptions: AngularFormly.IFieldConfigurationObject;
    constructor() {
        this.defaultOptions = {};
    }
}

export class BPFieldImageController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "$window", "messageService", "dialogService"];

    constructor(
        private $scope: AngularFormly.ITemplateScope,
        private localization: ILocalizationService,
        private $window: ng.IWindowService,
        private messageService: IMessageService,
        private dialogService: IDialogService
    ) {
        super();

        const templateOptions: AngularFormly.ITemplateOptions = $scope["to"];
        let onChange = (templateOptions["onChange"] as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        const maxAttachmentFilesizeDefault: number = 1048576; // 1 MB
        const maxNumberAttachmentsDefault: number = 1;
        const allowedExtensions = ['png', 'jpg', 'jpeg'];

        let setFields = (model: any) => {
            if (model) {
                $scope.model["image"] =  model.image;
            }
        }

        let currentModelVal = $scope.model[$scope.options["key"]];
        //let currentModelVal = <Models.IActorImagePropertyValue>$scope.model[$scope.options.key];
        if (!currentModelVal) {
            currentModelVal = <Models.IActorImagePropertyValue>{};
        }

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
                        $scope.model[$scope.options["key"]] = currentModelVal;
                        onChange(image.file, getImageField(), $scope);
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
            return $scope.fields[0];
        }

        $scope["onFileSelect"] = (files: File[], callback?: Function) => {
            chooseActorImage(files, callback);
        };

        $scope["onActorImageDelete"] = (isReadOnly: boolean) => {
            if(isReadOnly && isReadOnly === true){
                return ;
            }
            onChange(null, getImageField(), $scope);
            $scope.model[$scope.options["key"]] = null;
        };

    }
}