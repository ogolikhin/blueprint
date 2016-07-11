import {FontNormalizer} from "./impl/utils/font-normalizer";

export interface IDiagram {
    id: number;
    diagramType: string;
    width: number;
    height: number;
    shapes: IShape[];
    connections: IConnection[];
    libraryVersion: number;
}

export interface IDiagramElement {
        id: number;
        type: string;
        name: string;
        props: IProp[];
        zIndex: number;
        isShape: boolean;
    }

export interface IHierarchyDiagram extends IDiagram {
    children: Array<IHierarchyElement>;
}

export interface IHierarchyElement extends IShape, IConnection {
    children: Array<IHierarchyElement>;
    parent: IHierarchyElement;
}

export interface IShape extends IDiagramElement {
    id: number;
    name: string;
    parentId: number;
    type: string;
    height: number;
    width: number;
    x: number;
    y: number;
    zIndex: number;
    angle: number;
    stroke: string;
    strokeOpacity: number;
    strokeWidth: number;
    strokeDashPattern: string;
    fill: string;
    gradientFill: string;
    isGradient: boolean;
    fillOpacity: number;
    shadow: boolean;
    label: string;
    labelTextAlignment: string;
    description: string;
    props: IProp[];
    labelStyle: ILabelStyle;
}

export interface IProp {
    name: string;
    value: any;
}

export interface ILabelStyle {
    textAlignment: string;
    fontFamily: string;
    fontSize: string;
    isItalic: boolean;
    isBold: boolean;
    isUnderline: boolean;
    foreground: string;
}

export interface IConnection extends IDiagramElement {
    id: number;
    type: string;
    parentId: number;
    name: string;
    sourceId: number;
    targetId: number;
    stroke: string;
    strokeOpacity: number;
    strokeWidth: number;
    strokeDashPattern: string;
    label: string;
    sourceLabel: string;
    targetLabel: string;
    points: IPoint[];
    startArrow: string;
    endArrow: string;
    zIndex: number;
    props: IProp[];
}

export interface IPoint {
    y: number;
    x: number;
}

export interface IDiagramService {
    getDiagram(id: number): ng.IPromise<IDiagram>;
}

export class DiagramService implements IDiagramService {

    private promises: { [id: string]: ng.IPromise<any> } = {};

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, private fontNormalizer: any) {
    }

    public getDiagram(id: number): ng.IPromise<IDiagram> {
        let promise: ng.IPromise<IDiagram> = this.promises[String(id)];
        if (!promise) {
            var deferred: ng.IDeferred<IDiagram> = this.$q.defer<IDiagram>();
            this.$http.get<IDiagram>("/svc/components/RapidReview/diagram/" + id)
                .success((diagaram: IDiagram) => {
                        if (diagaram.shapes) {
                            for (var i = 0; i < diagaram.shapes.length; i++) {
                                var shape = diagaram.shapes[i];
                                shape.label = shape.label && FontNormalizer.normalize(shape.label);
                                var fontFamily = shape.labelStyle && shape.labelStyle.fontFamily;
                                if (fontFamily) {
                                    var newFontFamily = FontNormalizer.subsitution[fontFamily];
                                    if (newFontFamily) {
                                    shape.labelStyle.fontFamily = newFontFamily;
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
}