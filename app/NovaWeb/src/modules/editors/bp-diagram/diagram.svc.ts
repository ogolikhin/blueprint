import {ItemTypePredefined} from "../../main/models/enums";
import {FontNormalizer} from "./impl/utils/font-normalizer";
import {IDiagram} from "./impl/models";
import {IUseCase} from "./impl/usecase/models";
import {UsecaseToDiagram} from "./impl/usecase/usecase-to-diagram";

export interface IDiagramService {
    getDiagram(id: number, itemType: ItemTypePredefined, cancelationToken: ng.IPromise<any>): ng.IPromise<IDiagram>;
    isDiagram(itemType: ItemTypePredefined): boolean;
}

export var CancelationTokenConstant = {
    cancelationToken: "Cancelled"
};

export class DiagramService implements IDiagramService {

    private promises: { [id: string]: ng.IPromise<any> } = {};

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
    }

    public getDiagram(id: number, itemType: ItemTypePredefined, cancelationToken: ng.IPromise<any>): ng.IPromise<IDiagram> {
        let promise: ng.IPromise<IDiagram> = this.promises[String(id)];
        if (!promise) {
            const deferred: ng.IDeferred<IDiagram> = this.$q.defer<IDiagram>();
            cancelationToken.then(() => {
                deferred.reject(CancelationTokenConstant.cancelationToken);
            });
            this.loadDiagram(id, itemType, cancelationToken, deferred);
            promise = this.promises[String(id)] = deferred.promise;
        }
        return promise;
    }

    private loadDiagram(id: number, itemType: ItemTypePredefined, cancelationToken: ng.IPromise<any>, deferred: ng.IDeferred<IDiagram>) {
        const path = this.getPath(id, itemType);
        let diagaram: IDiagram = null;
        this.$http.get<IDiagram | IUseCase>(path, {timeout: cancelationToken})
            .then(result => {
                try {
                    if (itemType === ItemTypePredefined.UseCase) {
                        diagaram = new UsecaseToDiagram().convert(<IUseCase>result.data);
                    } else {
                        diagaram = (<IDiagram>result.data);
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
                    diagaram.data = result.data;
                    delete this.promises[id];
                    deferred.resolve(diagaram);
                } catch (error) {
                    delete this.promises[id];
                    deferred.reject(error);
                }
            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                delete this.promises[id];
                if (!result) {
                    deferred.reject();
                    return;   
                }

                if (result.status <= 0) {
                    deferred.reject(CancelationTokenConstant.cancelationToken);
                } else if (result.status === 1401) {
                    this.loadDiagram(id, itemType, cancelationToken, deferred);
                } else {
                    result.data.statusCode = result.status;
                    deferred.reject(result.data);
                }
            });
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
            return `/svc/bpartifactstore/usecase/${id}`;
        }
        return `/svc/bpartifactstore/diagram/${id}?addDraft=true`;
    }
}