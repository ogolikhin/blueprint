import {IWindowManager, IMainWindow} from "../../main";
import {
    Models, Enums,
    IArtifactManager,
    IStatefulArtifact,
    BpBaseEditor
} from "../bp-base-editor";

import {PropertyEditor} from "./bp-property-editor";
import {IPropertyDescriptor, IPropertyDescriptorBuilder} from "../configuration/property-descriptor-builder";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";

export {
    IArtifactManager,
    IStatefulArtifact,
    IWindowManager,
    Models,
    Enums
}

export abstract class BpArtifactEditor extends BpBaseEditor {

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[] = [];
    public artifactPreviouslyReadonly: boolean = false;
    public editor: PropertyEditor;

    constructor(public messageService: IMessageService,
                public artifactManager: IArtifactManager,
                public windowManager: IWindowManager,
                public localization: ILocalizationService,
                public propertyDescriptorBuilder: IPropertyDescriptorBuilder) {
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

    public hasFields(): boolean {
        return (this.fields || []).length > 0;
    }

    private shouldRenewFields(): boolean {
        //Renew fields only if readonly status has changed
        const readonlyStatusChanged = this.artifact.artifactState.readonly !== this.artifactPreviouslyReadonly;
        this.artifactPreviouslyReadonly = this.artifact.artifactState.readonly;

        return readonlyStatusChanged || !this.hasFields();
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        this.fields.push(field);
    }

    public onArtifactReady() {
        if (this.isDestroyed) {
            return;
        }
        this.propertyDescriptorBuilder.createArtifactPropertyDescriptors(this.artifact).then(propertyContexts => {
            this.displayContent(propertyContexts);
        });
    }

    private displayContent(propertyContexts: IPropertyDescriptor[]) {
        const shouldCreateFields = this.editor.create(this.artifact, propertyContexts, this.shouldRenewFields());
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
        } else {
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                field.data["isFresh"] = true;
            });
        }
        this.model = this.editor.getModel();

        this.setArtifactEditorLabelsWidth();
        super.onArtifactReady();
        this.onFieldUpdateFinished();
    }

    protected onFieldUpdateFinished() {
        //
    }

    public setArtifactEditorLabelsWidth(mainWindow?: IMainWindow) {
        // MUST match $property-width in styles/partials/_properties.scss plus various padding/margin
        // TODO: make more CSS/layout independent
        const minimumWidth: number = 392 + ((20 + 1 + 15 + 1 + 10) * 2) + 20;

        const pageBodyWrapper = document.querySelector(".page-body-wrapper") as HTMLElement;
        if (pageBodyWrapper) {
            const availableWidth: number = mainWindow ? mainWindow.contentWidth : pageBodyWrapper.offsetWidth;

            if (availableWidth < minimumWidth) {
                pageBodyWrapper.classList.add("single-column-property");
            } else {
                pageBodyWrapper.classList.remove("single-column-property");
            }
        }
    };

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: ng.IScope) {
        $scope.$applyAsync(() => {
            try {
                const context = $field.data as IPropertyDescriptor;
                if (!context || !this.editor) {
                    return;
                }
                //here we need to update original model
                const value = this.editor.convertToModelValue($field, $value);
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
                context.isFresh = false;
            } catch (err) {
                this.messageService.addError(err);
            }
        });
    };
}
