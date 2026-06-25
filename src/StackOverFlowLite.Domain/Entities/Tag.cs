using StackOverflowLite.Domain.Common;

namespace StackOverflowLite.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
}
