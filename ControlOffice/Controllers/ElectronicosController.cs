﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entidades;
using Modelos;
using Util;
using ControlOffice.CustomAttributes;

namespace ControlOffice.Controllers
{
    public class ElectronicosController : Controller
    {
        Usuarios usuario;
        ElectronicosModel em = new ElectronicosModel();
        SolicitudesModel sm = new SolicitudesModel();

        /// <summary>
        /// Constructor del controlador
        /// </summary>
        public ElectronicosController()
        {
            UsuarioModel usuarioModel = new UsuarioModel();
            //Obtenemos usuario actual
            this.usuario = usuarioModel.ObtenerUsuario(ManejadorDeSesiones.ObtenerUsuarioEnSesion());            
        }

        //
        // GET: /Electronicos/
        [Protegido]
        public ActionResult Index()
        {
            ViewBag.NombreUsuario = usuario != null ? usuario.Nombre : "Usuario";
            return View();
        }

        [ProtegidoVista]
        public PartialViewResult Registro()
        {
            ViewBag.TiposElectronicos = em.ObtenerTiposElectronicos();
            ViewBag.marcasElectronicos = em.ObtenerMarcas();
            return PartialView();
        }

        [ProtegidoVista]
        public PartialViewResult Inventario()
        {
            return PartialView();
        }

        [ProtegidoVista]
        public PartialViewResult InventarioSolicutides()
        {
            return PartialView();
        }

        [Protegido]
        public JsonResult RegistrarAparato(int utilizado, int tipo = 0, string cantidad = "", int marca = 0, string serie = null, DateTime FechaUso = new DateTime(),
            string reemplazo = "", int unidadReemplazo = -1)
        {
            string totalErrores="";
            int tiempoReemplazo=0;
            if (tipo == 0)
            {
                totalErrores += "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Debes seleccionar un tipo de electrónico <br />";
            }
            if (cantidad.Length <= 0)
            {
                totalErrores += "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Debes ingresar una cantidad <br />";
            }
            else
            {
                try
                {
                    Convert.ToInt32(cantidad);
                }
                catch
                {
                    totalErrores += "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Debes ingresar una cantidad valida <br />";
                }
            }
            if (reemplazo.Length > 0)
            {                
                try
                {
                    Convert.ToInt32(cantidad);
                }
                catch
                {
                    totalErrores += "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Debes ingresar un número valido como tiempo de reemplazo <br />";
                }
            }
            if (utilizado == 1) { FechaUso = DateTime.Now; }
            
            if (totalErrores.Length > 0)
            {
                return Json(new { response = false, mensaje = totalErrores });
            }
            else
            {//guarda la informacion
               // if (ManejadorDeSesiones.ExisteUsuarioEnSesion())
                {
                    if (reemplazo.Length > 0)
                    {
                        tiempoReemplazo = Convert.ToInt32(reemplazo);
                        if (unidadReemplazo == 1) { tiempoReemplazo *= 30; }
                        else if (unidadReemplazo == 2) { tiempoReemplazo *= 365; }
                    }
                    return Json(em.RegistrarElectronico(tipo, Convert.ToInt32(cantidad), marca, serie, FechaUso, utilizado, tiempoReemplazo, usuario.Usuario));
                }
                /*else
                {//Debe iniciar sesion, tal vez se ingreso aqui con una copia de la pagina o en cache                    
                    RespuestaModel respuesta = new RespuestaModel();
                    respuesta.SetRespuesta(false,"Primero debes iniciar sesión");
                    respuesta.href = "controloffice/index";
                    return Json(respuesta);
                }*/
            }
            

           
        }

