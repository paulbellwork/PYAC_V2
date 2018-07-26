//CDSC-6659
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ACQC.Models
{
    public class CommonVariables
    {
        //Build1
        public const String DEVELOPMENT_ENVIRONMENT = "Development Environment";
        public const String LOCAL_HOST = "Localhost";
        public const String DVLP = "DVLP";
        public const String TEST = "Test";
        public const String TEST_ENVIRONMENT = "Test Environment";
        public const String OPERATING_MODE = "OperatingMode";
        public const String ENVIRONMENT_PARTIAL = "~/Views/Shared/_Environment.cshtml";

        public static class Environment
        {
            public const string ENVIRONMENT_PRODUCTION = "Production";
            public const string ENVIRONMENT_DEVELOPMENT = "Development";
            public const string ENVIRONMENT_TEST = "Test";

            public const string DEV_SERVER = "txbapphur819v";
            public const string TEST_SERVER = "txbapphur934v";
            public const string PROD_SERVER = "txbapphur192v";

            public const string HOST_NAME_PRODUCTION = "bellapps.bh.textron.com";
            public const string HOST_NAME_DEVELOPMENT = "bellapps-dvlp.bh.textron.com";
            public const string HOST_NAME_TEST = "bellapps-test.bh.textron.com";

        }

        public static class Nlog
        {
            public const string HOST_NAME_PRODUCTION = "bellapps.bh.textron.com";
            public const string HOST_NAME_DEVELOPMENT = "bellapps-dvlp.bh.textron.com";
            public const string HOST_NAME_TEST = "bellapps-test.bh.textron.com";
            public const string HOST_NAME_LOCALHOST = "localhost";

            public const string ENVIRONMENT_PRODUCTION = "Production";
            public const string ENVIRONMENT_DEVELOPMENT = "Development";
            public const string ENVIRONMENT_TEST = "Test";

            public const string DEVELOPMENT_SERVER = "txbapphur819v";

            public const string DECRYPT_KEY = "MRI_CDSCLOG_K";
            public const string DEVELOPMENT_KEY = "CdscLog.Dvlp";
            public const string TEST_KEY = "CdscLog.Test";
            public const string PROD_KEY = "CdscLog.Prod";

            public const string LABEL_DECRYPT_CS = "DecryptCS";
            public const string LABEL_STACK_TRACE = "stack-trace";
            public const string LABEL_CALL_SITE = "call-site";
            public const string LABEL_LOGGER = "logger";
            public const string LABEL_ENVIRONMENT = "Environment";
        }
    }
}