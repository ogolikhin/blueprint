declare module "node-trx" {
    interface ITestRunParams {
        name: string;
        id?: string;
        runUser?: string;
    }

    interface IAddResultParams {
        test: UnitTest;
        computerName?: string;
        testList?: TestList;
        outcome?: string;
        startTime?: string;
        endTime?: string;
        duration?: string;
        executionId?: string;
        output?: string;
        errorMessage?: string;
        errorStacktrace?: string;
    }

    class TestRun {
        public testLists: TestList[];

        constructor(params: ITestRunParams);

        public toXml();
        public addResult(params: IAddResultParams);
    }

    interface IUnitTestParams {
        name: string;
        methodName: string;
        methodCodeBase: string;
        methodClassName: string;
        id?: string;
    }

    class UnitTest {
        constructor(params: IUnitTestParams);
    }

    interface ITestListParams {
        name: string;
        id?: string;
    }

    class TestList {
        public static ResultsNotInAList: TestList;
        public static AllLoadedResults: TestList;

        constructor(params: ITestListParams);
    }
}
