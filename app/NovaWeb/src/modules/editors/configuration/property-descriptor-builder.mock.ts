import * as angular from "angular";
import { IStatefulArtifact, IStatefulSubArtifact } from "../../managers/artifact-manager";
import {
    IPropertyDescriptor,
    IPropertyDescriptorBuilder,
    PropertyDescriptorBuilder
} from "./property-descriptor-builder";

export class PropertyDescriptorBuilderMock implements IPropertyDescriptorBuilder {

    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }
    
    public createArtifactPropertyDescriptors(artifact: IStatefulArtifact): ng.IPromise<IPropertyDescriptor[]> {
        const defered = this.$q.defer();
        defered.resolve([]);
        return defered.promise;
    }

    public createSubArtifactPropertyDescriptors(subArtifact: IStatefulSubArtifact): ng.IPromise<IPropertyDescriptor[]> {
        const defered = this.$q.defer();
        defered.resolve([]);
        return defered.promise;
    }
}
