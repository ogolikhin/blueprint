import "./bp-glossary";
import "./bp-artifact";
import "./bp-diagram";
import "./bp-process";

angular.module("bp.editors", [
    "bp.editors.glossary",
    "bp.editors.details",
    "bp.editors.diagram",
    "bp.editors.process"
]);
