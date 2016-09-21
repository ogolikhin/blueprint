import "angular";
import "angular-mocks";
import { ILocalizationService, IUsersAndGroupsService, IUserOrGroupInfo } from "../../../../core";
import { ITinyMceMentionOptions, MentionService } from "./mention.svc";
import { UsersAndGroupsServiceMock, UserOrGroupInfo } from "../../../../core/services/users-and-groups.svc.mock";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";

/* tslint:disable:max-line-length */
describe("Mention Service Test", () => {
    var mentions: ITinyMceMentionOptions<IUserOrGroupInfo>;
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("userService", UsersAndGroupsServiceMock);
        $provide.service("localization", LocalizationServiceMock);
    }));
    it("Mentions Service Test Render User Icon", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile).create(true);
        mentions["query"] = "test"; // emulating mention plugin internal query value
        // Arrange
        var person = new UserOrGroupInfo("test name", "a@a.com", false, false);
        person.id = "id";
        //Act
        var result = mentions.render(person).innerHTML;
        //Assert
        expect(result.indexOf(`<bp-avatar icon="" name="test name"`) >= 0).toBeTruthy();
    }));
    it("Mentions Service Test Render User Unauthorize Icon", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile).create(true);
        // Arrange
        var person = new UserOrGroupInfo("test name", "a@a.com", false, false, true);
        person.id = "id";
        //Act
        var result = mentions.render(person).innerHTML;
        //Assert
        expect(result.indexOf(`<img src="/novaweb/static/images/icons/user-unauthorize.svg" height="25" width="25">`) >= 0).toBeTruthy();
    }));
    it("Mentions Service Test Render User Email Icon", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile).create(true);
        // Arrange
        var person = new UserOrGroupInfo("test name", "a@a.com", false, true, false);
        person.id = "id";
        //Act
        var result = mentions.render(person).innerHTML;
        //Assert
        expect(result.indexOf(`<img src="/novaweb/static/images/icons/user-email.svg" height="25" width="25">`) >= 0).toBeTruthy();
    }));
    it("Mentions Service Test Render Group Icon", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        // Arrange
        var person = new UserOrGroupInfo("test name", "a@a.com", true, false, false);
        person.id = "id";
        //Act
        var result = mentions.render(person).innerHTML;
        //Assert
        expect(result.indexOf(`<img src="/novaweb/static/images/icons/user-group.svg" height="25" width="25">`) >= 0).toBeTruthy();
    }));
    it("Mentions Service Test Render No Icon (No Email)", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        // Arrange
        var person = new UserOrGroupInfo("test name", null, true, false, false);
        //Act
        var result = mentions.render(person).innerHTML;
        //Assert
        expect(result).toEqual(`<a href="javascript:;"><small>Add new: </small>test name</a>`);
    }));
    it("Mentions Service Test Insert Name No Id", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        // Arrange
        var person = new UserOrGroupInfo("test name", "a@a.com", true, false, false);
        //Act
        var result = mentions.insert(person);
        //Assert
        expect(result.indexOf(`linkassemblyqualifiedname`) >= 0).toBeTruthy();
    }));
    it("Mentions Service Test Insert Name With Id", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        // Arrange
        var person = new UserOrGroupInfo("test name", "a@a.com", true, false, false);
        person.id = "1";
        //simulate mentions plugin object conversion
        var convertedPerson: any = person;
        convertedPerson.isgroup = person.isGroup;
        //Act
        var result = mentions.insert(convertedPerson);
        //Assert
        expect(result.indexOf(`linkassemblyqualifiedname`) >= 0).toBeTruthy();
    }));
    it("Mentions Service Test Source Query Too Short", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        var scope = $rootScope.$new();
        var hasRan = false;
        var resultItems: IUserOrGroupInfo[];
        function process(items: IUserOrGroupInfo[]) {
            hasRan = true;
        }
        //Act
        mentions.source("a", process);
        scope.$digest();

        expect(hasRan).toEqual(true);
        expect(resultItems).toBeUndefined;
    }));
    it("Mentions Service Test Source Query Returns User", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        var scope = $rootScope.$new();
        var hasRan = false;
        var resultItems: IUserOrGroupInfo[];
        function process(items: IUserOrGroupInfo[]) {
            hasRan = true;
        }
        //Act
        mentions.source("return@user.com", process);
        scope.$digest();

        expect(hasRan).toEqual(true);
        expect(resultItems).toBeUndefined;
    }));
    it("Mentions Service Test Source Query Returns No User But Query is a valid email", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        var scope = $rootScope.$new();
        var hasRan = false;
        var resultItems: IUserOrGroupInfo[] = null;
        function process(items: IUserOrGroupInfo[]) {
            hasRan = true;
            resultItems = items;
        }
        //Act
        mentions.source("dontreturn@user.com", process);
        scope.$digest();

        expect(hasRan).toEqual(true);
        expect(resultItems.length).toEqual(1);
        expect(resultItems[0].name).toEqual("dontreturn@user.com");
        expect(resultItems[0].email).toEqual(resultItems[0].name);
        expect(resultItems[0].id).toBeUndefined();
    }));

    it("Mentions Service Test Source Query Returns No User and Query is Not a Valid Email", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        var scope = $rootScope.$new();
        var hasRan = false;
        var resultItems: IUserOrGroupInfo[] = null;
        function process(items: IUserOrGroupInfo[]) {
            hasRan = true;
            resultItems = items;
        }
        //Act
        mentions.source("dontreturn", process);
        scope.$digest();

        expect(hasRan).toEqual(true);
        expect(resultItems.length).toEqual(0);
    }));

    it("Mentions Service Test Source Query Returns Error", inject(($compile: ng.ICompileService, userService: IUsersAndGroupsService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        mentions = new MentionService(userService, $rootScope, localization, $compile);
        var scope = $rootScope.$new();
        var hasRan = false;
        function process(items: IUserOrGroupInfo[]) {
            hasRan = true;
        }
        //Act
        var result = mentions.source("error", process);
        scope.$digest();

        expect(result).toEqual(undefined);
    }));

    describe("matcher function", () => {
        beforeEach(() => {
            mentions = new MentionService(null, null, null, null);
        });

        it("name is null/undefined, email match", () => {
            // Assign
            mentions["queryText"] = "BLUE";

            //Act
            var result = mentions.matcher(<IUserOrGroupInfo>{
                email: "user@Blueprint.com"
            });

            //Assert
            expect(result).toBeTruthy();
        });

        it("name is not match, email null/undefined", () => {
            // Assign
            mentions = new MentionService(null, null, null, null);
            mentions["queryText"] = "blue";

            //Act
            var result = mentions.matcher(<IUserOrGroupInfo>{
                name: "User"
            });

            //Assert
            expect(result).toBeFalsy();
        });

        it("name match, email null/undefined", () => {
            // Assign
            mentions = new MentionService(null, null, null, null);
            mentions["queryText"] = "bluE";

            //Act
            var result = mentions.matcher(<IUserOrGroupInfo>{
                name: "Blueprint Admin"
            });

            //Assert
            expect(result).toBeTruthy();
        });
    });

    it("highlighter doing nothing", () => {
        // Assign
        mentions = new MentionService(null, null, null, null);
        mentions["query"] = "text";

        //Act
        var result = mentions.highlighter("some text");

        //Assert
        expect(result).toEqual("some text");
    });
});
/* tslint:enable:max-line-length */