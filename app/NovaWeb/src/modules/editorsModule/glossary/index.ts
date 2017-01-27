require("./glossary.scss");

import {BpGlossary} from "./glossary.component";
import {GlossaryService} from "./glossary.service";

export const GlossaryEditor = angular.module("glossaryEditor", [])
    .service("glossaryService", GlossaryService)
    .component("bpGlossary", new BpGlossary())
    .name;
