import "../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../util/component.test";
import {PageContentCtrl} from "./bp-page-content";
import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";
import {MainBreadcrumbServiceMock} from "./mainbreadcrumb.svc.mock";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {IArtifactManager, ISelection, IStatefulArtifactFactory, StatefulArtifactFactory, ArtifactManager} from "../../../managers/artifact-manager";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";

describe("Component BPPageContent", () => {

    let vm: PageContentCtrl;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("navigationService", NavigationServiceMock);
    }));

    let directiveTest: ComponentTest<PageContentCtrl>;
    let template = `<pagecontent></pagecontent>`;

    beforeEach(() => {
        directiveTest = new ComponentTest<PageContentCtrl>(template, "pagecontent");
        vm = directiveTest.createComponent({});
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find("bp-breadcrumb").length).toBe(1);
    });

    it("should load breadcrumb for a selected artifact",
        inject(($rootScope: ng.IRootScopeService,
            artifactManager: IArtifactManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const artifact = statefulArtifactFactory.createStatefulArtifact({ id: 22, name: "Artifact", prefix: "My" });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBeGreaterThan(0);
        }));

    it("should not load breadcrumb when selected artifact is null",
        inject(($rootScope: ng.IRootScopeService,
            artifactManager: IArtifactManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const spy = spyOn(mainbreadcrumbService, "reloadBreadcrumbs");

            //Act
            artifactManager.selection.setArtifact(null);
            $rootScope.$digest();

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBe(0);
            expect(spy).not.toHaveBeenCalled();
        }));

    it("should not load breadcrumb when selected artifact is same as current artifact",
        inject(($rootScope: ng.IRootScopeService,
            artifactManager: IArtifactManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const artifact = statefulArtifactFactory.createStatefulArtifact({ id: 22, name: "Artifact", prefix: "My" });

            //Act
            artifactManager.selection.setArtifact(artifact);
            $rootScope.$digest();
            const spy = spyOn(mainbreadcrumbService, "reloadBreadcrumbs");
            artifactManager.selection.setArtifact(artifact);

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBe(1);
            expect(spy).not.toHaveBeenCalled();
        }));

    it("should navigate to link",
        inject(($rootScope: ng.IRootScopeService,
            artifactManager: IArtifactManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService,
            navigationService: INavigationService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const artifact = statefulArtifactFactory.createStatefulArtifact({ id: 22, name: "Artifact", prefix: "My" });

            //Act
            artifactManager.selection.setArtifact(artifact);
            const spy = spyOn(navigationService, "navigateTo");
            $rootScope.$digest();
            vm.navigateTo(mainbreadcrumbService.breadcrumbLinks[0]);

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBeGreaterThan(0);
            expect(spy).toHaveBeenCalled();
        }));

});
