                                                                                    import {IStencilService} from "./impl/stencil.svc";
                        export class StencilServiceMock implements IStencilService {
                        /* tslint:disable:max-line-length */
                        public getStencil(diagramType: string): HTMLElement {
                        let data: string;
                        switch (diagramType) {
                        case "businessprocess":
                        data = "
                        ";
                            break;
                            case "genericdiagram":
                            data = "
                            ";
                                break;
                                case "uimockup":
                                data = "
                                ";
                                    break;
                                    case "storyboard":
                                    data = "
                                    ";
                                        break;
                                        case "domaindiagram":
                                        data = "
                                        ";
                                            break;
                                            case "usecasediagram":
                                            data = "
                                            ";
                                                break;
                                                default:
                                                throw "Unknown diagram type: " + diagramType;
                                                }
                                                let stencil = null;
                                                try {
                                                let xml = $.parseXML(data);
                                                stencil = xml.documentElement;
                                                }
                                                finally {
                                                return stencil;
                                                }
                                                }
                                                /* tslint:enable:max-line-length */
                                                }


                                                