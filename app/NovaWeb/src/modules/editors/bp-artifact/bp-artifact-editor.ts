import { ILocalizationService, IMessageService, Message, IStateManager, ItemState, IPropertyChangeSet } from "../../core";
import { IProjectManager, IWindowManager} from "../../main/services";
import { Enums, Models} from "../../main/models";

import { BpBaseEditor} from "../bp-base-editor";
import { PropertyEditor} from "./bp-property-editor";
import { PropertyContext} from "./bp-property-context";

export { ILocalizationService, IProjectManager, IMessageService, IStateManager, IWindowManager, PropertyContext, Models, Enums, ItemState, Message }

export class BpArtifactEditor extends BpBaseEditor {
    public static $inject: [string] = ["messageService", "stateManager", "windowManager", "localization", "projectManager"];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: PropertyEditor;
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
        this.editor = new PropertyEditor(this.localization);
    }

    public $onInit() {
        super.$onInit();

        this._subscribers.push(
            this.stateManager.stateChange.filter(it => !!it.lock).distinctUntilChanged().subscribeOnNext(this.onLockChanged, this)
        );
    }


    public $onChanges(obj: any) {
        try {
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
        return super.onLoading(obj);
    }

    public onLoad(context: Models.IEditorContext) {
         this.onUpdate(context);
    }

    public clearFields() { 
        this.fields = []; 
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        if (!angular.isArray(this.fields)) { }
        this.fields.push(field);
    }


    public onUpdate(context: Models.IEditorContext) {
        try {
            super.onUpdate(context);
            if (!context || !this.editor) {
                return;
            }
            this.clearFields();

            let artifact: Models.IArtifact;
            this.artifactState = this.stateManager.getState(context.artifact.id);

            if (this.artifactState) {
                artifact = this.artifactState.getArtifact();
            } else {
                throw Error("Artifact_Not_Found");
            }
            this.editor.propertyContexts = this.projectManager.getArtifactPropertyTypes(this.context.artifact, undefined).map((it: Models.IPropertyType) => {
                return new PropertyContext(it);
            });


            this.model = this.editor.load(artifact, undefined);

            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                angular.extend(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                if (this.artifactState.isReadonly || this.artifactState.lockedBy === Enums.LockedByEnum.OtherUser) {
                    field.type = "bpFieldReadOnly";
                }

                this.onFieldUpdate(field);

            });
        } catch (ex) {
            this.messageService.addError(ex);
        }

        this.setArtifactEditorLabelsWidth();
    }

    private onLockChanged(state: ItemState) {
        let lock = state.lock;
        if (lock.result === Enums.LockResultEnum.Success) {
            if (lock.info.versionId !== state.originItem.version) {
                this.onLoad(this.context);
            }
        } else if (lock.result === Enums.LockResultEnum.AlreadyLocked) {
            this.onUpdate(this.context);
            this.messageService.addMessage(new Message(3, "Artifact_Lock_" + Enums.LockResultEnum[lock.result]));
        } else if (lock.result === Enums.LockResultEnum.DoesNotExist) {
            this.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
        } else {
            this.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
        }

    }



    public doSave(state: ItemState): void { }

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: AngularFormly.ITemplateScope) {
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
            let state = this.stateManager.addChange(this.context.artifact, changeSet);

            this.stateManager.lockArtifact(state).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error);
                }
            });

        } catch (err) {
            this.messageService.addError(err);
        }
    };

}


