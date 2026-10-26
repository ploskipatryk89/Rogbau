namespace Rogbau.Api.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public string EntityName { get; }
        public object EntityId { get; }

        public NotFoundException(string entityName, object entityId)
            : base($"{entityName} o ID {entityId} nie został znaleziony")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}
