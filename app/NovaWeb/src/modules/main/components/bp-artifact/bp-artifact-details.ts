import {IProjectManager, Models} from "../..";

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
        //TEST VALUES FOR MENTIONS
        let dummySource = [
            {
                "id": "AAUQADgIDCALOpNEiqOH0K4SCAc=",
                "fullname": "Dennis Papp",
                "emailaddress": "dennis.papp@blueprintsys.com"
            },
            {
                "id": "AAUQAIPOUOkmhuxLqCUU5yTWUj4=",
                "fullname": "Sergey Kalinichenko",
                "emailaddress": "serg.kalinich@blueprintsys.com"
            },
            {
                "id": "AAUQADnicnUhl8NDhCZ6LjhnBWc=",
                "fullname": "Eric Chan",
                "emailaddress": "eric.chan@blueprintsys.com"
            },
            {
                "id": "AAUQACzR00+ycatBkug9NnJhBB4=",
                "fullname": "Alek Ohanian",
                "emailaddress": "alek.ohanian@blueprintsys.com"
            },
            {
                "id": "AAUQAOvzmho9NT5Ji449k3s9vAQ=",
                "fullname": "Stefan Magdziak",
                "emailaddress": "stefan.magdziak@blueprintsys.com"
            },
            {
                "id": "AAUQAKw7CqT18GpCvx\/4oPrsy2U=",
                "fullname": "Warren Szeto",
                "emailaddress": "warren.szeto@blueprintsys.com"
            },
            {
                "id": "AAUQAHyPSmoxZExNrl8wDF1e\/tk=",
                "fullname": "Michael Aronzon",
                "emailaddress": "michael.aronzon@blueprintsys.com"
            },
            {
                "id": "AAUQAOiWTKFgTAJNnUpqUTCbofY=",
                "fullname": "Ray Buck",
                "emailaddress": "ray.buck@blueprintsys.com"
            },
            {
                "id": "AAUQAP6QLf9XHFJLj+KaycXJy8k=",
                "fullname": "Roger Kwan",
                "emailaddress": "roger.kwan@blueprintsys.com"
            },
            {
                "id": "AAUQAKn3iWCckSxCslk7Y4HBljI=",
                "fullname": "Rosanne Scott",
                "emailaddress": "rosanne.scott@blueprintsys.com"
            },
            {
                "id": "AAUQAF9h+uc\/HspGtKkAdTojDhw=",
                "fullname": "Dmitry Agapitov",
                "emailaddress": "dmitry.agapitov@blueprintsys.com"
            },
            {
                "id": "AAUQAFqgiB9hWoBMnBwccw9ft4E=",
                "fullname": "Greg Kent",
                "emailaddress": "greg.kent@blueprintsys.com"
            },
            {
                "id": "AAUQAAdOJMsh6tFMsfXkM4Gy7N0=",
                "fullname": "Alexander Groman",
                "emailaddress": "alexander.groman@blueprintsys.com"
            },
            {
                "id": "AAUQACkkWlhzkUpJmWijQ0VwKvg=",
                "fullname": "Nick Karavas",
                "emailaddress": "nick.karavas@blueprintsys.com"
            },
            {
                "id": "AAUQAIB7ScMIX7pLi7vIRyK5cqI=",
                "fullname": "Roman Dolgov",
                "emailaddress": "roman.dolgov@blueprintsys.com"
            },
            {
                "id": "AAUQAPvzAWC+Le5KvJtX5m4GysU=",
                "fullname": "Susan Lu",
                "emailaddress": "susan.lu@blueprintsys.com"
            },
            {
                "id": "AAUQAEoOmNHRZhJFqNL\/DT\/qyos=",
                "fullname": "Munish Saini",
                "emailaddress": "munish.saini@blueprintsys.com"
            },
            {
                "id": "AAUQAACQElgm9y9EqxGfHtnhytQ=",
                "fullname": "Tony Higgins",
                "emailaddress": "tony.higgins@blueprintsys.com"
            },
            {
                "id": "AAUQACdPxS9df\/tIgcAprkRTeFI=",
                "fullname": "Glen Stone",
                "emailaddress": "glen.stone@blueprintsys.com"
            },
            {
                "id": "AAUQAEVew3556hdJvxmoo51CEnM=",
                "fullname": "Alexander Utkin",
                "emailaddress": "alexander.utkin@blueprintsys.com"
            },
            {
                "id": "AAUQAHtbrWBR6QxNtR2sbxYF3WM=",
                "fullname": "Alexandre Folomechine",
                "emailaddress": "alex.folomechine@blueprintsys.com"
            },
            {
                "id": "AAUQAIqxKeC8XH9Ot8LgdSZIsZ4=",
                "fullname": "Dan Shimmerman",
                "emailaddress": "dan.shimmerman@blueprintsys.com"
            },
            {
                "id": "AAUQAKg\/DEet\/PZMnY00RujbkGA=",
                "fullname": "Anna Tsaitlin",
                "emailaddress": "anna.tsaitlin@blueprintsys.com"
            },
            {
                "id": "AAUQAKvcVWz5i4NAvGFulaywNrk=",
                "fullname": "Anton Trinkunas",
                "emailaddress": "anton.trinkunas@blueprintsys.com"
            },
            {
                "id": "AAUQAGF7ZczI9PVMhobBec7rAC0=",
                "fullname": "Richard Plomp",
                "emailaddress": "richard.plomp@blueprintsys.com"
            },
            {
                "id": "AAUQAH6UwZATu+tAtvONy8mEhVM=",
                "fullname": "Matthew Dodgson",
                "emailaddress": "matthew.dodgson@blueprintsys.com"
            },
            {
                "id": "AAUQAP7UlcGCoI1KsL9orcC3mNE=",
                "fullname": "Sabuhi Yahyayev",
                "emailaddress": "sabuhi.yahyayev@blueprintsys.com"
            },
            {
                "id": "AAUQAEqfxjxIyP9ClptX1+acWVo=",
                "fullname": "Kandida Chen",
                "emailaddress": "kandida.chen@blueprintsys.com"
            },
            {
                "id": "AAUQANVa8hRsFbFGl1ZCFFhIlSY=",
                "fullname": "Maria Fong",
                "emailaddress": "maria.fong@blueprintsys.com"
            },
            {
                "id": "AAUQAC+rcubPXU5Hgzv5oGm7Bes=",
                "fullname": "Luigi Mariani",
                "emailaddress": "luigi.mariani@blueprintsys.com"
            },
            {
                "id": "AAUQAMbX64ukyd1Espf4iW2GF1k=",
                "fullname": "Vicky Goudogiannis",
                "emailaddress": "vicky.goudogiannis@blueprintsys.com"
            },
            {
                "id": "AAUQAALJhMmHHS5AkpFfssh7F0c=",
                "fullname": "Tara Bedford",
                "emailaddress": "tara.bedford@blueprintsys.com"
            },
            {
                "id": "AAUQAFWKOTX5CY1MtwGfX+vqULc=",
                "fullname": "Jim Roper",
                "emailaddress": "jim.roper@blueprintsys.com"
            },
            {
                "id": "AAUQAIoO2bSsX\/1IjHsmXCRrLyE=",
                "fullname": "Martin Saipe",
                "emailaddress": "martin.saipe@blueprintsys.com"
            },
            {
                "id": "AAUQAF4cBGNW0+lEggYGqF6ebso=",
                "fullname": "Saranya Yogarajah",
                "emailaddress": "saranya.yogarajah@blueprintsys.com"
            },
            {
                "id": "AAUQAJe9UOSm5wpJgQtgiQG66uQ=",
                "fullname": "Brian Hartlen",
                "emailaddress": "brian.hartlen@blueprintsys.com"
            },
            {
                "id": "AAUQAFLdgUpc\/ZlAvphSn9TXnR0=",
                "fullname": "Pavlo Pochapsky",
                "emailaddress": "Pavlo.Pochapsky@blueprintsys.com"
            },
            {
                "id": "AAUQAPC6KN4HgixLgGv1gtem1xU=",
                "fullname": "Mohammed Akbar",
                "emailaddress": "mohammed.akbar@blueprintsys.com"
            },
            {
                "id": "AAUQAEHNOVhydp1ClsFOVcV\/U98=",
                "fullname": "Tom Patak",
                "emailaddress": "tom.patak@blueprintsys.com"
            },
            {
                "id": "AAUQACfWW4qLkMdBuNwJn9DJZU0=",
                "fullname": "Mike Hanna",
                "emailaddress": "mike.hanna@blueprintsys.com"
            },
            {
                "id": "AAUQAAyYw\/CR\/YVAnQZiP7nq4Wk=",
                "fullname": "Fabio Gallo",
                "emailaddress": "Fabio.Gallo@blueprintsys.com"
            },
            {
                "id": "AAUQAIFmZ9iLuBhBlUub7CQjhAM=",
                "fullname": "Aman Singh",
                "emailaddress": "aman.singh@blueprintsys.com"
            },
            {
                "id": "AAUQACPY\/JX473NNjfiuKXSU\/JI=",
                "fullname": "Ilya Bass",
                "emailaddress": "ilya.bass@blueprintsys.com"
            },
            {
                "id": "AAUQAGU3GjmPJ39FjZUdKtcCR2A=",
                "fullname": "Andy Plomp",
                "emailaddress": "andy.plomp@blueprintsys.com"
            },
            {
                "id": "AAUQABnk7vfHDfBJraPyYCpepW8=",
                "fullname": "Brian Capson",
                "emailaddress": "brian.capson@blueprintsys.com"
            },
            {
                "id": "AAUQANK74ythHAdPhI3Or7qVWzM=",
                "fullname": "Daniel Goldberg",
                "emailaddress": "daniel.goldberg@blueprintsys.com"
            },
            {
                "id": "AAUQAMyEE6GtV4hFptFwEOxcByA=",
                "fullname": "Michael Arends",
                "emailaddress": "michael.arends@blueprintsys.com"
            },
            {
                "id": "AAUQAKutatjg7YdJtfv\/o1xSM24=",
                "fullname": "Sharon Sira-Macdiarmid",
                "emailaddress": "sharon.macd@blueprintsys.com"
            },
            {
                "id": "AAUQAChhv+T+hq9EqpVXGQ3jbHU=",
                "fullname": "Alex Kapsh",
                "emailaddress": "alex.kapsh@blueprintsys.com"
            },
            {
                "id": "AAUQACz0tkOHe5FIt1MIg\/jG61k=",
                "fullname": "Irina Lunin",
                "emailaddress": "irina.lunin@blueprintsys.com"
            }
        ];

        $scope.tinymceOptions = {
            onChange: function(e) {
                // put logic here for keypress and cut/paste changes
            },
            inline: true,
            plugins: "advlist autolink link image lists charmap print preview mention",
            mentions: {
                source: dummySource,
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
