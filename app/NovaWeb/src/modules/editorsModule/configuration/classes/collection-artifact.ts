import {Models} from "../../../main/models";
import {PropertyTypePredefined} from "../../../main/models/enums";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {IArtifact} from "../../../main/models/models";
import {IStatefulArtifact, StatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";

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
    private collectionContentPropertyValue: Models.IPropertyValue;

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

    protected initialize(artifact: Models.IArtifact): void {
        const state = super.initialize(artifact);
        this.collectionContentPropertyValue = this.createCollectionContentSpecificProperty();
        this.specialProperties.list().push(this.collectionContentPropertyValue);
        return state;
    }

    public supportRelationships(): boolean {
        return false;
    }

    public addArtifactsToCollection(artifacts: IArtifact[]) {

        if (this.artifact &&
            artifacts &&
            artifacts.length > 0 &&
            this.collectionContentPropertyValue) {

            const collectionContentPV = this.collectionContentPropertyValue.value as Models.ICollectionContentPropertyValue;

            artifacts.forEach((artifact: IArtifact) => {
                const newArtifact = <ICollectionArtifact>{
                    id: artifact.id,
                    description: "",
                    itemTypeId: artifact.itemTypeId,
                    itemTypePredefined: artifact.predefinedType,
                    name: artifact.name,
                    prefix: artifact.prefix,
                    artifactPath: artifact.artifactPath
                };
                this.artifacts.push(newArtifact);

                const index = collectionContentPV.removedArtifacts.indexOf(artifact.id);
                if (index > -1) {
                    collectionContentPV.removedArtifacts.splice(index, 1);
                } else {
                    collectionContentPV.addedArtifacts.push(artifact.id);
                }
            });

            this.updateCollectionContentSpecialProperty(collectionContentPV);
        }
    }

    private updateCollectionContentSpecialProperty(collectionContentPropertyValue: Models.ICollectionContentPropertyValue): void {
        const newPropertyValue: Models.ICollectionContentPropertyValue = {
            addedArtifacts: collectionContentPropertyValue.addedArtifacts,
            removedArtifacts: collectionContentPropertyValue.removedArtifacts
        };

        this.specialProperties.set(Models.PropertyTypePredefined.CollectionContent, newPropertyValue);
    }

    public removeArtifacts(artifacts: IArtifact[]) {
        if (this.artifact &&
            artifacts &&
            artifacts.length > 0 &&
            this.collectionContentPropertyValue) {

            const collectionContentPV = this.collectionContentPropertyValue.value as Models.ICollectionContentPropertyValue;
            let isSomethingDeleted: boolean = false;
            artifacts.forEach((artifact: IArtifact) => {

                let index = this.artifacts.indexOf(<ICollectionArtifact>artifact, 0);
                if (index > -1) {
                    isSomethingDeleted = true;
                    this.artifacts.splice(index, 1);

                    const addedArtifactsIndex = collectionContentPV.addedArtifacts.indexOf(artifact.id);
                    if (addedArtifactsIndex > -1) {
                        collectionContentPV.addedArtifacts.splice(addedArtifactsIndex, 1);
                    } else {
                        collectionContentPV.removedArtifacts.push(artifact.id);
                    }
                }
            });

            if (isSomethingDeleted) {
                this.updateCollectionContentSpecialProperty(collectionContentPV);
            }
        }
    }
}
