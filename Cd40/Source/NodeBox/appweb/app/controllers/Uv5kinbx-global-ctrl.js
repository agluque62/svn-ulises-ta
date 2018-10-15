/** */
angular.module("Uv5kinbx")
.controller("uv5kiGlobalCtrl", function ($scope, $interval, $location, $translate, $serv, $lserv) {
    /** Inicializacion */
    var ctrl = this;
    ctrl.pagina = 0;
    /** Lista de Incidencias */
    ctrl.listainci = [];
    ctrl.HashCode = 0;
    ctrl.timer = 0;

    getInci();


    ctrl.user="agl1";
    ctrl.date = (new Date()).toLocaleDateString();
    ctrl.hora = (new Date()).toLocaleTimeString();
    $location.path("/");

    /** */
    ctrl.appver = app_version;

    /** */
    ctrl.decodeHtml = function (html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    }

    /** Funciones o servicios */
    function getInci() {
        $serv.inci_get().then(function (response) {
            if (response.status == 200 && (typeof response) == 'object') {
                if (ctrl.HashCode != response.data.hash) {
                    ctrl.listainci = response.data.li;
                    ctrl.HashCode = response.data.hash;
                }
                // console.log(ctrl.listainci);
                /** */
                if (userLang != response.data.lang) {
                    userLang = response.data.lang;
                    if (userLang.indexOf("en") == 0)
                        $translate.use('en_US');
                    else if (userLang.indexOf("fr") == 0)
                        $translate.use('fr_FR');
                    else
                        $translate.use('es_ES');
                }
            }
            else {
                /** El servidor me devuelve errores... */
                // window.open(routeForDisconnect, "_self");
            }
        }
        , function (response) {
            // Error. No puedo conectarme al servidor.
            // window.open(routeForDisconnect, "_self");
        });
    }

    /** Funcion Periodica del controlador */
    var timer = $interval(function () {

        ctrl.date = moment().format('ll');
		ctrl.hora = moment().format('LTS');

		ctrl.timer++;

		if ((ctrl.timer % 5) == 0) {
		    getInci();
		}

	}, 1000);

    /** */
    $scope.$on('$viewContentLoaded', function () {
        /** Alertify */
        alertify.defaults.transition = 'zoom';
        alertify.defaults.glossary = {
            title: $lserv.translate("ULISES V 5000 I. Nodebox"),
            ok: $lserv.translate("Aceptar"),
            cancel: $lserv.translate("Cancelar")
        };

    });

    /** Salida del Controlador. Borrado de Variables */
	$scope.$on("$destroy", function () {
	    $interval.cancel(timer);
	});
	
});


