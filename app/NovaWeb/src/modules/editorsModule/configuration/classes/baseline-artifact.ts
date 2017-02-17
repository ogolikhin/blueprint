import {Models} from "../../../main/models";
import {PropertyTypePredefined} from "../../../main/models/enums";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IArtifact} from "../../../main/models/models";
import {IStatefulArtifact, StatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";

export interface IBaseline extends IArtifact {
    reviewName: string;
    isCreated: boolean;
    artifacts: IBaselineArtifact[];
}

export interface IBaselineArtifact {
    id: number;
    name: string;
    prefix: string;
    description: string;
    itemTypeId: number;
    artifactPath: string[];
    itemTypePredefined: ItemTypePredefined;
}

export interface IStatefulBaselineArtifact extends IStatefulArtifact {
    rapidReviewCreated: boolean;
    reviewName: string;
    artifacts: IBaselineArtifact[];
    addArtifactsToBaseline(artifactIds: IArtifact[]);
    removeArtifacts(artifactIds: IArtifact[]);
}

export class StatefulBaselineArtifact extends StatefulArtifact implements IStatefulBaselineArtifact {
    private baselineContentPropertyValue: Models.IPropertyValue;

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IArtifact> {
        const url = `/svc/bpartifactstore/baseline/${id}`;
        return this.services.artifactService.getArtifactModel<IBaseline>(url, id, versionId);
    }

    public get rapidReviewCreated() {
        if (this.artifact) {
            return (<IBaseline>this.artifact).isCreated;
        }
        return false;
    }

    public get reviewName() {
        if (this.artifact) {
            return (<IBaseline>this.artifact).reviewName;
        }
        return undefined;
    }

    public get artifacts() {
        if (this.artifact) {
            return (<IBaseline>this.artifact).artifacts;
        }
        return undefined;
    }

    private createBaselineContentSpecificProperty(): Models.IPropertyValue {
        let baselineContent = <Models.IBaselineContentPropertyValue>{
            addedArtifacts: [],
            removedArtifacts: []
        };

        let baselineContentProperty = <Models.IPropertyValue>{
            name: "BaselineContent",
            propertyTypeId: -1,
            propertyTypePredefined: PropertyTypePredefined.BaselineContent,
            value: baselineContent
        };
        return baselineContentProperty;
    }

    protected initialize(artifact: Models.IArtifact): void {
        const state = super.initialize(artifact);
        this.baselineContentPropertyValue = this.createBaselineContentSpecificProperty();
        this.specialProperties.list().push(this.baselineContentPropertyValue);
        return state;
    }

    public supportRelationships(): boolean {
        return false;
    }

    public addArtifactsToBaseline(artifacts: IArtifact[]) {

        if (this.artifact &&
            artifacts &&
            artifacts.length > 0 &&
            this.baselineContentPropertyValue) {

            const baselineContentPV = this.baselineContentPropertyValue.value as Models.IBaselineContentPropertyValue;

            artifacts.forEach((artifact: IArtifact) => {
                const newArtifact = <IBaselineArtifact>{
                    id: artifact.id,
                    description: "",
                    itemTypeId: artifact.itemTypeId,
                    itemTypePredefined: artifact.predefinedType,
                    name: artifact.name,
                    prefix: artifact.prefix,
                    artifactPath: artifact.artifactPath
                };
                this.artifacts.push(newArtifact);

                const index = baselineContentPV.removedArtifacts.indexOf(artifact.id);
                if (index > -1) {
                    baselineContentPV.removedArtifacts.splice(index, 1);
                } else {
                    baselineContentPV.addedArtifacts.push(artifact.id);
                }
            });

            this.updateBaselineContentSpecialProperty(baselineContentPV);
        }
    }

    private updateBaselineContentSpecialProperty(baselineContentPropertyValue: Models.IBaselineContentPropertyValue): void {
        const newPropertyValue: Models.IBaselineContentPropertyValue = {
            addedArtifacts: baselineContentPropertyValue.addedArtifacts,
            removedArtifacts: baselineContentPropertyValue.removedArtifacts
        };

        this.specialProperties.set(Models.PropertyTypePredefined.BaselineContent, newPropertyValue);
    }

    public removeArtifacts(artifacts: IArtifact[]) {
        if (this.artifact &&
            artifacts &&
            artifacts.length > 0 &&
            this.baselineContentPropertyValue) {

            const baselineContentPV = this.baselineContentPropertyValue.value as Models.IBaselineContentPropertyValue;
            let isSomethingDeleted: boolean = false;
            artifacts.forEach((artifact: IArtifact) => {

                let index = this.artifacts.indexOf(<IBaselineArtifact>artifact, 0);
                if (index > -1) {
                    isSomethingDeleted = true;
                    this.artifacts.splice(index, 1);

                    const addedArtifactsIndex = baselineContentPV.addedArtifacts.indexOf(artifact.id);
                    if (addedArtifactsIndex > -1) {
                        baselineContentPV.addedArtifacts.splice(addedArtifactsIndex, 1);
                    } else {
                        baselineContentPV.removedArtifacts.push(artifact.id);
                    }
                }
            });

            if (isSomethingDeleted) {
                this.updateBaselineContentSpecialProperty(baselineContentPV);
            }
        }
    }
}
