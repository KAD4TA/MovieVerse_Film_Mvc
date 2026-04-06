namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class UserListResponseDto : BaseResponse
    {
        
        public List<UserResponseDto> Users { get; set; } = new List<UserResponseDto>();

        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
