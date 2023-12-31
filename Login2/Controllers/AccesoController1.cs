﻿using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using Login2.Models1;

namespace Login2.Controllers
{
    public class AccesoController1 : Controller
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
		public ActionResult Registrar(Usuario oUsuario)
		{
			bool registrado;
			string mensaje;
			if (oUsuario.Contrasena == oUsuario.Confirmar)
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
				cmd.Parameters.Add("Registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
				cmd.CommandType = CommandType.StoredProcedure;

				cn.Open();

				cmd.ExecuteNonQuery();

				registrado = Convert.ToBoolean(cmd.Parameters["Registrado"].Value);
				mensaje = cmd.Parameters["Mensaje"].Value.ToString();


			}

			ViewData["Mensaje"] = mensaje;

			if (registrado)
			{
				return RedirectToAction("Login", "Acceso");
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

                oUsuario.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            if (oUsuario.Id != 0)
            {
                HttpContext.Session.SetString("usuario",oUsuario.Nombres);
                return RedirectToAction("Index", "Home");
            }
            else
            {
				ViewBag.SuccessMessage = "Error: el usuario no existe";
				return View();
            }
        }
        public static string HashPwd(string texto)
        {
            StringBuilder sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
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