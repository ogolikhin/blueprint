import {ArtifactTypeEnum} from "../../../../main/models/models";
import {FontNormalizer} from "./impl/utils/font-normalizer";
import {IDiagram} from "./impl/models";
import {IUseCase} from "./impl/usecase/models";
import {UsecaseToDiagram} from "./impl/usecase/usecase-to-diagram";

export interface IDiagramService {
    getDiagram(id: number, itemType: ArtifactTypeEnum): ng.IPromise<IDiagram>;
    isDiagram(itemType: ArtifactTypeEnum): boolean;
}

export class DiagramService implements IDiagramService {

    private promises: { [id: string]: ng.IPromise<any> } = {};

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
    }

    public getDiagram(id: number, itemType: ArtifactTypeEnum): ng.IPromise<IDiagram> {
        let promise: ng.IPromise<IDiagram> = this.promises[String(id)];
        if (!promise) {
            const deferred: ng.IDeferred<IDiagram> = this.$q.defer<IDiagram>();
            const path = this.getPath(id, itemType);
            let diagaram: IDiagram = null;
            this.$http.get<IDiagram | IUseCase>(path)
                .success(result => {
                    if (itemType === 4105) {
                        diagaram = new UsecaseToDiagram().convert(<IUseCase>result);
                    } else {
                        diagaram = (<IDiagram>result);
                        if (diagaram.shapes) {
                            for (let i = 0; i < diagaram.shapes.length; i++) {
                                const shape = diagaram.shapes[i];
                                shape.label = shape.label && FontNormalizer.normalize(shape.label);
                                const fontFamily = shape.labelStyle && shape.labelStyle.fontFamily;
                                if (fontFamily) {
                                    const newFontFamily = FontNormalizer.subsitution[fontFamily];
                                    if (newFontFamily) {
                                        shape.labelStyle.fontFamily = newFontFamily;
                                    }
                                }
                            }
                        }
                    }
                    delete this.promises[id];
                    deferred.resolve(diagaram);
                }).error((data: any, status: number) => {
                    delete this.promises[id];
                    data.statusCode = status;
                    deferred.reject(data);
                });

            promise = this.promises[String(id)] = deferred.promise;
        }
        return promise;
    }

    public isDiagram(itemType: ArtifactTypeEnum) {
        switch (itemType) {
            case 4108:
            case 4105:
            case ArtifactTypeEnum.GenericDiagram:
            case ArtifactTypeEnum.UseCaseDiagram:
            case ArtifactTypeEnum.Storyboard:
            case ArtifactTypeEnum.UseCase:
                return true;
            default:
                return false;
        }
    }

    private getPath(id: number, itemType: ArtifactTypeEnum): string {
        if (itemType === 4105) {
            return `/svc/components/RapidReview/usecase/${id}`;
        }
        return `/svc/components/RapidReview/diagram/${id}`;
    }
}