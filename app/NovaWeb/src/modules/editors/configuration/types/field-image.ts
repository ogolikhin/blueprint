﻿import "angular";
import { IArtifactAttachmentsService, IArtifactAttachmentsResultSet } from "../../../managers/artifact-manager";
import { Helper } from "../../../shared/utils/helper";
import { ILocalizationService, IMessageService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { BPFieldBaseController } from "./base-controller";


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
    static $inject: [string] = ["$scope", "localization", "$window", "messageService", "dialogService", "settingsService"];

    constructor(
        private $scope: AngularFormly.ITemplateScope,
        private localization: ILocalizationService,
        private $window: ng.IWindowService,
        private messageService: IMessageService,
        private dialogService: IDialogService,
        private settingsService: ISettingsService
    ) {
        super();

        $scope["changeLabelText"] = localization.get("App_UP_Document_File_Change", "Change");

        setFields(currentModelVal);
    }
}