using System.ComponentModel.DataAnnotations;

namespace MsAuth.Models
{
    public class EndpointCreateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del servicio debe ser un entero positivo.")]
        public int IdService { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 150 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El método HTTP es obligatorio.")]
        [RegularExpression("^(?i)(\\*|GET|POST|PUT|PATCH|DELETE)$", ErrorMessage = "El método HTTP no es válido.")]
        public string HttpMethod { get; set; } = "*";

        [Required(ErrorMessage = "La ruta es obligatoria.")]
        [StringLength(300, ErrorMessage = "La ruta no puede superar 300 caracteres.")]
        [RegularExpression(@"^\s*(/\S.*|[A-Za-z][A-Za-z0-9]*(\/[A-Za-z][A-Za-z0-9]*)?)\s*$", ErrorMessage = "La ruta debe ser un controlador (CreditCards), un controlador y acción (Cards/ListAvailableCards) o una ruta que empiece con /.")]
        public string Route { get; set; } = string.Empty;
    }

    public class EndpointUpdateRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del endpoint debe ser un entero positivo.")]
        public int IdEndpoint { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 150 caracteres.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El método HTTP es obligatorio.")]
        [RegularExpression("^(?i)(\\*|GET|POST|PUT|PATCH|DELETE)$", ErrorMessage = "El método HTTP no es válido.")]
        public string HttpMethod { get; set; } = "*";

        [Required(ErrorMessage = "La ruta es obligatoria.")]
        [StringLength(300, ErrorMessage = "La ruta no puede superar 300 caracteres.")]
        [RegularExpression(@"^\s*(/\S.*|[A-Za-z][A-Za-z0-9]*(\/[A-Za-z][A-Za-z0-9]*)?)\s*$", ErrorMessage = "La ruta debe ser un controlador (CreditCards), un controlador y acción (Cards/ListAvailableCards) o una ruta que empiece con /.")]
        public string Route { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar si el endpoint está activo.")]
        public bool? IsActive { get; set; }
    }

    public class EndpointListByServiceRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador del servicio debe ser un entero positivo.")]
        public int IdService { get; set; }
    }
}
