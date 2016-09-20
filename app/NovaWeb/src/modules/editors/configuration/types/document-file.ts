﻿import "angular";
import { IArtifactAttachmentsService, IArtifactAttachmentsResultSet } from "../../../managers/artifact-manager";
import { Helper } from "../../../shared/utils/helper";
import { ILocalizationService, IMessageService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { BPFieldBaseController } from "./base-controller";

export class BPFieldDocumentFile implements AngularFormly.ITypeOptions {
    public name: string = "bpDocumentFile";
    public template: string = require("./document-file.template.html");
    public controller: Function = BPFieldDocumentFileController;
    public defaultOptions: AngularFormly.IFieldConfigurationObject;
    constructor() {
        this.defaultOptions = {};
    }
}

export class BPFieldDocumentFileController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "artifactAttachments", "$window", "messageService", "dialogService"];

    constructor(
        private $scope: AngularFormly.ITemplateScope,
        private localization: ILocalizationService,
        private artifactAttachments: IArtifactAttachmentsService,
        private $window: ng.IWindowService,
        private messageService: IMessageService,
        private dialogService: IDialogService) {
        super();
        let currentModelVal = this.$scope.model[this.$scope.options["key"]];
        let guid: number; //we use this to download newly added files (prior to saving).
        const maxAttachmentFilesize: number = 1048576; // 1 MB
        const maxNumberAttachments: number = 1;
        const templateOptions: any = $scope["to"];
        let setFields = (model: any) => {
            if (model) {
                this.$scope["hasFile"] = true;
                this.$scope["fileName"] = model.fileName;
                this.$scope["extension"] = FiletypeParser.getFiletypeClass(model.fileName);
            }
        }
        let clearFields = () => {
            this.$scope["hasFile"] = false;
            this.$scope["fileName"] = null;
            this.$scope["extension"] = null;
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
                    templateOptions.onChange(newFileObject, $scope.fields[0], $scope);
                    setFields(newFileObject);
                    guid = uploadedFile.guid;
                }
            }).finally(() => {
                if (callback) {
                    callback();
                }
            });
        }

        setFields(currentModelVal);
        $scope["onFileSelect"] = (files: File[], callback?: Function) => {
            chooseDocumentFile(files, callback);
        };
        $scope["downloadFile"] = () => {
            if (guid) {
                $window.open(`/svc/bpfilestore/file/${guid}`, "_blank");
            } else {
                artifactAttachments.getArtifactAttachments(templateOptions.artifactId)
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
            }
        };

        $scope["deleteFile"] = () => {
            if (!templateOptions.isReadOnly) {
                const dialogSettings = <IDialogSettings>{
                    okButton: localization.get("App_Button_Ok", "OK"),
                    header: localization.get("App_UP_Attachments_Delete_Attachment", "Delete Attachment"),
                    message: localization.get("App_UP_Attachments_Delete_Attachment", "Attachment will be deleted. Continue?")
                };
                dialogService.open(dialogSettings).then(() => {
                    templateOptions.onChange(null, $scope.fields[0], $scope);
                    clearFields();
                    guid = null;
                });
            }
        }
        $scope["changeLabelText"] = localization.get("App_UP_Document_File_Change", "Change");
        $scope["uploadLabelText"] = localization.get("App_UP_Document_File_Upload", "Upload");
        $scope["downloadLabelText"] = localization.get("App_UP_Document_File_Download", "Download");
    }
}