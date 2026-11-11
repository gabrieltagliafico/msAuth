using System.ComponentModel.DataAnnotations;

namespace MsAuth.Models
{
    public class ServiceCreateRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 150 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;
    }

    public class ServiceUpdateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del servicio debe ser un entero positivo.")]
        public int IdService { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 150 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar si el servicio está activo.")]
        public bool? IsActive { get; set; }
    }
}
