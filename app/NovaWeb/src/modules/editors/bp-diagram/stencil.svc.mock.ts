import {IStencilService} from "./impl/stencil.svc";

export class StencilServiceMock implements IStencilService {
    private readonly parser = new DOMParser();

    public getStencil(diagramType: string): HTMLElement {
        let data: string;
        switch (diagramType) {
            case "businessprocess":
                data = require("../../../../libs/mxClient/stencils/bpmn.xml");
                break;
            case "genericdiagram":
                data = require("../../../../libs/mxClient/stencils/generic.xml");
                break;
            case "uimockup":
                data = require("../../../../libs/mxClient/stencils/uimockup.xml");
                break;
            case "storyboard":
                data = require("../../../../libs/mxClient/stencils/storyboard.xml");
                break;
            case "domaindiagram":
                data = "";
                break;
            case "usecasediagram":
                data = require("../../../../libs/mxClient/stencils/usecasediagram.xml");
                break;
            default:
                throw "Unknown diagram type: " + diagramType;
        }

        try {
            const xml = this.parser.parseFromString(data, "text/xml");
            return xml.documentElement;
        } catch (error) {
            return null;
        }
    }
}
