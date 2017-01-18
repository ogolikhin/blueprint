import "angular-mocks";
import "angular-sanitize";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";
import {Models} from "../../../main";
import {IStatefulArtifactFactory, StatefulArtifactFactory} from "../../../managers/artifact-manager";
import {ISelectionManager, SelectionManager} from "../../../managers/selection-manager/selection-manager";
import {ComponentTest} from "../../../util/component.test";
import {PageContentCtrl} from "./bp-page-content";
import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";
import {MainBreadcrumbServiceMock} from "./mainbreadcrumb.svc.mock";
import * as angular from "angular";

describe("Component BPPageContent", () => {

    let vm: PageContentCtrl;

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
        $provide.service("selectionManager", SelectionManager);
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
            selectionManager: ISelectionManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My"});

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBeGreaterThan(0);
        }));

    it("should not load breadcrumb when selected artifact is null",
        inject(($rootScope: ng.IRootScopeService,
            selectionManager: ISelectionManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const spy = spyOn(mainbreadcrumbService, "reloadBreadcrumbs");

            //Act
            selectionManager.setArtifact(null);
            $rootScope.$digest();

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBe(0);
            expect(spy).not.toHaveBeenCalled();
        }));

    it("should not load breadcrumb when selected artifact is same as current artifact",
        inject(($rootScope: ng.IRootScopeService,
            selectionManager: ISelectionManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            const subArtifactModel = {
                id: 32,
                name: "SubArtifact",
                prefix: "SA",
                predefinedType: Models.ItemTypePredefined.PROShape
            } as Models.ISubArtifact;
            mainbreadcrumbService.breadcrumbLinks = [];
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My"});
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            const spy = spyOn(mainbreadcrumbService, "reloadBreadcrumbs");
            selectionManager.setSubArtifact(subArtifact);

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBe(1);
            expect(spy).not.toHaveBeenCalled();
        }));
});
