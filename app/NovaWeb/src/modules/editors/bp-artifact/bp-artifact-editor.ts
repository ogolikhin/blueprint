import { ILocalizationService, Message, IPropertyChangeSet } from "../../core";
import { IWindowManager, IMainWindow } from "../../main";
//import { Models, Enums } from "../../main";
import { 
    Models, Enums, 
    IArtifactManager, 
    IStatefulArtifact, 
    IMessageService,
    BpBaseEditor 
} from "../bp-base-editor";

import { PropertyEditor} from "./bp-property-editor";
import { PropertyContext} from "./bp-property-context";

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
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: PropertyEditor;

    constructor(
        public messageService: IMessageService,
        public artifactManager: IArtifactManager,
        public windowManager: IWindowManager,
        public localization: ILocalizationService
    ) {
        super(messageService, artifactManager);
        this.editor = new PropertyEditor(this.localization);
    }

    public $onInit() {
        super.$onInit();
        this.subscribers.push(this.windowManager.mainWindow.subscribeOnNext(this.setArtifactEditorLabelsWidth, this));

        // this.subscribers.push(
        //     this.stateManager.stateChange
        //         .filter(it => this.context && this.context.artifact.id === it.originItem.id && !!it.lock)
        //         .distinctUntilChanged().subscribeOnNext(this.onLockChanged, this)
        // );
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

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        if (!angular.isArray(this.fields)) { }
        this.fields.push(field);
    }

    public onLoad() {
        this.model = {};
        super.onLoad();
    }

    public onUpdate() {
        super.onUpdate();
        if ( !this.editor) {
            return;
        }
        this.clearFields();

        // let artifact: Models.IArtifact;
        // this.artifactState = this.stateManager.getState(context.artifact.id);

        // if (this.artifactState) {
        //     artifact = this.artifactState.getArtifact();
        // } else {
        //     throw Error("Artifact_Not_Found");
        // }
        this.editor.propertyContexts = this.artifact.metadata.getArtifactPropertyTypes().map((it: Models.IPropertyType) => {
            return new PropertyContext(it);
        });


        this.model = this.editor.load(this.artifact, undefined);

        this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
            //add property change handler to each field
            angular.extend(field.templateOptions, {
                onChange: this.onValueChange.bind(this)
            });

            field.templateOptions["isReadOnly"] = this.artifact.artifactState.readonly || this.artifact.artifactState.lockedBy === Enums.LockedByEnum.OtherUser;
            if (this.artifact.artifactState.readonly || this.artifact.artifactState.lockedBy === Enums.LockedByEnum.OtherUser) {
                if (field.key !== "documentFile"  &&
                    field.type !== "bpFieldImage" &&
                    field.type !== "bpFieldInheritFrom") {  
                    field.type = "bpFieldReadOnly";                     
                }
            }
            this.onFieldUpdate(field);

        });

        this.setArtifactEditorLabelsWidth();
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
                let value = this.editor.convertToModelValue($field, $value);
                let changeSet: IPropertyChangeSet = {
                    lookup: context.lookup,
                    id: context.modelPropertyName,
                    value: value
                };
//                this.artifact.setValue()
                // let state = this.stateManager.addChange(this.context.artifact, changeSet);

                // if ($scope["form"]) {
                //     state.setValidationErrorsFlag($scope["form"].$$parentForm.$invalid);
                // }

                // this.stateManager.lockArtifact(state).catch((error: any) => {
                //     if (error) {
                //         this.messageService.addError(error);
                //     }
                // });
            } catch (err) {
                this.messageService.addError(err);
            }
        });
    };

}


