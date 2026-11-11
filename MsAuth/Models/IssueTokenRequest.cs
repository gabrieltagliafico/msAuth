using System.ComponentModel.DataAnnotations;

namespace MsAuth.Models
{
    public class IssueTokenRequest : IValidatableObject
    {
        [Required(ErrorMessage = "El identificador del usuario es obligatorio.")]
        public Guid UidUser { get; set; }

        [Required(ErrorMessage = "La API key es obligatoria.")]
        [StringLength(256, ErrorMessage = "La API key no puede superar 256 caracteres.")]
        public string ApiKey { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "El identificador del servicio debe ser un entero positivo.")]
        public int IdService { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UidUser == Guid.Empty)
            {
                yield return new ValidationResult(
                    "El identificador del usuario es obligatorio.",
                    new[] { nameof(UidUser) });
            }
        }
    }
}
