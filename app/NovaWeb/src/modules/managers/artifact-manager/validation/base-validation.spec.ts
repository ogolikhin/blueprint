import "angular";
import "angular-mocks";
import {BaseValidation} from "./base-validation";

describe("date validation tests - ", () => {
    let baseValidation: BaseValidation;

    beforeEach(inject(() => {
        baseValidation = new BaseValidation();
    }));


    it("required - some value - success", () => {
        const value = "test";
        // act
        const result = baseValidation.hasValueIfRequired(true, value, value, true);

        // assert
        expect(result).toBeTruthy();
    });

    it("required - null value - fails", () => {
        const value = null;
        // act
        const result = baseValidation.hasValueIfRequired(true, value, value, true);

        // assert
        expect(result).toBeFalsy();
    });

    it("not required - null value - success", () => {
        const value = null;
        // act
        const result = baseValidation.hasValueIfRequired(false, value, value, true);

        // assert
        expect(result).toBeTruthy();
    });

    it("required is not validatied- null value - success", () => {
        const value = null;
        // act
        const result = baseValidation.hasValueIfRequired(true, value, value, false);

        // assert
        expect(result).toBeTruthy();
    });
});