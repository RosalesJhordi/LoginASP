using Microsoft.AspNetCore.Mvc;
using System.Data;
using Login.Models;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;

namespace Login.Controllers
{
    public class AccesoController : Controller
    {
        static string cone_tring = "Server= DESKTOP-2QR1BR7\\SQLEXPRESS;DataBase= BD_IE_CM;Integrated Security=true";

        //GET
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(Usuario oUsuario)
        {
            bool Registrado;
            string Mensaje;

            if (oUsuario.Contrasena == oUsuario.Contrasena)
            {
                oUsuario.Contrasena = HashPwd(oUsuario.Contrasena);
            }
            else
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }
            using (SqlConnection cn = new SqlConnection(cone_tring))
            {
                SqlCommand cmd = new SqlCommand("sp_registrar", cn);
                cmd.Parameters.AddWithValue("nm", oUsuario.Nombres);
                cmd.Parameters.AddWithValue("ape", oUsuario.Apellidos);
                cmd.Parameters.AddWithValue("cor", oUsuario.Correo);
                cmd.Parameters.AddWithValue("pwd", oUsuario.Contrasena);

                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();

                cmd.ExecuteNonQuery();

                Registrado = Convert.ToBoolean(cmd.Parameters["Resistrado"].Value);
                Mensaje = cmd.Parameters["Mensaje"].Value.ToString();
            }
            ViewData["Mensaje"] = Mensaje;

            if (Registrado)
            {
                return RedirectToAction("Login","Acceso");
            }
            else
            {
                return View();
            }

        }
        [HttpPost]
        public IActionResult Login(Usuario oUsuario)
        {
            oUsuario.Contrasena = HashPwd(oUsuario.Contrasena);

            using (SqlConnection cn = new SqlConnection(cone_tring))
            {
                SqlCommand cmd = new SqlCommand("sp_login", cn);
                cmd.Parameters.AddWithValue("cor", oUsuario.Correo);
                cmd.Parameters.AddWithValue("pwd", oUsuario.Contrasena);

                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();

                oUsuario.Id = Convert.ToInt32( cmd.ExecuteScalar()); 
            }
            if(oUsuario.Id != 0)
            {
                Session["usuario"] = oUsuario;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }
        }
        public static string HashPwd(string texto)
        {
            StringBuilder sb = new StringBuilder();
            using(SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
