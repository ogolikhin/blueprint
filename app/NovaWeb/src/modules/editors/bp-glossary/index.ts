import * as angular from "angular";
import {BpGlossary} from "./bp-glossary";
import {GlossaryService} from "./glossary.svc";

angular.module("bp.editors.glossary", [])
    .service("glossaryService", GlossaryService)
    .component("bpGlossary", new BpGlossary());
