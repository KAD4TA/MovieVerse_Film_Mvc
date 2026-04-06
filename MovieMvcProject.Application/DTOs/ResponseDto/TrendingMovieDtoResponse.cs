using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMvcProject.Application.DTOs.ResponseDto
{
    public class TrendingMovieDtoResponse : MovieDtoResponse
    {
        public int WeeklyViewCount { get; set; }
    }
}
