﻿import { IUsersAndGroupsService, IUserOrGroupInfo } from "./users-and-groups.svc";
import { ILocalizationService } from "../../../../core";
import { Helper } from "../../../../shared/utils/helper";

// TinyMCE mention plugin interface - https://github.com/CogniStreamer/tinyMCE-mention
export interface ITinyMceMentionOptions<T> {
    source: (query: string, process: (items: T[]) => void) => void;
    render?: (item: T) => HTMLElement;
    // we have "any" type here instead of "T"
    // because in ArtifactMentionService this object is not of type "T" it is some other object
    // which is generated by mention plugin
    insert?: (item: any) => string;

    ///The name of the property used to do the lookup in the source.
    ///Default: 'name'.
    queryBy?: string;

    ///Checks for a match in the source collection.
    matcher?: (item: T) => boolean;

    renderDropdown?: () => string;
    highlighter?: (text: string) => string;
    delimiter?: string;
    delay?: number;
}

export interface IMentionService {
    create(areEmailDiscussionsEnabled: boolean): ITinyMceMentionOptions<IUserOrGroupInfo>;
}

export class MentionService implements IMentionService, ITinyMceMentionOptions<IUserOrGroupInfo> {
    public areEmailDiscussionsEnabled: boolean;
    public static $inject = ["usersAndGroupsService", "$rootScope", "localization", "$compile"];

    public static emailDiscussionDisabledMessage: string;

    private static emailValidator = /^[-a-z0-9~!$%^&*_=+}{\'?]+(\.[-a-z0-9~!$%^&*_=+}{\'?]+)*@([a-z0-9_][-a-z0-9_]*(\.[-a-z0-9_]+)*\.(aero|arpa|biz|com|coop|edu|gov|info|int|mil|museum|name|net|org|pro|travel|mobi|[a-z][a-z])|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?$/i;

    constructor(private usersAndGroupsService: IUsersAndGroupsService,
                private $rootScope: ng.IRootScopeService,
                private localization: ILocalizationService,
                private $compile: ng.ICompileService) {
        if (!MentionService.emailDiscussionDisabledMessage) {
            MentionService.emailDiscussionDisabledMessage = this.localization.get("Email_Discussions_Disabled_Message");
        }
    }

    public delay = 1000;

    public create(areEmailDiscussionsEnabled: boolean): ITinyMceMentionOptions<IUserOrGroupInfo> {
        let options = new MentionService(this.usersAndGroupsService, this.$rootScope, this.localization, this.$compile);

        options.areEmailDiscussionsEnabled = areEmailDiscussionsEnabled;

        return options;
    }

    public source = (query: string, process: (users: IUserOrGroupInfo[]) => void) => {
        if (query && query.length >= 3) {
            this.usersAndGroupsService.search(query, true).then(
                (users) => {
                    if (users && users.length === 0 && MentionService.emailValidator.test(query)) {
                        process([<IUserOrGroupInfo>
                            {
                                name: query,
                                email: query
                            }
                        ]);
                    } else {
                        process(users);
                    }
                },
                () => {
                    process([]);
                });
        } else {
            process([]);
        }
    }

    public matcher(person: IUserOrGroupInfo): boolean {
        // this.query is defined in the caller context (mention plugin)
        const query = (<any>this).query.toLowerCase();

        return (person.name && person.name.toLowerCase().indexOf(query) >= 0)
            || (person.email && person.email.toLowerCase().indexOf(query) >= 0);
    }

    public highlighter(text: string): string {

        //do nothing - highlight implemented in the render function
        return text;
    }

    private static highlight(query: string, text: string): string {
        if (!query) {
            return text;
        }

        return text.replace(new RegExp(`(${query})`, "ig"), ($1, match) => (`<span class="highlight">${match}</span>`));
    }

    public render = (person: IUserOrGroupInfo) => {
        let htmlToRender: string;
        let iconToRender: string;
        let boldName: boolean;
        if (person.id === "PlaceHolderEntry") { //we modified the plugin source code to insert this fake user when ever email discussions is disabled.
            const error = `<div class="error">
                               <img src="/novaweb/static/images/icons/warning.svg" height="32" width="32"/>
                               <span class="message">${MentionService.emailDiscussionDisabledMessage}</span>
                           </div>`;
            return angular.element(error)[0];
        }
        if (person.isGroup) {
            iconToRender = `<img src="/novaweb/static/images/icons/user-group.svg" height="25" width="25"/>`;
            boldName = true;
        } else if (person.isBlocked) {
            iconToRender = `<img src="/novaweb/static/images/icons/user-unauthorize.svg" height="25" width="25"/>`;
            boldName = false;
        } else if (person.guest) {
            iconToRender = `<img src="/novaweb/static/images/icons/user-email.svg" height="25" width="25"/>`;
            boldName = true;
        } else {
            iconToRender = `<bp-avatar icon="" name="${person.name}" color-base="${person.id}${person.name}"></bp-avatar>`;
            boldName = true;
        }
        if (person.id) {
            // this.query is defined in the caller context (mention plugin)
            const query = MentionService.escapeRegExp((<any>this).query);
            var nameString: string = Helper.escapeHTMLText(person.name);
            if (boldName) {
                nameString = `<strong>${MentionService.highlight(query, nameString)}</strong>`;
            } else {
                nameString = `${MentionService.highlight(query, nameString)}`;
            }
            htmlToRender = `<li><a href='javascript:;'>${iconToRender}${nameString}${(person.email && person.name !== person.email ? `
                                    <small>(${MentionService.highlight(query, person.email)})</small>` : "")}
                                </a>
                            </li>`;
        } else {
            htmlToRender = `<li><a href='javascript:;'><small>Add new: </small>${person.name}</a></li>`;
        }
        return (this.$compile(htmlToRender)(this.$rootScope)[0]);
    }

    private static escapeRegExp(str: string) {
        if (!str) {
            return str;
        }

        return str.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    }

    // TinyMCE strips unknown attributes, please update this string when changing content generated by insert method.
    public static requiredAttributes = "a[href|type|title|linkassemblyqualifiedname|text|canclick|isvalid|mentionid|isgroup|email|class|linkfontsize|linkfontfamily|linkfontstyle|linkfontweight|linktextdecoration|linkforeground|style|target]";
    public insert = (person: IUserOrGroupInfo): string => {
        if (person.id === "PlaceHolderEntry") {
            return "";
        }
        return `<a class="mceNonEditable" 
                    linkassemblyqualifiedname="BluePrintSys.RC.Client.SL.RichText.RichTextMentionLink, BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
                    text="${person.name}" 
                    canclick="True"
                    isvalid="True"
                    ${this.prepareMentionIdAttributes(person)} 
                    ${this.prepareMentionEmailAttribute(person)}>
                        <span style="font-family: 'Portable User Interface'; font-size: 13.3330001831055px; font-style: italic; font-weight: bold; color: Black; text-decoration: ; line-height: 1.45000004768372">
                            ${Helper.escapeHTMLText(person.name)}
                        </span>
                    </a> `;
    }

    private prepareMentionIdAttributes(person: any): string {
        if (person.id) {
            const id = person.id.slice(1);
            // This conversion is necessary because mentions plugin casts all fields into strings on select, and is cased differently on different browsers.
            let isgroup = (person["isgroup"] === "true" || person["isGroup"] === "true") ? "True" : "False"; 
            return `mentionid="${id}" isgroup="${isgroup}"`;
        }
        return "";
    }

    private prepareMentionEmailAttribute(person: IUserOrGroupInfo): string {
        if (person.email) {
            return `email="${person.email}"`;
        }
        return "";
    }
}