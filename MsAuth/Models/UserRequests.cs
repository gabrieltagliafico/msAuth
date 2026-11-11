using System.ComponentModel.DataAnnotations;

namespace MsAuth.Models
{
    public class UserCreateRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 150 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La API key es obligatoria.")]
        [StringLength(256, MinimumLength = 1, ErrorMessage = "La API key no puede superar 256 caracteres.")]
        public string ApiKey { get; set; } = string.Empty;
    }

    public class UserUpdateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del usuario debe ser un entero positivo.")]
        public int IdUser { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 150 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar si el usuario está activo.")]
        public bool? IsActive { get; set; }

        [StringLength(256, ErrorMessage = "La API key no puede superar 256 caracteres.")]
        public string? ApiKey { get; set; }
    }
}
