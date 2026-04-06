namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class RoleListResponseDto : BaseResponse
    {
        
        public List<RoleResponseDto> Roles { get; set; } = new List<RoleResponseDto>();
    }
}
