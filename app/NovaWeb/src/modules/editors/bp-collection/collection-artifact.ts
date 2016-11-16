import { IStatefulArtifact, StatefulArtifact } from "../../managers/artifact-manager/artifact";
import {IArtifact} from "../../main/models/models";
import {ItemTypePredefined, PropertyTypePredefined} from "../../main/models/enums";
import {ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet} from "../../managers/artifact-manager/changeset";
import {Helper} from "../../shared/utils/helper";
import {Models} from "../../main/models";
import {IState} from "../../managers/artifact-manager/state";

export interface ICollection extends IArtifact {
    reviewName: string;
    isCreated: boolean;        
    artifacts: ICollectionArtifact[];    
}

export interface ICollectionArtifact {
    id: number;
    name: string;
    prefix: string;
    description: string;
    itemTypeId: number;    
    artifactPath: string[];
    itemTypePredefined: ItemTypePredefined;   
}

export interface IStatefulCollectionArtifact extends IStatefulArtifact {
    rapidReviewCreated: boolean;
    reviewName: string;      
    artifacts: ICollectionArtifact[];
    addArtifactsToCollection(artifactIds: IArtifact[]);
    removeArtifacts(artifactIds: IArtifact[]);    
}

export class StatefulCollectionArtifact extends StatefulArtifact implements IStatefulCollectionArtifact {
    
    private _collectionContentPropertyValue: Models.IPropertyValue;    

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IArtifact> {
        const url = `/svc/bpartifactstore/collection/${id}`;
        return this.services.artifactService.getArtifactModel<ICollection>(url, id, versionId);
    }    

    public get rapidReviewCreated() {
        if (this.artifact) {
            return (<ICollection>this.artifact).isCreated;
        }
        return false;
    }

    public get reviewName() {
        if (this.artifact) {
            return (<ICollection>this.artifact).reviewName;
        }
        return undefined;
    }

    public get artifacts() {
        if (this.artifact) {
            return (<ICollection>this.artifact).artifacts;
        }
        return undefined;
    }

    private createCollectionContentSpecificProperty(): Models.IPropertyValue {
        let collectionContent = <Models.ICollectionContentPropertyValue>{
            addedArtifacts: [],
            removedArtifacts: []
        };

        let collectionContentProperty = <Models.IPropertyValue>{
            name: "CollectionContent",           
            propertyTypeId: -1,
            propertyTypePredefined: PropertyTypePredefined.CollectionContent,            
            value: collectionContent
        };
        return collectionContentProperty;
    }    

    protected initialize(artifact: Models.IArtifact): IState {
        const state = super.initialize(artifact);      
        this._collectionContentPropertyValue = this.createCollectionContentSpecificProperty();        
        this.specialProperties.list().push(this._collectionContentPropertyValue);
        return state;
    }   

    public addArtifactsToCollection(artifacts: IArtifact[]) {

        if (this.artifact &&
            artifacts &&
            artifacts.length > 0 &&
            this._collectionContentPropertyValue) {

            const collectionContentPV = this._collectionContentPropertyValue.value as Models.ICollectionContentPropertyValue;
            const removedArtifactsClone = collectionContentPV.removedArtifacts.slice();
            const addedArtifactsClone = collectionContentPV.addedArtifacts.slice();
            
            artifacts.map((artifact: IArtifact) => {
                const newArtifact = <ICollectionArtifact>{
                    id: artifact.id,
                    description: "",
                    itemTypeId: artifact.itemTypeId,
                    itemTypePredefined: artifact.predefinedType,
                    name: artifact.name,
                    prefix: artifact.prefix,
                    artifactPath: Helper.getArtifactPath(artifact)
                };
                this.artifacts.push(newArtifact);
                
                const index = removedArtifactsClone.indexOf(artifact.id);
                if (index > -1) {
                    removedArtifactsClone.splice(index, 1);
                } else {
                    addedArtifactsClone.push(artifact.id);
                }                
            });                        

            this.updateCollectionContentSpecialProperty(addedArtifactsClone, removedArtifactsClone);
        }
    }    

    private updateCollectionContentSpecialProperty(addedArtifacts: number[], removedArtifacts: number[]): void {
        const newPropertyValue: Models.ICollectionContentPropertyValue = {
            addedArtifacts: addedArtifacts,
            removedArtifacts: removedArtifacts
        };

        this.specialProperties.set(Models.PropertyTypePredefined.CollectionContent, newPropertyValue);
    }

    public removeArtifacts(artifacts: IArtifact[]) {

        if (this.artifact &&
            artifacts &&
            artifacts.length > 0 &&
            this._collectionContentPropertyValue) {

            const collectionContentPV = this._collectionContentPropertyValue.value as Models.ICollectionContentPropertyValue;
            const removedArtifactsClone = collectionContentPV.removedArtifacts.slice();
            const addedArtifactsClone = collectionContentPV.addedArtifacts.slice();
            let isSomethingDeleted: boolean = false;
            artifacts.map((artifact: IArtifact) => {
                
                let index = this.artifacts.indexOf(<ICollectionArtifact>artifact, 0);
                if (index > -1) {
                    isSomethingDeleted = true;
                    this.artifacts.splice(index, 1);

                    const addedArtifactsIndex = addedArtifactsClone.indexOf(artifact.id);
                    if (addedArtifactsIndex > -1) {
                        addedArtifactsClone.splice(addedArtifactsIndex, 1);
                    } else {
                        removedArtifactsClone.push(artifact.id);
                    }   
                }                                                                             
            });

            if (isSomethingDeleted) {
                this.updateCollectionContentSpecialProperty(addedArtifactsClone, removedArtifactsClone);
            }
        }
    }
}
