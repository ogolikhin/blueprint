import "angular";
import "angular-mocks";
import "lodash";
import {IProjectService} from "../../../managers/project-manager/";
import {SearchServiceModels} from "../../models";
import {ItemTypePredefined} from "../../models/item-type-predefined";
import {ArtifactSearchResultVM, ProjectSearchResultVM, SearchResultVM} from "./search-result-vm";

describe("SearchResultVMFactory", () => {
    let projectService: IProjectService;
    let onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean;

    beforeEach(() => {
        projectService = jasmine.createSpyObj("projectService", ["getFolders", "getArtifacts", "getSubArtifactTree"]) as IProjectService;
        onSelect = jasmine.createSpy("onSelect");
    });

    describe("ProjectSearchResultVM", () => {
        it("select calls onSelect", () => {
            // Arrange
            const model = {} as SearchServiceModels.ISearchResult;
            const searchResultVM = new ProjectSearchResultVM(model, onSelect);
            const value = true;

            // Act
            searchResultVM.selected(value);

            // Assert
            expect(onSelect).toHaveBeenCalledWith(searchResultVM, value);
        });

        it("id returns correct result", () => {
            // Arrange
            const model = {} as SearchServiceModels.ISearchResult;
            const searchResultVM = new ProjectSearchResultVM(model, onSelect);

            // Act
            const result = searchResultVM.id;

            // Assert
            expect(result).toEqual("");
        });

        it("iconClass returns correct result", () => {
            // Arrange
            const model = {} as SearchServiceModels.ISearchResult;
            const searchResultVM = new ProjectSearchResultVM(model, onSelect);

            // Act
            const result = searchResultVM.iconClass;

            // Assert
            expect(result).toEqual("icon-project");
        });
    });

    describe("ArtifactSearchResultVM", () => {
        it("select, when selectable, calls onSelect", () => {
            // Arrange
            const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = new ArtifactSearchResultVM(model, onSelect, () => true);
            const value = true;

            // Act
            searchResultVM.selected(value);

            // Assert
            expect(onSelect).toHaveBeenCalledWith(searchResultVM, value);
        });

        it("select, when not selectable, does not call onSelect", () => {
            // Arrange
            const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = new ArtifactSearchResultVM(model, onSelect, () => false);
            const value = true;

            // Act
            searchResultVM.selected(value);

            // Assert
            expect(onSelect).not.toHaveBeenCalled();
        });

        it("id returns correct result", () => {
            // Arrange
            const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = new ArtifactSearchResultVM(model, onSelect);

            // Act
            const result = searchResultVM.id;

            // Assert
            expect(result).toEqual("AC123");
        });

        it("iconClass returns correct result", () => {
            // Arrange
            const model = {id: 123, itemId: 123, predefinedType: ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = new ArtifactSearchResultVM(model, onSelect);

            // Act
            const result = searchResultVM.iconClass;

            // Assert
            expect(result).toEqual("icon-actor");
        });
    });
});
