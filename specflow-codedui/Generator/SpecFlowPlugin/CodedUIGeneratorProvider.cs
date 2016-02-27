namespace TvDijk.SpecFlowPlugin
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Configuration;    
    using System.Linq;

    using TechTalk.SpecFlow.Generator;
    using TechTalk.SpecFlow.Generator.UnitTestProvider;
    using TechTalk.SpecFlow.Utils;
    
    /// <summary>
    /// The CodedUI generator.
    /// </summary>
    public class CodedUIGeneratorProvider : IUnitTestGeneratorProvider
    {
        protected string TESTBASE_CLASSNAME = Configuration.CodedUiGeneratorConfigSection.Instance.TestBase.Name;

        protected const string CODEDUITEST_ATTR = "Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute";
        protected const string CATEGORY_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute";
        protected const string OWNER_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.OwnerAttribute";
        protected const string DATASOURCE_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.DataSourceAttribute";
        protected const string WORKITEM_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute";

        protected const string OWNER_TAG = "owner:";
        protected const string WORKITEM_TAG = "workitem:";
        protected const string DATASOURCE_TAG = "datasource:";

        protected const string TESTCLASS_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
        protected const string TESTMETHOD_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
        protected const string PROPERTY_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute";
        protected const string TESTCLASSTEARUP_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute";
        protected const string TESTCLASSTEARDOWN_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute";
        protected const string TESTTEARUP_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute";
        protected const string TESTTEARDOWN_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute";
        protected const string IGNORE_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute";
        protected const string DESCRIPTION_ATTR = "Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute";

        protected const string FEATURE_TITILE_PROPERTY_NAME = "FeatureTitle";

        protected const string TESTCONTEXT_TYPE = "Microsoft.VisualStudio.TestTools.UnitTesting.TestContext";

        /// <summary>
        /// Initializes a new instance of the <see cref="CodedUiGeneratorProvider"/> class.
        /// </summary>
        /// <param name="codeDomHelper">
        /// The code dom helper.
        /// </param>
        public CodedUIGeneratorProvider(CodeDomHelper codeDomHelper)
        {
            CodeDomHelper = codeDomHelper;
        }

        /// <summary>
        /// The set test class.
        /// </summary>
        /// <param name="generationContext">
        /// The generation context.
        /// </param>
        /// <param name="featureTitle">
        /// The feature title.
        /// </param>
        /// <param name="featureDescription">
        /// The feature description.
        /// </param>
        public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, TESTCLASS_ATTR);

            foreach (CodeAttributeDeclaration declaration in generationContext.TestClass.CustomAttributes)
            {
                if (declaration.Name == TESTCLASS_ATTR)
                {
                    generationContext.TestClass.CustomAttributes.Remove(declaration);
                    break;
                }
            }
            
            generationContext.TestClass.BaseTypes.Add(new CodeTypeReference(TESTBASE_CLASSNAME));
            
            generationContext.TestClass.CustomAttributes.Add(
                new CodeAttributeDeclaration(
                    new CodeTypeReference(CODEDUITEST_ATTR)));
        }

        /// <summary>
        /// The set test class categories
        /// </summary>
        /// <param name="generationContext">
        /// The generation context.
        /// </param>
        /// <param name="featureCategories">
        /// The feature categories.
        /// </param>
        public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            generationContext.CustomData["featureCategories"] = GetNonMSTestSpecificTags(featureCategories).ToArray();

            IEnumerable<string> ownerTags = featureCategories.Where(t => t.StartsWith(OWNER_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (ownerTags.Any())
            {
                generationContext.CustomData[OWNER_TAG] = ownerTags.Select(t => t.Substring(OWNER_TAG.Length).Trim('\"')).FirstOrDefault();
            }

            IEnumerable<string> workitemTags = featureCategories.Where(t => t.StartsWith(WORKITEM_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (workitemTags.Any())
            {
                int temp;
                IEnumerable<string> workitemsAsStrings = workitemTags.Select(t => t.Substring(WORKITEM_TAG.Length).Trim('\"'));
                if (workitemsAsStrings.Any())
                {
                    generationContext.CustomData[WORKITEM_TAG] = workitemsAsStrings.Where(t => int.TryParse(t, out temp)).Select(t => int.Parse(t));
                }
            }

            // TODO implement datasource extraction
            //IEnumerable<string> datasourceTags = featureCategories.Where(t => t.StartsWith(DATASOURCE_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            //if (datasourceTags.Any())
            //{
            //    generationContext.CustomData[DATASOURCE_TAG] = datasourceTags.Select(t => t.Substring(DATASOURCE_TAG.Length).Trim('\"')).FirstOrDefault();
            //}            
        }

        /// <summary>
        /// The set test method
        /// </summary>
        /// <param name="generationContext">
        /// The generation context.
        /// </param>
        /// <param name="testMethod">
        /// The testMethod.
        /// </param>
        /// <param name="friendlyTestName">
        /// The testMethod.
        /// </param>
        public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string friendlyTestName)
        {
            CodeDomHelper.AddAttribute(testMethod, TESTMETHOD_ATTR);
            CodeDomHelper.AddAttribute(testMethod, DESCRIPTION_ATTR, friendlyTestName);

            //as in mstest, you cannot mark classes with the description attribute, we
            //just apply it for each test method as a property
            SetProperty(testMethod, FEATURE_TITILE_PROPERTY_NAME, generationContext.Feature.Name);

            if (generationContext.CustomData.ContainsKey("featureCategories"))
            {
                var featureCategories = (string[])generationContext.CustomData["featureCategories"];
                CodeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, featureCategories);
            }

            if (generationContext.CustomData.ContainsKey(OWNER_TAG))
            {
                string ownerName = generationContext.CustomData[OWNER_TAG] as string;
                if (!String.IsNullOrEmpty(ownerName))
                {
                    CodeDomHelper.AddAttribute(testMethod, OWNER_ATTR, ownerName);
                }
            }

            // TODO implement datasource
            // add Datasource attribute if the scenario has the datasource key
            //if (generationContext.CustomData.ContainsKey(DATASOURCE_TAG))
            //{
            //    string datasource = generationContext.CustomData[DATASOURCE_TAG] as string;
            //    if (!String.IsNullOrEmpty(datasource))
            //    {
            //        CodeDomHelper.AddAttribute(
            //            testMethod,
            //            DATASOURCE_ATTR,
            //            "Microsoft.VisualStudio.TestTools.DataSource.TestCase",
            //            $@"http://{"tfs-server-url"}:{"tfs-port"}/tfs/{"project-collection-name"};{"project-name"}\",
            //            datasource,
            //            0);
            //    }
            //}

            if (generationContext.CustomData.ContainsKey(WORKITEM_TAG))
            {
                IEnumerable<int> workitems = generationContext.CustomData[WORKITEM_TAG] as IEnumerable<int>;
                foreach (int workitem in workitems)
                {
                    CodeDomHelper.AddAttribute(testMethod, WORKITEM_ATTR, workitem);
                }
            }

        }

        public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            IEnumerable<string> tags = scenarioCategories.ToList();

            IEnumerable<string> ownerTags = tags.Where(t => t.StartsWith(OWNER_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (ownerTags.Any())
            {
                string ownerName = ownerTags.Select(t => t.Substring(OWNER_TAG.Length).Trim('\"')).FirstOrDefault();
                if (!String.IsNullOrEmpty(ownerName))
                {
                    CodeDomHelper.AddAttribute(testMethod, OWNER_ATTR, ownerName);
                }
            }

            IEnumerable<string> workitemTags = tags.Where(t => t.StartsWith(WORKITEM_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            if (workitemTags.Any())
            {
                int temp;
                IEnumerable<string> workitemsAsStrings = workitemTags.Select(t => t.Substring(WORKITEM_TAG.Length).Trim('\"'));
                IEnumerable<int> workitems = workitemsAsStrings.Where(t => int.TryParse(t, out temp)).Select(t => int.Parse(t));
                foreach (int workitem in workitems)
                {
                    CodeDomHelper.AddAttribute(testMethod, WORKITEM_ATTR, workitem);
                }
            }

            // TODO implement datasource
            // add Datasource attribute if the scenario has the datasource key
            //IEnumerable<string> datasourceTags = tags.Where(t => t.StartsWith(DATASOURCE_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t);
            //if (datasourceTags.Any())
            //{
            //    string datasourceName = datasourceTags.Select(t => t.Substring(DATASOURCE_TAG.Length).Trim('\"')).FirstOrDefault();
            //    if (!String.IsNullOrEmpty(datasourceName))
            //    {
            //        CodeDomHelper.AddAttribute(
            //            testMethod,
            //            DATASOURCE_ATTR,
            //            "Microsoft.VisualStudio.TestTools.DataSource.TestCase",
            //            $@"http://{"tfs-server-url"}:{"tfs-port"}/tfs/{"project-collection-name"};{"project-name"}\",
            //            datasourceName,
            //            0);
            //    }
            //}

            CodeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, GetNonMSTestSpecificTags(tags));
        }


        // TODO this method should be deprecated
        private IEnumerable<string> GetNonMSTestSpecificTags(IEnumerable<string> tags)
        {
            return tags == null ? new string[0] : tags.Where(t =>
                (!t.StartsWith(OWNER_TAG, StringComparison.InvariantCultureIgnoreCase))
                && (!t.StartsWith(WORKITEM_TAG, StringComparison.InvariantCultureIgnoreCase))
                // TODO implement datasource
                //&& (!t.StartsWith(DATASOURCE_TAG, StringComparison.InvariantCultureIgnoreCase))
                )
                .Select(t => t);
        }



        protected CodeDomHelper CodeDomHelper { get; set; }

        public virtual UnitTestGeneratorTraits GetTraits()
        {
            return UnitTestGeneratorTraits.None;
        }

        private void SetProperty(CodeTypeMember codeTypeMember, string name, string value)
        {
            CodeDomHelper.AddAttribute(codeTypeMember, PROPERTY_ATTR, name, value);
        }

        public void SetTestClassIgnore(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, IGNORE_ATTR);
        }

        public virtual void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            // by default, doing nothing to the final generated code
        }


        public virtual void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestClassInitializeMethod.Attributes |= MemberAttributes.Static;
            generationContext.TestRunnerField.Attributes |= MemberAttributes.Static;

            generationContext.TestClassInitializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                TESTCONTEXT_TYPE, "testContext"));

            CodeDomHelper.AddAttribute(generationContext.TestClassInitializeMethod, TESTCLASSTEARUP_ATTR);
        }

        public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestClassCleanupMethod.Attributes |= MemberAttributes.Static;
            CodeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTCLASSTEARDOWN_ATTR);
        }


        public virtual void SetTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestInitializeMethod, TESTTEARUP_ATTR);
            FixTestRunOrderingIssue(generationContext);
        }

        protected virtual void FixTestRunOrderingIssue(TestClassGenerationContext generationContext)
        {
            //see https://github.com/techtalk/SpecFlow/issues/96

            //if (testRunner.FeatureContext != null && testRunner.FeatureContext.FeatureInfo.Title != "<current_feature_title>")
            //  <TestClass>.<TestClassInitialize>(null);

            var featureContextExpression = new CodePropertyReferenceExpression(
                new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name),
                "FeatureContext");
            generationContext.TestInitializeMethod.Statements.Add(
                new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(
                        new CodeBinaryOperatorExpression(
                            featureContextExpression,
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(null)),
                        CodeBinaryOperatorType.BooleanAnd,
                        new CodeBinaryOperatorExpression(
                            new CodePropertyReferenceExpression(
                                new CodePropertyReferenceExpression(
                                    featureContextExpression,
                                    "FeatureInfo"),
                                "Title"),
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(generationContext.Feature.Name))),
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression(
                                generationContext.Namespace.Name + "." + generationContext.TestClass.Name
                                ),
                            generationContext.TestClassInitializeMethod.Name,
                            new CodePrimitiveExpression(null)))));
        }

        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestCleanupMethod, TESTTEARDOWN_ATTR);
        }

        public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            CodeDomHelper.AddAttribute(testMethod, IGNORE_ATTR);
        }

        public virtual void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            // TODO does codedui support row tests?
            throw new NotSupportedException();
        }

        public virtual void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            // TODO does codedui support row tests?
            throw new NotSupportedException();
        }


        public virtual void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            if (!string.IsNullOrEmpty(exampleSetName))
            {
                SetProperty(testMethod, "ExampleSetName", exampleSetName);
            }

            if (!string.IsNullOrEmpty(variantName))
            {
                SetProperty(testMethod, "VariantName", variantName);
            }

            foreach (var pair in arguments)
            {
                SetProperty(testMethod, "Parameter:" + pair.Key, pair.Value);
            }
        }
    }
}