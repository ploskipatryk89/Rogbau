namespace Rogbau.Api.Common.Exceptions.Equipments
{
    public class NameAlreadyExistsException : Exception
    {
        public string Name { get; }

        public NameAlreadyExistsException(string name)
            :base($"Sprzęt o nazwie {name} już istnieje")
        {
            Name = name;
        }
    }
}
