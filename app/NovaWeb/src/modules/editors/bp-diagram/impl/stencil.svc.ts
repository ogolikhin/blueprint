export interface IStencilService {
    getStencil(diagramType: string): HTMLElement;
}

export class StencilService implements IStencilService {

    public static $inject = ["mxUtils", "$http"];

    constructor(private mxUtils: MxUtils) {
    }

    public getStencil(diagramType: string): HTMLElement {
        let pathToStencil = mxBasePath + "/stencils/";
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
        let stencil = null;
        try {
            const req = <XMLHttpRequest>this.mxUtils.load(pathToStencil).request;
            stencil = req.responseXML.documentElement;
        } catch (e) {
//fixme: why is this empty? try catch is a very expensive operation and as should should not be empty
        }
        return stencil;
    }
}
