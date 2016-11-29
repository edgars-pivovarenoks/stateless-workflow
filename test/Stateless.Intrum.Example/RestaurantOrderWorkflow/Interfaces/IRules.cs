namespace Stateless.Workflow.Example
{
    public interface IRules
    {
        bool NoTablesAvailable();
        bool TablesAvailable();
    }
}
