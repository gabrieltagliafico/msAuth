using System.ComponentModel.DataAnnotations;

namespace MsAuth.Models
{
    public class PermissionCreateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del usuario debe ser un entero positivo.")]
        public int IdUser { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El identificador del servicio debe ser un entero positivo.")]
        public int IdService { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El identificador del endpoint debe ser un entero positivo.")]
        public int IdEndpoint { get; set; }
    }

    public class PermissionUpdateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del permiso debe ser un entero positivo.")]
        public int IdPermission { get; set; }

        [Required(ErrorMessage = "Debe indicar si el permiso está activo.")]
        public bool? IsActive { get; set; }
    }

    public class PermissionListByUserRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del usuario debe ser un entero positivo.")]
        public int IdUser { get; set; }
    }
}
