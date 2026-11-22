using System.ComponentModel.DataAnnotations;

namespace CatalogoRopaCalzado.ViewModels
{
    public class InicioSesion
    {
        [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "Escribe un correo válido."), StringLength(254)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Escribe tu contraseña."), StringLength(128)]
        public string Clave { get; set; }
    }

    public class RegistroCuenta : InicioSesion
    {
        [Required(ErrorMessage = "Escribe el nombre completo."), StringLength(120)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Confirma la contraseña.")]
        [Compare(nameof(Clave), ErrorMessage = "Las contraseñas deben coincidir.")]
        public string Confirmacion { get; set; }

        public string Rol { get; set; }
    }
}
