using Autofac;
using SpecFlow.Autofac;
using Stateless.Workflow.Example;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;

public static class ContainerBuilderHelper
{
    [ScenarioDependencies]
    public static ContainerBuilder CreateContainerBuilder()
    {
        var builder = new ContainerBuilder();

        var asm = Assembly.GetExecutingAssembly();

        builder.RegisterTypes(
            asm.GetTypes()
                .Where(t => t.CustomAttributes
                    .Any(a => a.AttributeType.IsAssignableFrom(typeof(BindingAttribute)))).ToArray())
            .SingleInstance()
            .PropertiesAutowired();

        builder.RegisterType<WorkflowProviderAutofac>()
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<ActorProviderManual>()
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();

        builder.RegisterType<RestaurantOrderWorkflowDefault>()
            .AsImplementedInterfaces()
            .AsSelf()
            .Named(RestaurantOrderWorkflowDefault.VersionKeyString, typeof(IRestaurantOrderWorkflow))
            .InstancePerLifetimeScope();

        /*
        builder.RegisterType<ApplicationTransitionsRepository>()
            .AsImplementedInterfaces();
        */

        // RegisterDependencies(builder);

        return builder;
    }
}

