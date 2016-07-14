import {ItemTypePredefined} from "../../../models/enums";
import {FontNormalizer} from "./impl/utils/font-normalizer";
import {IDiagram, IShape, IConnection, IProp, ILabelStyle, IPoint, IHierarchyElement} from "./impl/models";
import {IUseCase} from "./impl/usecase/models";
import {UsecaseToDiagram} from "./impl/usecase/usecase-to-diagram";
import {IDiagramService} from "./diagram.svc";
import {Shapes, ShapeProps, Diagrams} from "./impl/utils/constants";


export class Diagram implements IDiagram {
    public id: number;
    public diagramType: string;
    public width: number;
    public height: number;
    public shapes: IShape[];
    public connections: IConnection[];
    public libraryVersion: number;

}

export class Shape implements IShape {
    public id: number;
    public name: string;
    public parentId: number;
    public type: string;
    public height: number;
    public width: number;
    public x: number;
    public y: number;
    public zIndex: number;
    public angle: number;
    public stroke: string;
    public strokeOpacity: number;
    public strokeWidth: number;
    public strokeDashPattern: string;
    public fill: string;
    public fillOpacity: number;
    public gradientFill: string;
    public isGradient: boolean;
    public shadow: boolean;
    public label: string;
    public labelTextAlignment: string;
    public description: string;
    public image: string;
    public props: IProp[];
    public labelStyle: ILabelStyle;
    public isShape: boolean;
}

export class LabelStyle implements ILabelStyle {
    public textAlignment: string;
    public fontFamily: string;
    public fontSize: string;
    public isItalic: boolean;
    public isBold: boolean;
    public isUnderline: boolean;
    public foreground: string;
}

export class Connection implements IConnection {
    public id: number;
    public name: string;
    public parentId: number;
    public sourceId: number;
    public targetId: number;
    public type: string;
    public stroke: string;
    public strokeOpacity: number;
    public strokeWidth: number;
    public strokeDashPattern: string;
    public label: string;
    public sourceLabel: string;
    public targetLabel: string;
    public points: IPoint[];
    public startArrow: string;
    public endArrow: string;
    public props: IProp[];
    public zIndex: number;
    public isShape: boolean;
}

export class Point implements IPoint {
    public y: number;
    public x: number;
}

export class Prop implements IProp {
    public name: string;
    public value: any;
}

export class DiagramServiceMock implements IDiagramService {

    public static $inject = ["$http", "$q"];

    public static genericDiagram = DiagramServiceMock.createGenericDiagramMock();

    constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
    }

    public getDiagram(id: number): ng.IPromise<IDiagram> {
        var deferred: ng.IDeferred<IDiagram> = this.$q.defer<IDiagram>();
        if (this.diagramMock) {
            deferred.resolve(this.diagramMock);
        } else {
            deferred.resolve(DiagramServiceMock.genericDiagram);
        }
        return deferred.promise;
    }

    public diagramMock: IDiagram;

    private static createGenericDiagramMock(): IDiagram {
        var diagram = new Diagram();
        diagram.id = 555;
        diagram.diagramType = Diagrams.GENERIC_DIAGRAM;
        diagram.height = 800;
        diagram.width = 1024;
        diagram.shapes = new Array<Shape>();
        diagram.connections = new Array<Connection>();

        var shape1 = new Shape();
        shape1.id = 1;
        shape1.height = 80;
        shape1.width = 100;
        shape1.x = 100;
        shape1.y = 100;
        shape1.label = "shape1 label";
        diagram.shapes.push(shape1);

        var shape2 = new Shape();
        shape2.id = 2;
        shape2.height = 180;
        shape2.width = 200;
        shape2.x = 200;
        shape2.y = 200;
        shape2.label = "shape2 label";
        diagram.shapes.push(shape2);

        var connection1 = new Connection();
        connection1.id = 3;
        connection1.label = "connection label";
        connection1.sourceId = 1;
        connection1.targetId = 2;

        diagram.connections.push(connection1);

        return diagram;
    }

    public static createDiagramMock(shapes: Array<IShape>, connections?: Array<Connection>, diagramType?: string): IDiagram {
        var diagram = new Diagram();
        diagram.id = 555;
        diagram.diagramType = diagramType ? diagramType : Diagrams.GENERIC_DIAGRAM;
        diagram.height = 800;
        diagram.width = 1024;
        diagram.shapes = shapes;
        diagram.connections = connections ? connections : [];
        return diagram;
    }

    public static createImageShape(x: number, y: number, w: number, h: number, aspectRatio?: boolean, url?: string): IShape {
        var shape = new Shape();
        shape.type = "Image";
        shape.id = 2;
        shape.height = h;
        shape.width = w;
        shape.x = x;
        shape.y = y;
        shape.label = "Image: " + "x=" + x + "; y=" + y + "; width=" + w + "; height=" + h;
        shape.props = [];

        var prop;
        if (aspectRatio != null) {
            prop = new Prop();
            prop.name = "IsKeepAspectRatio";
            prop.value = aspectRatio;
            shape.props.push(prop);
        }
        if (url != null) {
            prop = new Prop();
            prop.name = "Image";
            prop.value = url;
            shape.props.push(prop);
        }
        return shape;
    }

    public static createShape(shapeType: string, props?: Array<Prop>, id?: number, x?: number, y?: number, w?: number, h?: number): IHierarchyElement {
        var shape = new Shape();
        shape.type = shapeType;
        shape.id = id ? id : 1;
        shape.parentId = null;
        shape.height = h ? h : 100;
        shape.width = w ? w : 100;
        shape.x = x ? x : 100;
        shape.y = y ? y : 100;
        var label = shapeType + ": " + "x=" + shape.x + "; y=" + shape.y + "; width=" + shape.width + "; height=" + shape.height;
        shape.label = DiagramServiceMock.createRichText(label);
        shape.props = props ? props : [];
        return <IHierarchyElement><any>shape;
    }

    public static createConnection(connectionType?: string, points?: Array<Point>, props?: Array<Prop>): IConnection {
        var connection = new Connection();
        if (connectionType) {
            connection.type = connectionType;
        }
        if (points) {
            connection.points = points;
        }
        // Only create points if they are undefined
        if (typeof points === "undefined") {
            connection.points = [];
            connection.points.push(<Point>{ x: 0, y: 0 });
            connection.points.push(<Point>{ x: 100, y: 100 });
        }
        // If points are explicity set to null - set points to null
        if (points === null) {
            connection.points = null;
        }
        // Ensure that connection is visible
        connection.stroke = "black";
        connection.strokeOpacity = 1;
        connection.strokeWidth = 1;
        connection.props = props ? props : [];
        return connection;
    }

    public static createRichText(plainText: string) {
        return "<html><head></head><body style=\"padding: 1px 0px 0px; font-size: 11px\"><div style=\"font-size: 11px\"><p style=\"margin: 0px; font-size: 11px; text-align: center\"><span style=\"font-size: 11px\">" + plainText + "</span></p></div></body></html>";
    }

    public isDiagram(itemType: ItemTypePredefined) {
        switch (itemType) {
            case ItemTypePredefined.GenericDiagram:
            case ItemTypePredefined.UIMockup:
            case ItemTypePredefined.BusinessProcess:
            case ItemTypePredefined.DomainDiagram:
            case ItemTypePredefined.Storyboard:
            case ItemTypePredefined.UseCaseDiagram:
            case ItemTypePredefined.UseCase:
                return true;
            default:
                return false;
        }
    }
}