        [ProtegidoVista]
        public JsonResult RegistrarSolicitud(int envio,string destino="",string descripcion="",DateTime fechaEnvio = new DateTime(), string horaEnvio="",
             string crearFolio="",string folio="", string imagen="" )
        {
            try
            {
                string totalErrores = "";
                if (destino.Length <= 0)
                {
                    totalErrores += "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Debes indicar para quien es esta solicitud. <br />";
                }
                if (descripcion.Length <= 0)
                {
                    totalErrores += "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Ingresa una descripción para esta solicitud. <br />";
                }
                if (envio == 1) { fechaEnvio = DateTime.Now; }
                else if (envio == 2) {
                    if (horaEnvio.Length <= 0) { horaEnvio = "00:00"; } 
                    fechaEnvio = new DateTime(fechaEnvio.Year, fechaEnvio.Month, fechaEnvio.Day, Convert.ToInt32(horaEnvio.Substring(0, 2)), Convert.ToInt32(horaEnvio.Substring(3, 2)), 0);
                }

                if (crearFolio == "on")
                {
                    folio = DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                    int f = sm.ObtenerUltimoFolio();
                    int conta = 20;//intentos de asignacion de folio
                    while (conta > 0)
                    {
                        if (sm.ExisteSolicitud(folio + f)) { f++; }
                        else { folio += f; conta = -1; }
                        conta--;
                    }
                    if (conta == 0)//fue imposible asignar el folio
                    {
                        totalErrores += "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Error: Imposible asignar el folio automaticamente; Por favor, insegresa el folio manualmente <br />";
                    }
                }
                if (totalErrores.Length > 0)
                {
                    return Json(new { response = false, mensaje = totalErrores });
                }
                else
                {
                    return Json( em.RegistrarSolicitud(destino,descripcion,fechaEnvio,folio,imagen,envio,usuario.Usuario) );
                }
            }
            catch (Exception ex)
            {
                return Json(new { response = false, mensaje = "Error inesperado del sistema, verifique la información. "+ex.Message});
            }
        }

        [ProtegidoVista]
        public JsonResult RegistrarMarca(string nuevaMarca = "")
        {

            if (nuevaMarca.Length <= 0)
            {
                return Json(new { Response = false, mensaje = "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Ingresa la marca que quieres registrar. <br />" });
            }
            else
            {
                RespuestaModel respuesta = em.RegistrarMarca(nuevaMarca);
                respuesta.funcion= "actualizarMarcas()";
                return Json(respuesta);
            }
        }

        [Protegido]
        public JsonResult RegistrarTipoElectronico(string nuevoTipo = "")
        {

            if (nuevoTipo.Length <= 0)
            {
                return Json(new { Response = false, mensaje = "<span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'> </span> Ingresa el nombre del nuevo tipo de electrónico que quieres registrar. <br />" });
            }
            else
            {
                RespuestaModel respuesta = em.RegistrarTipoElectronico(nuevoTipo);
                respuesta.funcion= "actualizarTipos()";
                return Json(respuesta);
            }
        }

        public JsonResult ObtenerMarcas()
        {
            string opciones = "<option value='0'>Selecciona una marca</option>";
            List<Marca_electronicos> marcas = em.ObtenerMarcas();
            foreach(Marca_electronicos m in marcas )
            {
                opciones+="<option value='"+m.Id_marca+"'>"+m.Marca+"</option>";
            }
            opciones = "<select class='form-control' id='listaMarcas' name='marca' style='border-color:white'>"+opciones+"</select>";
            
            return Json(new { response = true,mensaje=opciones});
        }

        public JsonResult ObtenerTipos()
        {
            string opciones = "<option value='0'>Seleccionar</option>";
            List<Tipo_electronico> tipos = em.ObtenerTipos();
            foreach (Tipo_electronico t in tipos)
            {
                opciones += "<option value='" + t.Id_tipo_electronico + "'>" + t.Nombre + "</option>";
            }
            opciones = "<select class='form-control' id='FORM-aparato_LB-tipo' name='tipo' style='border-color:white'>" + opciones + "</select>";

            return Json(new { response = true, mensaje = opciones });
        }

        [ProtegidoVista]
        public JsonResult listaElectronicos(JqGrid jq)
        {
            return Json(em.ObtenerTodosLosElectronicos(jq), JsonRequestBehavior.AllowGet);
        }

        [ProtegidoVista]
        public JsonResult listaSolicitudElectronicos(JqGrid jq)
        {
            return Json(em.ObtenerSolicitudesElectronicos(jq), JsonRequestBehavior.AllowGet);
        }

        [ProtegidoVista]
        [SoloAdministrador]
        public JsonResult eliminar(string id)
        {
            if (usuario.Administrador)
            {
                return Json(em.eliminar(id));
            }
            else
            {
                RespuestaModel respuesta = new RespuestaModel();
                respuesta.SetRespuesta(false);
                respuesta.alerta="No tienes los permisos necesarios para realizar esta acción";
                return Json(respuesta);
            }
        }

        [ProtegidoVista]
        [SoloAdministrador]
        public JsonResult eliminarSolicitud(string id)
        {
            if(usuario.Administrador)
            {
                return Json(sm.eliminarSolicitud(id));
            }
            else
            {
                RespuestaModel respuesta = new RespuestaModel();
                respuesta.SetRespuesta(false);
                respuesta.alerta="No tienes los permisos necesarios para realizar esta acción";
                return Json(respuesta);
            }
        }

    }
}
