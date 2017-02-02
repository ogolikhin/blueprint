import "angular";
import {BpCollectionHeader} from "./collectionHeader.component";

export const CollectionHeader = angular.module("collectionHeader", [])
    .component("bpCollectionHeader", new BpCollectionHeader()).name;
