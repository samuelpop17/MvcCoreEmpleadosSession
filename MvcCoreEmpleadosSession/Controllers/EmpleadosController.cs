﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MvcCoreEmpleadosSession.Extensions;
using MvcCoreEmpleadosSession.Models;
using MvcCoreEmpleadosSession.Repositories;

namespace MvcCoreEmpleadosSession.Controllers
{
    public class EmpleadosController : Controller
    {
        private RepositoryEmpleados repo;
        private IMemoryCache memoryCache;

        public EmpleadosController
            (RepositoryEmpleados repo
            , IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.repo = repo;
        }

        public IActionResult EmpleadosFavoritos()
        {
            //PREGUNTAMOS SI TENEMOS FAVORITOS
            if (this.memoryCache.Get("FAVORITOS") == null)
            {
                ViewData["MENSAJE"] = "No tiene empleados favoritos";
                return View();
            }
            else
            {
                List<Empleado> favoritos =
                    this.memoryCache.Get<List<Empleado>>("FAVORITOS");
                return View(favoritos);
            }
        }

        public async Task<IActionResult> 
            SessionEmpleadosOk(int? idempleado, int? idfavorito)
        {
            if (idfavorito != null)
            {
                //COMO ALMACENAMOS EN CLIENTE CACHE, VAMOS A UTILIZAR
                //LA COLECCION DE EMPLEADOS DIRECTAMENTE
                List<Empleado> empleadosFavoritos;
                if (this.memoryCache.Get("FAVORITOS") == null)
                {
                    //CREAMOS NUESTRA COLECCION
                    empleadosFavoritos = new List<Empleado>();
                }
                else
                {
                    //RECUPERAMOS LOS EMPLEADOS QUE YA TENGAMOS EN CACHE
                    empleadosFavoritos =
                        this.memoryCache.Get<List<Empleado>>("FAVORITOS");
                }
                //BUSCAMOS AL EMPLEADO POR SU ID DE FAVORITO
                Empleado empleado = 
                    await this.repo.FindEmpleadoAsync(idfavorito.Value);
                empleadosFavoritos.Add(empleado);
                //ALMACENAMOS LOS NUEVOS DATOS DENTRO DE CACHE
                this.memoryCache.Set("FAVORITOS", empleadosFavoritos);
            }

            if (idempleado != null)
            {
                //ALMACENAREMOS LO MINIMO DEL OBJETO, UNA COLECCION INT
                List<int> idsEmpleados;
                if (HttpContext.Session.GetString("IDSEMPLEADOS") == null)
                {
                    //TODAVIA NO TENEMOS DATOS EN SESSION Y CREAMOS LA COLECCION
                    idsEmpleados = new List<int>();
                }
                else
                {
                    //RECUPERAMOS LA COLECCION DE Ids DE SESSION
                    idsEmpleados =
                        HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                }
                //ALMACENAMOS EL ID DEL EMPLEADO EN LA COLECCION
                idsEmpleados.Add(idempleado.Value);
                //ALMACENAMOS LA COLECCION EN SESSION CON LOS CAMBIOS REALIZADOS
                HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                ViewData["MENSAJE"] = "Empleados almacenados: "
                    + idsEmpleados.Count;
            }
            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public async Task<IActionResult> 
            EmpleadosAlmacenadosOk(int? ideliminar)
        {
            //DEBEMOS RECUPERAR LOS EMPLEADOS QUE ESTEN DENTRO 
            //DE LA COLECCION DE SESSION
            //RECUPERAMOS LOS DATOS DE LA COLECCION DE IDS DE SESSION
            List<int> idsEmpleados =
                HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
            if (idsEmpleados != null)
            {
                //DEBEMOS ELIMINAR DE SESSION
                if (ideliminar != null)
                {
                    //NOS HAN ENVIADO UN DATO PARA ELIMINARLO DE SESSION
                    idsEmpleados.Remove(ideliminar.Value);
                    //DEBEMOS PREGUNTAR SI YA NO TENEMOS EMPLEADOS EN LA 
                    //COLECCION
                    if (idsEmpleados.Count == 0)
                    {
                        //ELIMINAMOS DIRECTAMENTE SESSION CON SU KEY
                        HttpContext.Session.Remove("IDSEMPLEADOS");
                    }
                    else
                    {
                        //ALMACENAMOS DE NUEVO LOS DATOS EN SESSION
                        HttpContext.Session.SetObject("IDSEMPLEADOS", idsEmpleados);
                    }
                }
                List<Empleado> empleados = await
                    this.repo.GetEmpleadosSessionAsync(idsEmpleados);
                return View(empleados);
            }
            return View();
        }

        public async Task<IActionResult> SessionEmpleados
            (int? idEmpleado)
        {
            if (idEmpleado != null)
            {
                //BUSCAMOS AL EMPLEADO
                Empleado empleado = await
                    this.repo.FindEmpleadoAsync(idEmpleado.Value);
                //EN SESSION ALMACENAREMOS UN CONJUNTO DE EMPLEADOS
                List<Empleado> empleadosList;
                //DEBEMOS PREGUNTAR SI TENEMOS EMPLEADOS DENTRO DE 
                //SESSION
                if (HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS") != null)
                {
                    //SI YA TENEMOS EMPLEADOS, LOS RECUPERAMOS DE SESSION
                    empleadosList =
                        HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS");
                }
                else
                {
                    //SI NO TENEMOS EMPLEADOS, CREAMOS LA COLECCION PARA 
                    //ALMACENAR EL PRIMER EMPLEADO
                    empleadosList = new List<Empleado>();
                }
                //ALMACENAMOS EL NUEVO EMPLEADO EN SESSION
                empleadosList.Add(empleado);
                //GUARDAMOS LA COLECCION DENTRO DE SESSION
                HttpContext.Session.SetObject("EMPLEADOS", empleadosList);
                ViewData["MENSAJE"] = "Empleado " + empleado.Apellido
                    + " almacenado correctamente";
            }
            List<Empleado> empleados =
                await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult EmpleadosAlmacenados()
        {
            return View();
        }

        public async Task<IActionResult> SessionSalarios(int? salario)
        {
            if (salario != null)
            {
                //NECESITAMOS ALMACENAR EL SALARIO TOTAL 
                //DE TODOS LOS EMPLEADOS DE SESSION
                int sumaSalarial = 0;
                //PREGUNTAMOS SI YA TENEMOS LA SUMA EN SESSION
                if (HttpContext.Session.GetString("SUMASALARIAL") != null)
                {
                    //RECUPERAMOS LA SUMA SALARIAL DE SESSION
                    sumaSalarial = int.Parse
                        (HttpContext.Session.GetString("SUMASALARIAL"));
                }
                //REALIZAMOS LA SUMA DEL SALARIO RECIBIDO
                sumaSalarial += salario.Value;
                //ALMACENAMOS LA NUEVA SUMA SALARIAL EN SESSION
                HttpContext.Session.SetString("SUMASALARIAL", sumaSalarial.ToString());
                ViewData["MENSAJE"] = "Salario almacenado: " + salario.Value;
            }
            List<Empleado> empleados = await
                this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult SumaSalarios()
        {
            return View();
        }
    }
}
