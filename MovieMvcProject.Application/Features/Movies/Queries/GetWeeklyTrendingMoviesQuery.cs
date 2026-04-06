using MediatR;
using MovieMvcProject.Application.DTOs.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMvcProject.Application.Features.Movies.Queries
{
    public class GetWeeklyTrendingMoviesQuery : IRequest<List<TrendingMovieDtoResponse>>
    {
        public string LangCode { get; set; }
        public int Limit { get; set; }

        public GetWeeklyTrendingMoviesQuery(string langCode, int limit = 10)
        {
            LangCode = langCode;
            Limit = limit;
        }
    }
}
