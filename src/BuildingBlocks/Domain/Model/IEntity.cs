namespace BuildingBlocks.Domain.Model;

public interface IEntity<out TId>
{
    TId Id { get; }
    DateTime LastModified { get; }
    bool IsDeleted { get; }
    int? ModifiedBy { get; }
}
