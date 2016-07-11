export interface IStencilService {
        getStencil(diagramType: string): HTMLElement;
    }

export class StencilService implements IStencilService {

    public static $inject = ["mxUtils"];

    constructor(private mxUtils: MxUtils) {
    }

    public getStencil(diagramType: string): HTMLElement {
        var pathToStencil = mxBasePath + "/stencils/";
        switch (diagramType) {
            case "businessprocess":
                pathToStencil += "bpmn.xml";
                break;
            case "genericdiagram":
                pathToStencil += "generic.xml";
                break;
            case "uimockup":
                pathToStencil += "uimockup.xml";
                break;
            case "storyboard":
                pathToStencil += "storyboard.xml";
                break;
            case "usecasediagram":
                pathToStencil += "usecasediagram.xml";
                break;
            default:
                return null;
        }
        var stencil = null;
        try {
            var req = <XMLHttpRequest>this.mxUtils.load(pathToStencil).request;
            stencil = req.responseXML.documentElement;
        } catch (e) {

        }
        return stencil;
    }
}
