namespace GrowBox.Abstractions;

public interface IEntityBase : IEntityId, IEntityUpdated, IEntityCreated
{
}

public interface IEntityId
{
    public Guid Id { get; set; }
}

public interface IEntityUpdated
{
    public DateTime Updated { get; set; }
}

public interface IEntityCreated
{
    public DateTime Created { get; set; }
}