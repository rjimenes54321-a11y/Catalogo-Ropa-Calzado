(function () {
    "use strict";
    var tema = "light";
    try {
        tema = localStorage.getItem("temaCatalogo") === "dark" ? "dark" : "light";
    } catch (error) {
        // La interfaz sigue funcionando si el navegador bloquea el almacenamiento.
    }

    function aplicarTema() {
        document.documentElement.setAttribute("data-bs-theme", tema);
        document.querySelectorAll("[data-tema]").forEach(function (boton) {
            boton.textContent = tema === "dark" ? "Tema claro" : "Tema oscuro";
        });
    }

    aplicarTema();
    document.addEventListener("DOMContentLoaded", aplicarTema);

    document.addEventListener("click", function (evento) {
        var botonTema = evento.target.closest("[data-tema]");
        if (botonTema) {
            tema = tema === "dark" ? "light" : "dark";
            try { localStorage.setItem("temaCatalogo", tema); } catch (error) { /* Preferencia de sesión. */ }
            aplicarTema();
        }

        var botonClave = evento.target.closest("[data-clave]");
        if (botonClave) {
            var entrada = document.getElementById(botonClave.dataset.clave);
            entrada.type = entrada.type === "password" ? "text" : "password";
            botonClave.textContent = entrada.type === "text" ? "Ocultar contraseña" : "Mostrar contraseña";
        }
    });

    document.addEventListener("input", function (evento) {
        var formulario = evento.target.closest("[data-registro]");
        if (!formulario) return;
        var clave = formulario.elements.Clave;
        var confirmacion = formulario.elements.Confirmacion;
        var compleja = clave.value.length >= 8 && /\p{Lu}/u.test(clave.value)
            && /\p{Ll}/u.test(clave.value) && /\p{Nd}/u.test(clave.value);
        clave.setCustomValidity(clave.value && !compleja
            ? "Usa al menos 8 caracteres, con mayúscula, minúscula y número." : "");
        confirmacion.setCustomValidity(confirmacion.value && confirmacion.value !== clave.value
            ? "Las contraseñas deben coincidir." : "");
    });

    document.addEventListener("submit", function (evento) {
        var mensaje = evento.target.dataset.confirmar;
        if (mensaje && !window.confirm(mensaje)) evento.preventDefault();
    });
})();
