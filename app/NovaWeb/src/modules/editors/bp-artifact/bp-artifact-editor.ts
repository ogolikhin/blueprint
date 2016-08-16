import { ILocalizationService, IMessageService, IStateManager, ItemState, IPropertyChangeSet } from "../../core";
import { IProjectManager, IAvailableContentArea, IWindowManager, Enums, Models} from "../../main";

import { BpBaseEditor} from "../bp-base-editor";
import { PropertyEditor} from "./bp-property-editor";
import { PropertyContext} from "./bp-property-context";

export { ILocalizationService, IProjectManager, IMessageService, IStateManager, IWindowManager, PropertyContext, Models, Enums }

export class BpArtifactEditor extends BpBaseEditor {
    public static $inject: [string] = ["messageService", "stateManager", "windowManager", "localization", "projectManager"];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: PropertyEditor;
    public context: Models.IEditorContext;
    public artifactState: ItemState;

    public isLoading: boolean = true;

    constructor(
        public messageService: IMessageService,
        public stateManager: IStateManager,
        public windowManager: IWindowManager,
        public localization: ILocalizationService,
        private projectManager: IProjectManager
    ) {
        super(messageService, stateManager, windowManager);
        this.editor = new PropertyEditor(this.localization.current);
    }

    public $onInit() {
        super.$onInit();
        this._subscribers.push(this.windowManager.getAvailableArea.subscribeOnNext(this.setArtifactEditorLabelsWidth, this));
    }

    public $onChanges(obj: any) {
        try {
            this.fields = [];
            this.model = {};
            super.$onChanges(obj);
        } catch (ex) {
            this.messageService.addError(ex.message);
        }
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

    public onLoading(obj: any): boolean  {
        this.fields = [];
        return super.onLoading(obj);
    }


    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        this.fields.push(field);
    }

    public onUpdate(context: Models.IEditorContext) {
        try {
            super.onUpdate(context);

            if (!context || !this.editor) {
                return;
            }
            let artifact: Models.IArtifact;
            this.artifactState = this.stateManager.getState(context.artifact.id);

            if (this.artifactState) {
                artifact = this.artifactState.changedItem || this.artifactState.originItem;
            } else {
                artifact = this.context.artifact;
            }
            

            let fieldContexts = this.projectManager.getArtifactPropertyTypes(artifact).map((it: Models.IPropertyType) => {
                return new PropertyContext(it);
            });

            this.editor.load(artifact, fieldContexts);
            this.model = this.editor.getModel();
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                angular.extend(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                this.onFieldUpdate(field);

                if (this.artifactState && this.artifactState.isReadOnly) {
                    field.type = "bpFieldReadOnly";
                }
                if (this.artifactState && this.artifactState.isLocked) {
                    field.type = "bpFieldReadOnly";
                }



            });
        } catch (ex) {
            this.messageService.addError(ex);
        }

        this.setArtifactEditorLabelsWidth();
    }

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: AngularFormly.ITemplateScope) {
        //here we need to update original model
        let context = $field.data as PropertyContext;
        if (!context) {
            return;
        }
        if (($scope.fc as angular.IFormController).$valid) {
            let value = this.editor.convertToModelValue($field, $value);
            let changeSet: IPropertyChangeSet = {
                lookup: context.lookup,
                id: context.modelPropertyName,
                value: value
            };
            this.stateManager.addChange(this.context.artifact, changeSet);
        }
    };

    public setArtifactEditorLabelsWidth(contentArea?: IAvailableContentArea) {
        // MUST match $property-width in styles/partials/_properties.scss plus various padding/margin
        const minimumWidth: number = 392 + ((20 + 1 + 15 + 1 + 10) * 2);

        let pageBodyWrapper = document.querySelector(".page-body-wrapper") as HTMLElement;
        if (pageBodyWrapper) {
            let avaliableWidth: number = contentArea ? contentArea.width : pageBodyWrapper.offsetWidth;

            if (avaliableWidth < minimumWidth) {
                pageBodyWrapper.classList.add("single-column-property");
            } else {
                pageBodyWrapper.classList.remove("single-column-property");
            }
        }
    };
}


