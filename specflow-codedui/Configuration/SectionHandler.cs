namespace TvDijk.SpecFlowPlugin.Configuration
{
    using System;
    using System.Configuration;

    public class CodedUiGeneratorConfigSection : ConfigurationSection
    {
        public static CodedUiGeneratorConfigSection Instance
        {
            get
            {
                return (CodedUiGeneratorConfigSection)ConfigurationManager.GetSection("codedUiGenerator");
            }
        }

        [ConfigurationProperty("testbase", IsRequired = false)]
        public TestBaseConfigElement TestBase
        {
            get { return (TestBaseConfigElement)this["testbase"]; }
            set { this["testbase"] = value; }
        }

        [ConfigurationProperty("datasource", IsRequired = false)]
        public DataSourceConfigSection DataSource
        {
            get { return (DataSourceConfigSection)this["datasource"]; }
            set { this["datasource"] = value; }
        }
    }

    public class DataSourceConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("tfsServer", IsRequired = false)]
        public TfsServerConfigElement TfsServer
        {
            get { return (TfsServerConfigElement)this["tfsServer"]; }
            set { this["tfsServer"] = value; }
        }

        [ConfigurationProperty("tfsServer", IsRequired = false)]
        public TfsProjectCollectionConfigElement TfsProjectCollection
        {
            get { return (TfsProjectCollectionConfigElement)this["tfsProjectCollection"]; }
            set { this["tfsProjectCollection"] = value; }
        }

        [ConfigurationProperty("tfsProject", IsRequired = false)]
        public TfsProjectConfigElement TfsProject
        {
            get { return (TfsProjectConfigElement)this["tfsProject"]; }
            set { this["tfsProject"] = value; }
        }
    }

    public class TestBaseConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, DefaultValue = null)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (String)this["name"]; }
            set { this["name"] = value; }
        }
    }

    public class TfsServerConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("url", IsRequired = true, DefaultValue = null)]
        [StringValidator(MinLength = 1)]
        public string Url
        {
            get { return (String)this["url"]; }
            set { this["url"] = value; }
        }
    }

    public class TfsProjectCollectionConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, DefaultValue = null)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (String)this["name"]; }
            set { this["name"] = value; }
        }
    }
    public class TfsProjectConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, DefaultValue = null)]
        [StringValidator(MinLength = 1)]
        public string Name
        {
            get { return (String)this["name"]; }
            set { this["name"] = value; }
        }
    }
}
