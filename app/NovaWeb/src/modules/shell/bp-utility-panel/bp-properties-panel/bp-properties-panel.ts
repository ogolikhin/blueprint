import {ILocalizationService } from "../../../core";
import { Models, IWindowManager } from "../../../main";
import { ISelectionManager, IStatefulArtifact, IStatefulSubArtifact } from "../../../managers/artifact-manager";
import {IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import {BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import {IMessageService} from "../../../core";
import {PropertyEditor} from "../../../editors/bp-artifact/bp-property-editor";
import {PropertyContext} from "../../../editors/bp-artifact/bp-property-context";
import {PropertyLookupEnum, LockedByEnum} from "../../../main/models/enums";
import { Helper } from "../../../shared/utils/helper";

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
        "windowManager",
        "localization",
    ];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: PropertyEditor;   

    public isLoading: boolean = false;

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public specificFields: AngularFormly.IFieldConfigurationObject[];
    public richTextFields: AngularFormly.IFieldConfigurationObject[];

    private selectedArtifact: Models.IArtifact;
    private selectedSubArtifact: Models.ISubArtifact;

    constructor(
        $q: ng.IQService,
        protected selectionManager: ISelectionManager,        
        public messageService: IMessageService,
        public windowManager: IWindowManager,
        public localization: ILocalizationService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, selectionManager, bpAccordionPanel);
        this.editor = new PropertyEditor(this.localization);
    }    

    public $onDestroy() {
        delete this.systemFields;
        delete this.specificFields;
        delete this.customFields;
        delete this.richTextFields;
        super.$onDestroy();        
    }

    public get isSystemPropertyAvailable(): boolean {
        return this.systemFields && this.systemFields.length > 0;
    }

    public get isSpecificPropertyAvailable(): boolean {
        return this.specificFields && this.specificFields.length > 0;
    }

    public get isCustomPropertyAvailable(): boolean {
        return this.customFields && this.customFields.length > 0;
    }

    public get isRichTextPropertyAvailable(): boolean {
        return this.richTextFields && this.richTextFields.length > 0;
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        try {
            this.fields = [];
            this.model = {};
            this.systemFields = [];
            this.customFields = [];
            this.specificFields = [];
            this.richTextFields = [];         
            if (artifact) {
                return this.onLoad(artifact, subArtifact, timeout);
            }
        } catch (ex) {
            this.messageService.addError(ex);
            throw ex;
        }
        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    // private addSubArtifactChangeset(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact) {
    //     artifact.subArtifacts = [subArtifact];        
    // }

    private onLoad(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<void> {
        let deferred = this.$q.defer<any>();
        this.isLoading = true;
        if (subArtifact) {
            subArtifact.load().then(() => {
                this.onUpdate(artifact, subArtifact);
            })
            .finally(() => {
                deferred.resolve();
                this.isLoading = false;
            });
                    
        } else {
            artifact.load().then(() => {
                this.onUpdate(artifact, subArtifact);
            })
            .finally(() => {
                deferred.resolve();
                this.isLoading = false;
            });
        }
        return deferred.promise;

    //    if (subArtifact) {
        
            
    //         return this.artifactService.getSubArtifact(artifact.id, subArtifact.id, timeout).then((it: Models.ISubArtifact) => {
    //             angular.extend(subArtifact, it);
    //             this.addSubArtifactChangeset(artifact, subArtifact, undefined);
    //             this.selectedArtifact = artifact;
    //             this.selectedSubArtifact = subArtifact;
    //             this.onUpdate(artifact, subArtifact);
    //         }).catch((error: any) => {
    //             if (error) {
    //                 this.messageService.addError(error["message"] || "SubArtifact_NotFound");
    //             }
    //         }).finally(() => {
    //             this.isLoading = false;
    //         });
    //    } else {
    //         return this.artifactService.getArtifact(artifact.id, timeout).then((it: Models.IArtifact) => {
    //             angular.extend(artifact, it);
    //             this.stateManager.addChange(artifact);                
    //             this.selectedArtifact = artifact;
    //             this.selectedSubArtifact = undefined;
    //             this.onUpdate(artifact, subArtifact);
    //         }).catch((error: any) => {
    //             if (error) {
    //                 this.messageService.addError(error["message"] || "Artifact_NotFound");
    //             }
    //         }).finally(() => {
    //             this.isLoading = false;
    //         });   
    //     }        
     }   

    // private getChangedArtifact(item: Models.IArtifact): Models.IArtifact {        
    //     if (!item) {
    //         return undefined;
    //     }
    //     let changedItem: Models.IArtifact;
    //     this.itemState = this.stateManager.getState(item.id);

    //     if (this.itemState) {
    //         changedItem = this.itemState.getArtifact();
    //     } else {
    //         changedItem = item;
    //     }
    //     return changedItem;
    // }

    // private getChangedSubArtifact(item: Models.ISubArtifact): Models.ISubArtifact {
    //     if (!item) {
    //         return undefined;
    //     }
    //     let changedItem: Models.ISubArtifact;
    //     this.itemState = this.stateManager.getState(item.id);

    //     if (this.itemState) {
    //         if (this.itemState.changedItem) {                
    //             changedItem = this.getSubArtifactById(this.itemState.changedItem, item.id);
    //         } else {                               
    //             changedItem = this.getSubArtifactById(this.itemState.originItem, item.id);
    //         }
    //     } else {
    //         changedItem = item;
    //     }
    //     return changedItem;
    // }

    // private getSubArtifactById(artifact: Models.IArtifact, subArtifactId: number): Models.ISubArtifact {
    //     for (var i = 0; i < artifact.subArtifacts.length; i++) {
    //         let subArtifact = artifact.subArtifacts[i];
    //         if (subArtifact.id === subArtifactId) {
    //             return subArtifact;
    //         }
    //     }
    //     throw new Error("SubArtifact_Not_Found");
    // }

    public onUpdate(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact) {
        this.selectedArtifact = artifact;
        this.selectedSubArtifact = subArtifact;
        try {
            
            if (!artifact || !this.editor) {
                return;
            }

            if (subArtifact) {
                this.editor.load(subArtifact, subArtifact.metadata.getSubArtifactPropertyTypes());
            } else {
                this.editor.load(artifact, artifact.metadata.getArtifactPropertyTypes());
            }

            // let changedArtifact = this.getChangedArtifact(artifact);           
            // let changedSubArtifact = this.getChangedSubArtifact(subArtifact);            

            
            this.model = this.editor.getModel();
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                angular.extend(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                Helper.updateFieldReadOnlyState(field, this.itemState);              

                this.onFieldUpdate(field);                                

            });
        } catch (ex) {
            this.messageService.addError(ex);
            throw ex;
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
            this.specificFields.push(field);
        }

    }

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: AngularFormly.ITemplateScope) {
        //here we need to update original model
        let context = $field.data as PropertyContext;
        if (!context) {
            return;
        }
        //let value = this.editor.convertToModelValue($field, $value);
        // let changeSet: IPropertyChangeSet = {
        //     lookup: context.lookup,
        //     id: context.modelPropertyName,
        //     value: value
        // };
        
        // if (this.selectedSubArtifact) {
        //     this.addSubArtifactChangeset(this.selectedArtifact, this.selectedSubArtifact, changeSet);
        // } else {
        //     this.stateManager.addChange(this.selectedArtifact, changeSet);
        // }
    };

    public get specificPropertiesHeading(): string {
        if (this.selectedArtifact.predefinedType === Models.ItemTypePredefined.Document) {
            return this.localization.get("Nova_Document_File", "File");
        } else if (this.selectedArtifact.predefinedType === Models.ItemTypePredefined.Actor) {
            return this.localization.get("Property_Actor_Section_Name", "Actor Properties");
        } else {
            return this.selectedArtifact.predefinedType + this.localization.get("Nova_Properties", " Properties");
        }
    }
}

