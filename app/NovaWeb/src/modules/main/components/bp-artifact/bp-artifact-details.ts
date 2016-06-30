import {IProjectManager, Models} from "../..";
import {tinymceMentionsData} from "../../../util/tinymce-mentions.mock.ts";

export class BpArtifactDetails implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-details.html");
    public controller: Function = BpArtifactRetailsController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        currentArtifact: "<",
    };
    public transclude: boolean = true;
}

export class BpArtifactRetailsController {
    private _subscribers: Rx.IDisposable[];
    static $inject: [string] = ["$scope", "projectManager"];
    private _artifact: Models.IArtifactDetails;

    public currentArtifact: string;

    constructor(private $scope, private projectManager: IProjectManager) {
        $scope.tinymceOptions = {
            inline: true,
            plugins: "advlist autolink link image paste lists charmap print preview mention",
            mentions: {
                source: tinymceMentionsData,
                delay: 100,
                items: 5,
                queryBy: "fullname",
                insert: function (item) {
                    return `<a class="mceNonEditable" href="mailto:` + item.emailaddress + `" title="ID# ` + item.id + `">` + item.fullname + `</a>`;
                }
            },
            fixed_toolbar_container: "#tinymce-toolbar"
        };
    }
    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
//            this.projectManager.currentArtifact.subscribeOnNext(this.loadView, this),
        ];
    }
    public model = {
        firstName: "John",
        description: "Fusce pellentesque pellentesque augue, sit amet ultricies mauris dictum sit amet. Vestibulum sed leo suscipit, dignissim nisi non, dictum tellus. Etiam tincidunt nisl at ante vehicula, vitae pretium eros semper. Maecenas eu lacus faucibus, pretium sapien in, ullamcorper magna. Aenean eget bibendum orci, sit amet rutrum metus. Mauris non justo at mauris viverra ultricies sed vitae odio. Nunc volutpat nisi ac magna efficitur, ut dignissim erat sodales. In non lorem mi. Nam ipsum lectus, luctus vitae tellus quis, porta imperdiet nisi. Sed vehicula risus vitae dolor aliquet lacinia. Nam convallis gravida enim. Etiam congue quam in lectus iaculis, at pretium libero ultrices. Integer tempus nunc sed eleifend imperdiet. Cras sed tempus felis, sed sodales ante. Ut auctor vitae dolor eget blandit."
    };
    public fields = [
        {
            className: "property-group",
            key: "name",
            type: "input",
            templateOptions: {
                label: "Name",
                required: true
            }
        },
        {   className: "property-group",
            key: "type",
            type: "select",
            templateOptions: {
                label: "Type",
                options: [
                    {
                        "name": "Snickers",
                        "value": "snickers"
                    },
                    {
                        "name": "Baby Ruth",
                        "value": "baby_ruth"
                    }]
            }
        },
        {
            className: "property-group",
            key: "createdBy",
            type: "input",
            templateOptions: {
                label: "Created by",
            }
        },
        {
            className: "property-group",
            key: "createdOn",
            type: "input",
            templateOptions: {
                label: "Created on",
            }
        },
        {
            className: "property-group",
            key: "lastEditBy",
            type: "input",
            templateOptions: {
                label: "Last edited by",
            }
        },
        {
            className: "property-group",
            key: "lastEditOn",
            type: "input",
            templateOptions: {
                label: "Last edited by",
            }
        }/*,
        {
            key: 'textarea1',
            type: 'tinymce',
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standart tinymce option
                    inline: false,
                    skin: 'lightgray',
                    theme: 'modern',
                    plugins: [
                        'advlist autolink lists link image charmap print preview hr anchor pagebreak',
                        'searchreplace wordcount visualblocks visualchars code fullscreen',
                        'insertdatetime media nonbreaking save table contextmenu directionality',
                        'emoticons template paste textcolor colorpicker textpattern imagetools'
                    ],

                    image_advtab: true,
                    toolbar1: 'insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image',
                    toolbar2: 'print preview media | forecolor backcolor emoticons',

                }
            },
            templateOptions: {
                label: 'a tinymce',
            }
        }*/
    ];
    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private properties: Models.IPropertyType[];
    public loadView(artifact: Models.IArtifactDetails) {
        this._artifact = artifact;
        this.properties = this.projectManager.getArtifactPropertyFileds(artifact);
        this.properties.forEach((it: Models.IPropertyType) => {
            return {
                key: it.name,
                type: "input",
                templateOptions: {
                    type: "text",
                    label: "Last Name",
                    required: it
                }
            };
        });
    }

}
