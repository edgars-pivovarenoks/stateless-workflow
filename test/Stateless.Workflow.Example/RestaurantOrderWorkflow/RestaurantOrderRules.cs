namespace Stateless.Workflow.Example
{
    public class RestaurantOrderRules : IRules
    {
        private bool _tablesAvailable;
        public RestaurantOrderRules(bool tablesAvailable)
        {
            _tablesAvailable = tablesAvailable;
        }
        public bool NoTablesAvailable()
        {
            return !TablesAvailable();
        }

        public bool TablesAvailable()
        {
            return _tablesAvailable;
        }
    }
}
