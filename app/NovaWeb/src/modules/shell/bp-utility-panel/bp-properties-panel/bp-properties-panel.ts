import {ILocalizationService, IStateManager, ItemState, IPropertyChangeSet } from "../../../core";
import {ISelectionManager, Models, IProjectManager, IWindowManager, IArtifactService} from "../../../main";
import {IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import {BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import {IMessageService} from "../../../core";
import {PropertyEditor} from "../../../editors/bp-artifact/bp-property-editor";
import {PropertyContext} from "../../../editors/bp-artifact/bp-property-context";
import {PropertyLookupEnum} from "../../../main/models/enums";

export class BPPropertiesPanel implements ng.IComponentOptions {
    public template: string = require("./bp-properties-panel.html");
    public controller: Function = BPPropertiesController;   
    public controllerAs = "$ctrl";
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPPropertiesController extends BPBaseUtilityPanelController {

    public static $inject: [string] = [
        "$q",
        "selectionManager",
        "messageService",
        "stateManager",
        "windowManager",
        "localization",
        "projectManager",
        "artifactService"
    ];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: PropertyEditor;   
    public itemState: ItemState;

    public isLoading: boolean = false;

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public richTextFields: AngularFormly.IFieldConfigurationObject[];

    private selectedArtifact: Models.IArtifact;
    private selectedSubArtifact: Models.ISubArtifact;

    constructor(
        $q: ng.IQService,
        protected selectionManager: ISelectionManager,        
        public messageService: IMessageService,
        public stateManager: IStateManager,
        public windowManager: IWindowManager,
        public localization: ILocalizationService,
        private projectManager: IProjectManager,
        private artifactService: IArtifactService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, selectionManager, stateManager, bpAccordionPanel);
        this.editor = new PropertyEditor(this.localization);
    }    

    public $onDestroy() {
        delete this.itemState;
        delete this.systemFields;
        delete this.customFields;
        delete this.richTextFields;
        super.$onDestroy();        
    }

    public get isSystemPropertyAvailable(): boolean {
        return this.systemFields && this.systemFields.length > 0;
    }
    public get isCustomPropertyAvailable(): boolean {
        return this.customFields && this.customFields.length > 0;
    }

    public get isRichTextPropertyAvailable(): boolean {
        return this.richTextFields && this.richTextFields.length > 0;
    }

    protected onSelectionChanged (artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        try {
            this.fields = [];
            this.model = {};
            this.systemFields = [];
            this.customFields = [];
            this.richTextFields = [];         
            if (artifact) {
                return this.onLoad(artifact, subArtifact, timeout);
            }
        } catch (ex) {
            this.messageService.addError(ex.message);
        }
        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private addSubArtifactChangeset(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, changeSet: IPropertyChangeSet) {
        artifact.subArtifacts = [subArtifact];        
        this.stateManager.addChange(artifact, changeSet);
        this.stateManager.addChange(subArtifact, changeSet);
    }

    private onLoad(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, timeout: ng.IPromise<void>): ng.IPromise<void> {
        this.isLoading = true;
        
        if (subArtifact) {
            return this.artifactService.getSubArtifact(artifact.id, subArtifact.id, timeout).then((it: Models.ISubArtifact) => {
                angular.extend(subArtifact, it);
                this.addSubArtifactChangeset(artifact, subArtifact, undefined);
                this.selectedArtifact = artifact;
                this.selectedSubArtifact = subArtifact;
                this.onUpdate(artifact, subArtifact);
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || "SubArtifact_NotFound");
                }
            }).finally(() => {
                this.isLoading = false;
            });
        } else {
            return this.artifactService.getArtifact(artifact.id, timeout).then((it: Models.IArtifact) => {
                angular.extend(artifact, it);
                this.stateManager.addChange(artifact);                
                this.selectedArtifact = artifact;
                this.selectedSubArtifact = undefined;
                this.onUpdate(artifact, subArtifact);
            }).catch((error: any) => {
                if (error) {
                    this.messageService.addError(error["message"] || "Artifact_NotFound");
                }
            }).finally(() => {
                this.isLoading = false;
            });   
        }        
    }   

    private getChangedArtifact(item: Models.IArtifact): Models.IArtifact {        
        if (!item) {
            return undefined;
        }
        let changedItem: Models.IArtifact;
        this.itemState = this.stateManager.getState(item.id);

        if (this.itemState) {
            changedItem = this.itemState.getArtifact();
        } else {
            changedItem = item;
        }
        return changedItem;
    }

    private getChangedSubArtifact(item: Models.ISubArtifact): Models.ISubArtifact {
        if (!item) {
            return undefined;
        }
        let changedItem: Models.ISubArtifact;
        this.itemState = this.stateManager.getState(item.id);

        if (this.itemState) {
            if (this.itemState.changedItem) {                
                changedItem = this.getSubArtifactById(this.itemState.changedItem, item.id);
            } else {                               
                changedItem = this.getSubArtifactById(this.itemState.originItem, item.id);
            }
        } else {
            changedItem = item;
        }
        return changedItem;
    }

    private getSubArtifactById(artifact: Models.IArtifact, subArtifactId: number): Models.ISubArtifact {
        for (var i = 0; i < artifact.subArtifacts.length; i++) {
            let subArtifact = artifact.subArtifacts[i];
            if (subArtifact.id === subArtifactId) {
                return subArtifact;
            }
        }
        throw new Error("SubArtifact_Not_Found");
    }

    public onUpdate(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact) {
        try {
            
            if (!artifact || !this.editor) {
                return;
            }

            let changedArtifact = this.getChangedArtifact(artifact);           
            let changedSubArtifact = this.getChangedSubArtifact(subArtifact);            

            this.editor.propertyContexts = this.projectManager.getArtifactPropertyTypes(changedArtifact, changedSubArtifact).map((it: Models.IPropertyType) => {
                return new PropertyContext(it);
            });

            
            this.model = this.editor.load(changedArtifact, changedSubArtifact);
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                angular.extend(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                this.onFieldUpdate(field);

                // if (this.itemState && this.itemState.isReadOnly) {
                //     field.type = "bpFieldReadOnly";
                // }
                // if (this.itemState && this.itemState.isLocked) {
                //     field.type = "bpFieldReadOnly";
                // }

                field.type = "bpFieldReadOnly";

            });
        } catch (ex) {
            this.messageService.addError(ex);
        }       
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        let propertyContext = field.data as PropertyContext;
        if (!propertyContext) {
            return;
        }

        if (true === propertyContext.isRichText && PropertyLookupEnum.Special === propertyContext.lookup) {
            this.systemFields.push(field);
            return;
        }

        //re-group fields
        if (true === propertyContext.isRichText) {
            this.richTextFields.push(field);
        } else if (PropertyLookupEnum.System === propertyContext.lookup) {
            this.systemFields.push(field);
        } else if (PropertyLookupEnum.Custom === propertyContext.lookup) {
            this.customFields.push(field);
        } else if (PropertyLookupEnum.Special === propertyContext.lookup) {
            this.systemFields.push(field);
        }

    }

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: AngularFormly.ITemplateScope) {
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
        
        if (this.selectedSubArtifact) {
            this.addSubArtifactChangeset(this.selectedArtifact, this.selectedSubArtifact, changeSet);
        } else {
            this.stateManager.addChange(this.selectedArtifact, changeSet);
        }
    };

}

