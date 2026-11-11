using System.ComponentModel.DataAnnotations;

namespace MsAuth.Models
{
    public class RoleCreateRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;
    }

    public class RoleUpdateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del rol debe ser un entero positivo.")]
        public int IdRole { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar si el rol está activo.")]
        public bool? IsActive { get; set; }
    }

    public class UserRoleAssignRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del usuario debe ser un entero positivo.")]
        public int IdUser { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El identificador del rol debe ser un entero positivo.")]
        public int IdRole { get; set; }
    }

    public class UserRoleUpdateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador de la asignación debe ser un entero positivo.")]
        public int IdUserRole { get; set; }

        [Required(ErrorMessage = "Debe indicar si la asignación está activa.")]
        public bool? IsActive { get; set; }
    }

    public class UserRoleListByUserRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del usuario debe ser un entero positivo.")]
        public int IdUser { get; set; }
    }
}
