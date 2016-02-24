using TechTalk.SpecFlow.Infrastructure;

[assembly: GeneratorPlugin(typeof(TvDijk.SpecFlowPlugin.CodedUIGeneratorPlugin))]

namespace TvDijk.SpecFlowPlugin
{
    using BoDi;
    using TechTalk.SpecFlow.Generator.Configuration;
    using TechTalk.SpecFlow.Generator.Plugins;
    using TechTalk.SpecFlow.Generator.UnitTestProvider;

    /// <summary>
    /// The CodedUI generator plugin.
    /// </summary>
    public class CodedUIGeneratorPlugin : IGeneratorPlugin
    {
        /// <summary>
        /// The register dependencies.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public void RegisterDependencies(ObjectContainer container)
        {
        }

        /// <summary>
        /// The register customizations.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="generatorConfiguration">
        /// The generator configuration.
        /// </param>
        public void RegisterCustomizations(ObjectContainer container, SpecFlowProjectConfiguration generatorConfiguration)
        {
            container.RegisterTypeAs<CodedUIGeneratorProvider, IUnitTestGeneratorProvider>();
        }

        /// <summary>
        /// The register configuration defaults.
        /// </summary>
        /// <param name="specFlowConfiguration">
        /// The spec flow configuration.
        /// </param>
        public void RegisterConfigurationDefaults(SpecFlowProjectConfiguration specFlowConfiguration)
        {
        }
    }
}
