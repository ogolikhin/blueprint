import {IStatefulArtifact, StatefulArtifact} from "../artifact";

export interface IStatefulCollectionArtifact extends IStatefulArtifact {
    rapidReviewCreated: boolean;
}

export class StatefulCollectionArtifact extends StatefulArtifact implements IStatefulCollectionArtifact {
    rapidReviewCreated: boolean = false;
}
