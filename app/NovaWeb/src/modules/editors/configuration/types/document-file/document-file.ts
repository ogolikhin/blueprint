import {IArtifactAttachmentsService, IArtifactAttachmentsResultSet} from "../../../../managers/artifact-manager";
import {Helper} from "../../../../shared/utils/helper";
import {FiletypeParser} from "../../../../shared/utils/filetypeParser";
import {IDialogSettings, IDialogService} from "../../../../shared";
import {IUploadStatusDialogData} from "../../../../shared/widgets";
import {BpFileUploadStatusController} from "../../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import {BPFieldBaseController} from "../base-controller";
import {ISettingsService} from "../../../../core/configuration/settings";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class BPFieldDocumentFile implements AngularFormly.ITypeOptions {
    public name: string = "bpDocumentFile";
    public template: string = require("./document-file.template.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPFieldDocumentFileController;
    public defaultOptions: AngularFormly.IFieldConfigurationObject;

    constructor() {
        this.defaultOptions = {};
    }
}

export class BPFieldDocumentFileController extends BPFieldBaseController {
    static $inject: [string] = [
        "$scope",
        "localization",
        "artifactAttachments",
        "$window",
        "messageService",
        "dialogService",
        "settings"
    ];

    constructor(private $scope: AngularFormly.ITemplateScope,
                private localization: ILocalizationService,
                private artifactAttachments: IArtifactAttachmentsService,
                private $window: ng.IWindowService,
                private messageService: IMessageService,
                private dialogService: IDialogService,
                private settings: ISettingsService) {
        super();
        const maxAttachmentFilesizeDefault: number = 10485760; // 10 MB
        const maxNumberAttachments: number = 1;

        const templateOptions: AngularFormly.ITemplateOptions = $scope["to"];
        let onChange = (templateOptions["onChange"] as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.

        let guid: number; //we use this to download newly added files (prior to saving).

        let maxAttachmentFilesize: number = this.settings.getNumber("MaxAttachmentFilesize", maxAttachmentFilesizeDefault);
        if (maxAttachmentFilesize < 0 || !Helper.isInt(maxAttachmentFilesize)) {
            maxAttachmentFilesize = maxAttachmentFilesizeDefault;
        }

        this.$scope["getFilename"] = () => {
            return this.$scope.model[this.$scope.options["key"]]["fileName"];
        };

        this.$scope["getExtension"] = () => {
            return FiletypeParser.getFiletypeClass($scope["getFilename"]());
        };

        this.$scope["getHasFile"] = () => {
            return this.$scope.model && this.$scope.model[this.$scope.options["key"]] && this.$scope.model[this.$scope.options["key"]]["fileName"];
        };

        let chooseDocumentFile = (files: File[], callback?: Function) => {
            const dialogSettings = <IDialogSettings>{
                okButton: localization.get("App_Button_Ok", "OK"),
                template: require("../../../../shared/widgets/bp-file-upload-status/bp-file-upload-status.html"),
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
            dialogService.open(dialogSettings, dialogData).then((uploadList: any[]) => {
                if (uploadList && uploadList.length > 0) {
                    let uploadedFile = uploadList[0];
                    const fileExt: RegExpMatchArray = uploadedFile.name.match(/([^.]*)$/);
                    let newFileObject = {
                        fileName: uploadedFile.name,
                        fileExtension: fileExt[0] ? fileExt[0] : "",
                        fileGuid: uploadedFile.guid,
                        filePath: uploadedFile.url
                    };
                    this.$scope.model[this.$scope.options["key"]] = newFileObject;
                    if (onChange) {
                        onChange(newFileObject, $scope.fields[0], $scope);
                    }
                    guid = uploadedFile.guid;
                }
            }).finally(() => {
                if (callback) {
                    callback();
                }
            });
        };

        $scope["onFileSelect"] = (files: File[], callback?: Function) => {
            chooseDocumentFile(files, callback);
        };

        $scope["downloadFile"] = () => {
            if (guid) {
                $window.open(`/svc/bpfilestore/file/${guid}`, "_blank");
            } else {
                const artifactId = templateOptions["artifactId"];
                const versionId = templateOptions["versionId"];
                artifactAttachments.getArtifactAttachments(artifactId, undefined, versionId)
                    .then((attachmentResultSet: IArtifactAttachmentsResultSet) => {
                        if (attachmentResultSet.attachments.length) {
                            const attachmentId = attachmentResultSet.attachments[0].attachmentId;
                            let url = `/svc/bpartifactstore/artifacts/${artifactId}/attachments/${attachmentId}`;
                            if (_.isFinite(versionId)) {
                                url += `?versionId=${versionId}`;
                            }
                            this.$window.open(url, "_blank");
                        } else {
                            messageService.addError(localization.get("App_UP_Attachments_Download_No_Attachment"));
                        }
                    });
            }
        };

        $scope["deleteFile"] = () => {
            if (!templateOptions["disabled"] && !templateOptions["isReadOnly"]) {
                const dialogSettings = <IDialogSettings>{
                    okButton: localization.get("App_Button_Delete", "Delete"),
                    header: localization.get("App_UP_Attachments_Delete_Attachment_Header", "Delete Attachment"),
                    message: localization.get("App_UP_Attachments_Delete_Attachment", "Please confirm the deletion of this attachment.")
                };
                dialogService.open(dialogSettings).then(() => {
                    this.$scope.model[this.$scope.options["key"]] = null;
                    if (onChange) {
                        onChange(null, $scope.fields[0], $scope);
                    }
                    guid = null;
                });
            }
        };

        $scope["changeLabelText"] = localization.get("App_UP_Document_File_Change", "Change");
        $scope["uploadLabelText"] = localization.get("App_UP_Document_File_Upload", "Upload");
        $scope["downloadLabelText"] = localization.get("App_UP_Document_File_Download", "Download");
        $scope["deleteLabelText"] = localization.get("App_UP_Attachments_Delete", "Delete");
    }
}
