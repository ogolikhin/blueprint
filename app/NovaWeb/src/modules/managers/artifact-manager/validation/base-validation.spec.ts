import "angular";
import "angular-mocks";
import {BaseValidation} from "./base-validation";

describe("BaseValidation tests -", () => {
    let baseValidation: BaseValidation;

    beforeEach(inject(() => {
        baseValidation = new BaseValidation();
    }));

    describe("hasValueIfRequired -", () => {


        it("returns true when required value is provided", () => {
            const value = "test";
            // act
            const result = baseValidation.hasValueIfRequired(true, value, value, true);

            // assert
            expect(result).toBe(true);
        });

        it("returns false when required value is null", () => {
            const value = null;
            // act
            const result = baseValidation.hasValueIfRequired(true, value, value, true);

            // assert
            expect(result).toBe(false);
        });

        it("returns true when non-required value is provided", () => {
            const value = null;
            // act
            const result = baseValidation.hasValueIfRequired(false, value, value, true);

            // assert
            expect(result).toBe(true);
        });

        it("returns true when required value does not need to be validated", () => {
            const value = null;
            // act
            const result = baseValidation.hasValueIfRequired(true, value, value, false);

            // assert
            expect(result).toBe(false);
        });

    });
});