import {ILocalizationService, Message} from "../../core";
import {IWindowManager, IMainWindow} from "../../main";
//import { Models, Enums } from "../../main";
import {
    Models, Enums,
    IArtifactManager,
    IStatefulArtifact,
    IMessageService,
    BpBaseEditor
} from "../bp-base-editor";

import {PropertyEditor} from "./bp-property-editor";
import {PropertyContext} from "./bp-property-context";

export {
    ILocalizationService,
    IArtifactManager,
    IStatefulArtifact,
    IMessageService,
    IWindowManager,
    PropertyContext,
    Models,
    Enums,
    Message
}

export class BpArtifactEditor extends BpBaseEditor {

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[] = [];

    public editor: PropertyEditor;

    constructor(public messageService: IMessageService,
                public artifactManager: IArtifactManager,
                public windowManager: IWindowManager,
                public localization: ILocalizationService) {
        super(messageService, artifactManager);
    }

    public $onInit() {
        super.$onInit();
        this.editor = new PropertyEditor(this.localization);
        this.subscribers.push(this.windowManager.mainWindow.subscribeOnNext(this.setArtifactEditorLabelsWidth, this));
    }

    public $onDestroy() {
        super.$onDestroy();

        if (this.editor) {
            this.editor.destroy();
        }
        delete this.editor;
        delete this.fields;
        delete this.model;
    }

    public clearFields() {
        this.fields = [];
    }

    public hasFields(): boolean  {
        return (this.fields || []).length > 0;

    }

    private shouldRenewFields(): boolean {
        return this.artifact.artifactState.readonly || !this.hasFields();
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        this.fields.push(field);
    }

    public onArtifactReady() {
        if (this.editor && this.artifact) {
            this.artifact.metadata.getArtifactPropertyTypes().then((propertyTypes) => {
                const shouldCreateFields = this.editor.create(this.artifact, propertyTypes, this.shouldRenewFields());
                if (shouldCreateFields) {
                    this.clearFields();
                    this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                        //add property change handler to each field
                        Object.assign(field.templateOptions, {
                            onChange: this.onValueChange.bind(this)
                        });

                        const isReadOnly = this.artifact.artifactState.readonly;
                        field.templateOptions["isReadOnly"] = isReadOnly;
                        if (isReadOnly) {
                            if (field.type !== "bpDocumentFile" &&
                                field.type !== "bpFieldImage" &&
                                field.type !== "bpFieldInheritFrom") {
                                field.type = "bpFieldReadOnly";
                            }
                        }
                        this.onFieldUpdate(field);

                    });

                }
                this.model = this.editor.getModel();

                this.setArtifactEditorLabelsWidth();
                super.onArtifactReady();
                this.onFieldUpdateFinished();
            });
        } else {
            this.setArtifactEditorLabelsWidth();
            super.onArtifactReady();
        }
    }

    protected onFieldUpdateFinished() {
        //
    }

    public setArtifactEditorLabelsWidth(mainWindow?: IMainWindow) {
        // MUST match $property-width in styles/partials/_properties.scss plus various padding/margin
        const minimumWidth: number = 392 + ((20 + 1 + 15 + 1 + 10) * 2);

        let pageBodyWrapper = document.querySelector(".page-body-wrapper") as HTMLElement;
        if (pageBodyWrapper) {
            let avaliableWidth: number = mainWindow ? mainWindow.contentWidth : pageBodyWrapper.offsetWidth;

            if (avaliableWidth < minimumWidth) {
                pageBodyWrapper.classList.add("single-column-property");
            } else {
                pageBodyWrapper.classList.remove("single-column-property");
            }
        }
    };


    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: ng.IScope) {
        $scope.$applyAsync(() => {
            try {
                //here we need to update original model
                let context = $field.data as PropertyContext;
                if (!context) {
                    return;
                }
                if (!this.editor) {
                    return;
                }
                let value = this.editor.convertToModelValue($field, $value);
                switch (context.lookup) {
                    case Enums.PropertyLookupEnum.Custom:
                        this.artifact.customProperties.set(context.modelPropertyName as number, value);
                        break;
                    case Enums.PropertyLookupEnum.Special:
                        this.artifact.specialProperties.set(context.modelPropertyName as number, value);
                        break;
                    default:
                        this.artifact[context.modelPropertyName] = value;
                        break;
                }

                if ($scope["form"]) {
                    this.artifact.artifactState.invalid = $scope["form"].$$parentForm.$invalid;
                }

            } catch (err) {
                this.messageService.addError(err);
            }
        });
    };


}


