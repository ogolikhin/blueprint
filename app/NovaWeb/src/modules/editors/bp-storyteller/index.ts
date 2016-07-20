import {BpStorytellerEditor} from "./bp-storyteller-editor";

angular.module("app.storyteller", [])
    .component("bpStorytellerEditor", new BpStorytellerEditor);

export {BpStorytellerEditor};