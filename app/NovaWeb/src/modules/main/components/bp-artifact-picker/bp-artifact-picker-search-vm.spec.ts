import "angular";
import "angular-mocks";
import {Models, SearchServiceModels} from "../../models";
import {ProjectSearchResultVM, ArtifactSearchResultVM} from "./bp-artifact-picker-search-vm";

describe("ProjectSearchResultVM", () => {
    it("select calls onSelect", () => {
        // Arrange
        const model = {} as SearchServiceModels.ISearchResult;
        const onSelect = jasmine.createSpy("onSelect");
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
        const searchResultVM = new ProjectSearchResultVM(model, undefined);

        // Act
        const result = searchResultVM.id;

        // Assert
        expect(result).toEqual("");
    });

    it("iconClass returns correct result", () => {
        // Arrange
        const model = {} as SearchServiceModels.ISearchResult;
        const searchResultVM = new ProjectSearchResultVM(model, undefined);

        // Act
        const result = searchResultVM.iconClass;

        // Assert
        expect(result).toEqual("icon-project");
    });
});

describe("ArtifactSearchResultVM", () => {
    it("select calls onSelect", () => {
        // Arrange
        const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
        const onSelect = jasmine.createSpy("onSelect");
        const searchResultVM = new ArtifactSearchResultVM(model, onSelect);
        const value = true;

        // Act
        searchResultVM.selected(value);

        // Assert
        expect(onSelect).toHaveBeenCalledWith(searchResultVM, value);
    });

    it("id returns correct result", () => {
        // Arrange
        const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
        const searchResultVM = new ArtifactSearchResultVM(model, undefined);

        // Act
        const result = searchResultVM.id;

        // Assert
        expect(result).toEqual("AC123");
    });

    it("iconClass returns correct result", () => {
        // Arrange
        const model = {id: 123, itemId: 123, predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
        const searchResultVM = new ArtifactSearchResultVM(model, undefined);

        // Act
        const result = searchResultVM.iconClass;

        // Assert
        expect(result).toEqual("icon-actor");
    });
});
