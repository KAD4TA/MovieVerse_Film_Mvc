using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMvcProject.Application.DTOs.RequestDto
{
    public class ToggleSliderRequest
    {
        public Guid MovieId { get; set; }
        public bool IsOnSlider { get; set; }
    }
}
