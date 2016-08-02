import "angular";

export class ArtifactDetailsState implements ng.ui.IState {
    public template = '<bp-artifact-editor></bp-artifact-editor>';

    public onEnter = () => {
        let enter = "test";
        console.log("artifact editor");
    };

    public onExit = () => {
        let ex = "test";
    };
}