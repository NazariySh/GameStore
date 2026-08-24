namespace Gamestore.BLL.DTOs.Games.Comments;

public class CommentTreeDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Body { get; set; }

    public ICollection<CommentTreeDto> ChildComments { get; set; } = [];
}