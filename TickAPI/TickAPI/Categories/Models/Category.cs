using TickAPI.Events.Models;

namespace TickAPI.Categories.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<Event> Events { get; set; }
}