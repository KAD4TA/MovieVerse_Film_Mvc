



namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    
    public class LoginResponseDto : BaseResponse
    {
        
    }

    public class LoginSuccessResponseDto : LoginResponseDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }


        public LoginSuccessResponseDto(string fullName, string email)
        {
            FullName = fullName;
            Email = email;
            IsSuccess = true;
            Message = "Giriş Başarılı";
        }
    }

    public class LoginErrorResponseDto : LoginResponseDto
    {
        public LoginErrorResponseDto(string message)
        {
            IsSuccess = false;
            Message = message;
        }
    }
}