import {ItemTypePredefined} from "../../../models/enums";
import {FontNormalizer} from "./impl/utils/font-normalizer";
import {IDiagram} from "./impl/models";
import {IUseCase} from "./impl/usecase/models";
import {UsecaseToDiagram} from "./impl/usecase/usecase-to-diagram";

export interface IDiagramService {
    getDiagram(id: number, itemType: ItemTypePredefined): ng.IPromise<IDiagram>;
    isDiagram(itemType: ItemTypePredefined): boolean;
}

export class DiagramService implements IDiagramService {

    private promises: { [id: string]: ng.IPromise<any> } = {};

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
    }

    public getDiagram(id: number, itemType: ItemTypePredefined): ng.IPromise<IDiagram> {
        let promise: ng.IPromise<IDiagram> = this.promises[String(id)];
        if (!promise) {
            const deferred: ng.IDeferred<IDiagram> = this.$q.defer<IDiagram>();
            const path = this.getPath(id, itemType);
            let diagaram: IDiagram = null;
            this.$http.get<IDiagram | IUseCase>(path)
                .success(result => {
                    if (itemType === ItemTypePredefined.UseCase) {
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

    public isDiagram(itemType: ItemTypePredefined) {
        switch (itemType) {
            case ItemTypePredefined.GenericDiagram:
            case ItemTypePredefined.BusinessProcess:
            case ItemTypePredefined.DomainDiagram:
            case ItemTypePredefined.Storyboard:
            case ItemTypePredefined.UseCaseDiagram:
            case ItemTypePredefined.UseCase:
            case ItemTypePredefined.UIMockup:
                return true;
            default:
                return false;
        }
    }

    private getPath(id: number, itemType: ItemTypePredefined): string {
        if (itemType === ItemTypePredefined.UseCase) {
            return `/svc/components/RapidReview/usecase/${id}`;
        }
        return `/svc/components/RapidReview/diagram/${id}?addDraft=true`;
    }
}