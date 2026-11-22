using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CatalogoRopaCalzado.Data
{
    public static class Claves
    {
        private const int Iteraciones = 600000;

        public static bool EsValida(string clave)
        {
            return clave != null && Regex.IsMatch(clave, @"\A(?=[\s\S]{8,128}\z)(?=[\s\S]*\p{Lu})(?=[\s\S]*\p{Ll})(?=[\s\S]*\p{Nd})[\s\S]+\z");
        }

        public static string Crear(string clave)
        {
            byte[] sal = new byte[16];
            using (var generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(sal);
            }
            return Convert.ToBase64String(sal) + "." + Convert.ToBase64String(Derivar(clave, sal));
        }

        public static bool Verificar(string clave, string hash)
        {
            var partes = hash.Split('.');
            var esperado = Convert.FromBase64String(partes[1]);
            var generado = Derivar(clave, Convert.FromBase64String(partes[0]));
            int diferencia = 0;
            for (int i = 0; i < generado.Length; i++)
            {
                diferencia |= esperado[i] ^ generado[i];
            }
            return diferencia == 0;
        }

        private static byte[] Derivar(string clave, byte[] sal)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(clave, sal, Iteraciones, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32);
            }
        }
    }
}
