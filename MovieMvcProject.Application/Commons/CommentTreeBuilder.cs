using MovieMvcProject.Application.DTOs.ResponseDto;

namespace MovieMvcProject.Application.Commons
{
    public static class CommentTreeBuilder
    {
        public static List<CommentDtoResponse> BuildTree(List<CommentDtoResponse> flatList)
        {
            if (flatList == null || !flatList.Any()) return new List<CommentDtoResponse>();

         
            var distinctList = flatList.GroupBy(x => x.CommentId)
                                       .Select(g => g.First())
                                       .ToList();

            var lookup = distinctList.ToDictionary(x => x.CommentId);
            var roots = new List<CommentDtoResponse>();

            foreach (var comment in distinctList)
            {
               
                comment.Replies ??= new List<CommentDtoResponse>();

                if (comment.ParentId == null || !lookup.ContainsKey(comment.ParentId.Value))
                {
                    roots.Add(comment);
                }
                else
                {
                    var parent = lookup[comment.ParentId.Value];
                    parent.Replies ??= new List<CommentDtoResponse>();
                    parent.Replies.Add(comment);
                }
            }

            return roots.OrderByDescending(x => x.CreatedAt).ToList();
        }
    }
}