angular.module("Uv5kinbx")
    .controller("uv5kiRadioCtrl", function ($scope, $interval, $serv, $lserv) {
        /** Inicializacion */
        var ctrl = this;
        ctrl.translate = (str) => { return $lserv.translate(str); }

        var session_stdcodes = { Desconectado: 0, Conectado: 1, Deshabilitado: 2 };
        var session_types = { RX: "Rx", TX: "Tx", RXTX: "RxTx" };
        var equ_types = { Main: 0, Reserva: 1 };
        var led_std = { Off: 0, On1: 1, On2: 2 };

        var frec_stdcodes = { NoDisponible: 0, Disponible: 1, Degradada: 2 };
        var frec_tipos = { Normal: 0, UnoMasUno: 1, FD: 2, EM: 3 };
        var frec_prio = { Normal: 0, Emergencia: 1 };
        var frec_cclimax = { Realtivo: 0, Absoluto: 1 };

        ctrl.pagina = 0;
        ctrl.leds = led_std.Off;
        ctrl.sessions = [];
        ctrl.gestormn = [];
        ctrl.gestorhf = [];

        /** Tabla de Presentacion V 2.5.5 y posteriores... */
        ctrl.frecs = [];
        ctrl.mnman = [];
        ctrl.site_select = "";
        ctrl.vhf_mode_select = -1;
        ctrl.uhf_mode_select = -1;

        ctrl.lsrv = $lserv;
        ctrl.StringCut = StringCut;
        //** */
        ctrl.Pagina = function (pg) {
            if (pg == 0)
                rdSessionsGet();
            else {
                rdGestormnGet();
            }
            ctrl.pagina = pg;
        };

        /** Servicios Pagina de Sesiones*/
        //** Version 1 */
        ctrl.colorEstadoFrecuencia = function (std) {
            return (std == frec_stdcodes.NoDisponible ? "freqNotAvailable" :
                std == frec_stdcodes.Disponible ? "freqAvailable" :
                    std == frec_stdcodes.Degradada ? "freqDegraded" : "text-danger bg-danger");
        };
        ctrl.textTipoPrio = function (tp, pr) {
            var txtTipo = tp == frec_tipos.Normal ? $lserv.translate("Simple") :
                tp == frec_tipos.UnoMasUno ? $lserv.translate("Dual") :
                    tp == frec_tipos.FD ? $lserv.translate("FD") :
                        tp == frec_tipos.EM ? $lserv.translate("ME") : $lserv.translate("ERR");
            var txtPrio = pr == frec_prio.Normal ? $lserv.translate("Normal") :
                pr == frec_prio.Emergencia ? $lserv.translate("Emergencia") : $lserv.translate("Error");
            //return txtTipo + "/" + txtPrio;
            return { txtTipo, txtPrio };
        };
        ctrl.colorBotonSesion = function (std, dis) {
            return dis == true ? "btn-default" :
                std == session_stdcodes.Desconectado ? "btn-danger" :
                    std == session_stdcodes.Conectado ? "btn-success" :
                        std == session_stdcodes.Deshabilitado ? "btn-default" : "btn-danger";
        };
        //ctrl.colorEstadoSesion = (std, dis) => {
        //    return dis == true ? "text-muted" :
        //        std == session_stdcodes.Desconectado ? "bg-warning text-danger" :
        //            std == session_stdcodes.Conectado ? "text-info" :
        //                std == session_stdcodes.Deshabilitado ? "text-muted" : "text-danger bg-danger";
        //}
        ctrl.txtCClimax = function (md) {
            return md == frec_cclimax.Absoluto ? "A" :
                md == frec_cclimax.Realtivo ? "R" : "?";
        };
		ctrl.txtRtpPort = function (ses, port) {
			var ret = ses.std == session_stdcodes.Conectado ? port : "";
			return ret;
		};
        ctrl.enableOnFD = function (tp) {
            return { FD: tp == frec_tipos.FD ? true : false, UnoMasUno: true };
        };
        ctrl.showOnTx = function (tp) {
            return (tp == session_types.TX || tp == session_types.RXTX);
        };
        ctrl.showOnRx = function (tp) {
            return (tp == session_types.RX || tp == session_types.RXTX);
        };
        ctrl.txtPestana = function (pes) {
            if (pes == 0) return $lserv.translate("Frecuencias");
            if (pes == 1) return $lserv.translate("Gestor M+N");
            if (pes == 2) return $lserv.translate("Gestor M+N (UHF)");
            if (pes == 3) return $lserv.translate("Transmisores HF");
            if (pes == 4) return $lserv.translate("Radio 1+1");
            return "Estado Erroneo";
        };
        ctrl.txtFrecAndType = function (fr, tp) {
            var txtTipo = tp == frec_tipos.Normal ? $lserv.translate("Normal") :
                tp == frec_tipos.UnoMasUno ? $lserv.translate("1+1") :
                    tp == frec_tipos.FD ? $lserv.translate("FD") :
                        tp == frec_tipos.EM ? $lserv.translate("ME") : $lserv.translate("ERR");
            if (fr.length == 0)
                return fr;
            return (fr + " (" + txtTipo + ")");
        };

        /** Version 0 */
        //** */
        ctrl.txtEstado = function (std) {
            return std == session_stdcodes.Desconectado ? $lserv.translate("Desconectado") :
                std == session_stdcodes.Conectado ? $lserv.translate("Conectado") : $lserv.translate("Estado Erroneo");
        };
        //** */
        ctrl.colorEstado = function (std) {
            return std == session_stdcodes.Desconectado ? "text-danger" :
                std == session_stdcodes.Conectado ? "text-info" :
                    std == session_stdcodes.Deshabilitado ? "text-warning" : "text-danger";
        };
        //** */
        ctrl.txtTipo = function (type) {
            return type == session_types.RX ? $lserv.translate("Rx") :
                type == session_types.TX ? $lserv.translate("Tx") :
                    type == session_types.RXTX ? $lserv.translate("TxRx") : "??";
        };
        //** Servicios Pagina del Gestor */
        ctrl.txtTipoEquipo = function (eq) {
            var type = eq.tip == equ_types.Main ? $lserv.translate("M") :
                eq.tip == equ_types.Reserva ? $lserv.translate("S") : $lserv.translate("ERROR");
            type += eq.tip == equ_types.Main ? ("-" + eq.prio) : "";
            return type;
        };
        //** */
        ctrl.gearColorEstado = function (std, sip) {
            return std == 0 ? "text-warning" :                              // Estado Inicial
                std == 1 ? "text-info" :                                 // Ok. sin Asignar.
                    std == 5 ? "text-info" :                                    // Ok.  Asignaci�n en progreso = disponible
                        std == 2 ? (sip == "3" ? "text-success" : "text-danger") :  // OK. Asignado. SIP Conectado...
                            std == 3 ? "text-danger" :                                  // En Fallo....
                                std == 4 ? "text-muted" : "text-danger";                    // Ok. Deshabilitado....
        };
        //** */
        ctrl.gearEnableDisableShow = function (std) {
            return (std != 0);
        };
        //** */
        ctrl.gearTextoEstadoEquipo = function (std) {
            return std == 0 ? $lserv.translate("No Inicializado") :         // Estado Inicial
                std == 1 ? $lserv.translate("Disponible") :              // Ok. sin Asignar.
                    std == 2 ? $lserv.translate("Asignado") :                   // OK. Asignado....
                        std == 3 ? $lserv.translate("Fallo") :                      // En Fallo....
                            std == 4 ? $lserv.translate("No Habilitado") :              // Ok. Deshabilitado....
                                std == 5 ? $lserv.translate("Disponible") : "???_" + std;    // Ok. Asignaci�n en progreso = disponible
        };
        //** */
        ctrl.hfColorEstado = function (std) {
            return std == 0 ? "text-muted" :                                // No Info.               
                std == 2 ? "text-primary" :                                 // Disponible.. 
                    std == 3 ? "text-success" :                                 // Asignado
                        std == 5 ? "text-primary" :                                 // Asignaci�n en progreso = disponible
                            "text-danger";                                   // Error
        };
        ctrl.hfTextoEstadoEquipo = function (std) {
            return std == 0 ? $lserv.translate("No Inicializado") :         // No Info.               
                std == 2 ? $lserv.translate("Disponible") :                 // Disponible.. 
                    std == 5 ? $lserv.translate("Disponible") :                 // Asignaci�n en progreso = disponible
                        std == 3 ? $lserv.translate("Asignado") :                   // Asignado
                            $lserv.translate("Fallo");                      // Error
        };
        //** */
        ctrl.hfEnableDisableShow = function (std) {
            return (std == 3);
        };
        /* */
        ctrl.txtHabilitar = function (equ) {
            return (equ.std == 1 || equ.std == 2 || equ.std == 3 || equ.std == 5) ? $lserv.translate("Disable") :
                equ.std == 4 ? $lserv.translate("Enable") : "???_" + equ.std;
        };
        //** */
        ctrl.led = function () {
            return ctrl.leds == led_std.Off ? "" : ctrl.leds == led_std.On1 ? "led-green" : ctrl.leds == led_std.On2 ? "led-yellow" : "led-red";
        };
        //** */
        ctrl.EnableDisable = function (item) {
            var bDisable = (item.std == 4);
            var strQuestion = item.equ + ". " +
                (bDisable ? $lserv.translate("�Desea Habilitar el Equipo?") :
                    $lserv.translate("�Desea Deshabilitar el Equipo?"));
            alertify.confirm(strQuestion,
                function () {
                    $serv.radio_gestormn_enable(item).then(
                        function () {
                            alertify.success($lserv.translate("Operacion Ejecutada."));
                            rdGestormnGet();
                        },
                        function (response) {
                            console.log(response);
                            alertify.error($lserv.translate("No se ha podido ejecutar la operacion."));
                        });
                },
                function () {
                    alertify.message($lserv.translate("Operacion Cancelada"));
                });
        };
        ctrl.RxSelected = function (item) {
            switch (item.ftipo) {
                case frec_tipos.UnoMasUno:
                case frec_tipos.EM:
                    return "????-" + item.ftipo.toString();
                case frec_tipos.Normal:
                case frec_tipos.FD:
                    break;
                default:
                    return "????";
            }
            if (item.selected_site == "")
                return "";
            return StringCut(item.selected_site, 10) + "/" + StringCut(item.selected_rx, 10) + "/" + item.selected_site_qidx.toString();
        };
        ctrl.TxSelected = function (item) {
            switch (item.ftipo) {
                case frec_tipos.UnoMasUno:
                case frec_tipos.EM:
                    return "????-" + item.ftipo.toString();
                case frec_tipos.Normal:
                    return "";
                case frec_tipos.FD:
                    break;
                default:
                    return "????";
            }
            return StringCut(item.selected_tx, 12);
        };
        //** */
        ctrl.txtOnVHF = function () {
            return ctrl.site_select + ' (VHF)'; /* txtMdSelect(ctrl.vhf_mode_select)*/
        };
        //** */
        ctrl.txtOnUHF = function () {
            return ctrl.site_select + ' (UHF)'; /*txtMdSelect(ctrl.uhf_mode_select)*/
        };
        //** */
        ctrl.txtMdSelect = function (md) {
            return md == 0 ? $lserv.translate("Transmisores") :
                md == 1 ? $lserv.translate("Receptores") : $lserv.translate("Transmisores y Receptores");
        };

        //** */
        ctrl.gearAsignarShow = function (eq) {
            return (eq.tip == 1) && (eq.std == 1 /*|| eq.std == 2*/);
        };
        //** */
        ctrl.gearAsignarText = function (eq) {
            return (eq.std == 1) ? $lserv.translate("Sintonizar") :
                (eq.std == 2) ? $lserv.translate("Liberar") : "???_" + eq.std;
        };
        //** */
        ctrl.Asignar = function (item) {
            switch (item.std) {
                case 0:
                    alertify.alert($lserv.translate("El equipo aun no esta inicializado. No puede ser asignado"));
                    break;
                case 1:
                    alertify.prompt($lserv.translate("Introduzca la frecuencia"), "",
                        function (evt, frec) {
                            var modo_val = item.grp == 0 ? 3 : 4;               // Rango de VHF (4) o de UHF (3)
                            if (frec && $lserv.validate(modo_val, frec) == true) {
                                var strQuestion = item.equ + " / " + frec + ". " +
                                    $lserv.translate("�Desea Asignar el equipo esta frecuencia?");

                                alertify.confirm(strQuestion,
                                    function () {
                                        var cmd = { equ: item.equ, cmd: 1, frec: frec };
                                        $serv.radio_gestormn_asigna(cmd).then(
                                            function (response) {
                                                alertify.success($lserv.translate("Operacion Ejecutada."));
                                            },
                                            function (response) {
                                                alertify.error($lserv.translate("No se ha podido ejecutar la operacion."));
                                                console.log(response);
                                            });
                                    },
                                    function () {
                                        alertify.message($lserv.translate("Operacion Cancelada"));
                                    });
                            }
                            else
                                alertify.alert($lserv.translate("Error en formato de frecuencia introducida. El formato de la frecuencia debe ser 'XXX.XXX'"));
                        },
                        function () {
                            alertify.message($lserv.translate("Operacion Cancelada"));
                        });
                    break;

                case 2:
                    var strQuestion = item.equ + ". " + $lserv.translate("�Desea Desasignar el equipo?");

                    alertify.confirm(strQuestion,
                        function () {
                            var cmd = { equ: item.eq, cmd: 0, frec: "---.--" };
                            $serv.radio_gestormn_asigna(cmd).then(function (response) {
                                alertify.success($lserv.translate("Operacion Ejecutada."));
                            }, function (response) {
                                console.log(response);
                                alertify.error($lserv.translate("No se ha podido ejecutar la operacion."));
                            });
                        },
                        function () {
                            alertify.message($lserv.translate("Operacion Cancelada"));
                        });
                    break;
                case 3:
                    alertify.alert($lserv.translate("El equipo esta en FALLO. No puede ser asignado"));
                    break;
                case 4:
                    alertify.alert($lserv.translate("El equipo esta deshabilitado. Habilitalo antes para poder asignarlo"));
                    break;
                default:
                    break;
            }
        };
        /** */
        ctrl.ResetServicio = function () {

            alertify.confirm($lserv.translate("�Desea reiniciar el servicio de gesti�n radio?"),
                function () {
                    $serv.radio_gestormn_reset().then(function () {
                        alertify.success($lserv.translate("Operacion Ejecutada."));
                    }, function (response) {
                        console.log(response);
                        alertify.error($lserv.translate("No se ha podido ejecutar la operacion."));
                    });
                },
                function () {
                    alertify.message($lserv.translate("Operacion Cancelada"));
                });
        };

        //** */
        ctrl.siteSelect = function (site) {
            ctrl.site_select = site;
        };
        //** */
        ctrl.hfLiberar = function (item) {
            var strQuestion = item.id + ". " + $lserv.translate("�Desea Liberar el Transmisor?");
            alertify.confirm(strQuestion,
                function () {
                    $serv.radio_hf_release(item).then(
                        function () {
                            alertify.success($lserv.translate("Operacion Ejecutada."));
                            rdHfGet();
                        },
                        function (response) {
                            console.log(response);
                            alertify.error($lserv.translate("No se ha podido ejecutar la operacion."));
                        });
                },
                function () {
                    alertify.message($lserv.translate("Operacion Cancelada"));
                });
        };

        /** Rutinas Generales */
        /** Datos desde el Servidor */
        // Sesiones Radio
        function rdSessionsGet() {
            $serv.radio_sessions_get().then(function (response) {
                console.log(response.data);
                if (rdSessionsChanged(response.data) == true) {
                    console.log("Cambio en tabla de sesiones");
                    ctrl.sessions = response.data;
                    rdSessionsSort();
                }
            }
                , function (response) {
                    console.log(response);
                });
        }
        function rdSessionsChanged(ses) {
            if (ses.constructor === Array && ctrl.sessions.constructor === Array) {
                /** Deben ser iguales */
                if (ses.length != ctrl.sessions.length)
                    return true;
                /** ... y en el mismo orden */
                for (i = 0; i < ses.length; i++) {
                    var nuevo = angular.toJson(ses[i]);
                    var viejo = angular.toJson(ctrl.sessions[i]);
                    if (nuevo != viejo)
                        return true;
                }
            }
            return false;
        }
        function rdSessionsSort() {
            var sorted = new Object();
            for (i = 0; i < ctrl.sessions.length; i++) {
                var session = ctrl.sessions[i];
                var fkey = session.idDestino ? session.idDestino.toString() : session.frec.toString();
                if (sorted[fkey] == undefined) {
                    sorted[fkey] = {
                        frec: session.frec,
                        ftipo: session.ftipo,
                        prio: session.prio,
                        fstd: session.fstd,
                        fpar: session.fpar,
                        fp_climax_mc: session.fp_climax_mc,
                        fp_bss_win: session.fp_bss_win,
                        selected_site: session.selected_site,
                        selected_site_qidx: session.selected_site_qidx,
                        selected_tx: session.selected_tx,
                        selected_rx: session.selected_resource,
                        fp_unomasuno: session.UnoMasUno,
                        fp_bss_mod: session.selected_BSS_method,
                        fp_tx_mod: session.ftipo == 2 ? (session.selected_tx == "CLX" ? "Climax" : "BTS") : "",
                        ses: new Object()
                    };
                }
                var sindex = session.uri.toString();
                sorted[fkey].ses[sindex] = {
                    uri: session.uri,
                    site: session.site,
                    tipo: session.tipo,
                    std: session.std,
                    dis: session.UnoMasUnoDisabled,
                    tx_rtp: session.tx_rtp,
                    tx_cld: session.tx_cld,
                    tx_owd: session.tx_owd,
                    rx_rtp: session.rx_rtp,
                    rx_qidx: session.rx_qidx
                };
            }
            /** Ordeno por frecuencia */
            //sorted.sort();
            ctrl.frecs = sorted;
        }
        /** Gestor M+N*/
        function rdGestormnGet() {
            if ($lserv.RdModuleExist('M+N')) {
                $serv.radio_gestormn_get().then(function (response) {
                    console.log(response.data);
                    if (rdMNManagerChanged(response.data) == true) {
                        console.log("Cambio en tabla de M+N");
                        ctrl.gestormn = response.data;
                        rdMNManagerSort();
                    }
                    ctrl.leds = ctrl.leds == led_std.On2 ? led_std.On1 : led_std.On2;
                }, function (response) {
                    console.log(response);
                    ctrl.leds = led_std.Off;
                });
            } else {
                ctrl.gestormn = [];
            }
        }
        function rdMNManagerChanged(mn) {
            if (mn.constructor === Array && ctrl.gestormn.constructor === Array) {
                /** Deben ser iguales */
                if (mn.length != ctrl.gestormn.length)
                    return true;
                /** ... y en el mismo orden */
                for (i = 0; i < mn.length; i++) {
                    if (angular.toJson(mn[i]) != angular.toJson(ctrl.gestormn[i]))
                        return true;
                }
            }
            return false;
        }
        function rdMNManagerSort() {
            var PrimerNivel = new Object();
            for (i = 0; i < ctrl.gestormn.length; i++) {

                if (!ctrl.gestormn[i].emp)
                    ctrl.gestormn[i].emp = $lserv.translate("Emplazamiento");

                var empl = ctrl.gestormn[i].emp.toString();
                if (PrimerNivel[empl] == undefined) {
                    PrimerNivel[empl] = { name: empl, equipos: [] };
                }
                PrimerNivel[empl].equipos.push(ctrl.gestormn[i]);
            }

            /** Ordenar en cada Emplazamiento. Criterio 1: BANDA (vhf,uhf), Criterio 2: TIPO (Rx,Tx), Criterio 3: Modo (M,N)*/
            jQuery.each(PrimerNivel, function (index, item) {
                item.equipos.sort(function (a, b) {
                    if (a.grp == b.grp) {
                        if (a.mod == b.mod) {
                            return a.tip < b.tip ? -1 : a.tip > b.tip ? 1 : 0;
                        }
                        return a.mod < b.mod ? -1 : 1;
                    }
                    return a.grp < b.grp ? -1 : 1;
                });
            });

            ctrl.mnman = PrimerNivel;
            /** Selecciono el emplazamiento del primer equipo */
            if (!ctrl.site_select)
                ctrl.site_select = ctrl.gestormn.length == 0 ? "" : ctrl.gestormn[0].emp;
        }
        /** Gestor Tx HF*/
        function rdHfGet() {
            if ($lserv.RdModuleExist('HF')) {
                $serv.radio_hf_get().then(function (response) {
                    console.log(response.data);
                    if (rdHfChanged(response.data) == true) {
                        console.log("Cambio en tabla HF");
                        ctrl.gestorhf = response.data;
                    }
                }
                    , function (response) {
                        console.log(response);
                    });
            } else {
                ctrl.gestorhf = [];
            }
        }
        function rdHfChanged(newdata) {
            return angular.toJson(newdata) != angular.toJson(ctrl.gestorhf);
        }

        // Gestor 1+1
        ctrl.dtUnoMasUno = [];
        ctrl.dtUnoMasUnoSelectedFrec = "";
        function rdUnoMasUnoGet() {
            if ($lserv.RdModuleExist('1+1')) {
                $serv.radio_11_get().then(function (response) {
                    console.log("RD1+1 Data =>", response.data);
                    var normalizedData = rdUnoMasUnoNormalize(response.data);
                    if (rdUnoMasUnoChanged(normalizedData) == true) {
                        console.log("Cambio en tabla de 1+1");
                        ctrl.dtUnoMasUno = normalizedData;
                        ctrl.dtUnoMasUnoSelectedFrec = ctrl.dtUnoMasUnoSelectedFrec == "" ?
                            ctrl.dtUnoMasUno.length > 0 ? ctrl.dtUnoMasUno[0].idDestino : "" :
                            ctrl.dtUnoMasUnoSelectedFrec;
                    }
                }, function (response) {
                    console.log(response);
                });
            } else {
                ctrl.dtUnoMasUno = [];
            }
        }
        function rdUnoMasUnoChanged(newdata) {
            return angular.toJson(newdata) != angular.toJson(ctrl.dtUnoMasUno);
        }
        function rdUnoMasUnoNormalize(datain) {

            //var data = shuffle(datain);
            //console.log("1+1 Data shuffled => ", data);
            var data = datain;
            // Utiliza linq.js
            // Los agrupa por frecuencia y despues por emplazamiento...
            var res = Enumerable.from(data)
                .orderBy('$.fr')
                .groupBy(
                    '$.idDestino', '$',
                    function (idDestino, grp) {
                        //console.log("grp =>", grp);
                        var tmp = Enumerable.from(grp.getSource()).select(t => t).toArray();
                        var res1 = Enumerable.from(/*grp.getSource()*/tmp)
                            .orderBy('$.site')
                            .groupBy('$.site', '$', function (site, grp1) {
                                var en = Enumerable.from(grp1.getSource());
                                var txs = en.where(eq => eq.tx == 1).toArray();
                                var rxs = en.where(eq => eq.tx == 0).toArray();
                                return { site: site, txs: txs, rxs: rxs };
                            })
                            .toArray();
                        return { idDestino: idDestino, fr: tmp[0].fr, fdata: tmp[0].fdata, sites: res1 };
                    }
                )
                .toArray();
            console.log("Res =>", res);
            return res;
        }
        ctrl.rdUnoMasUnoId = function (res) {
            var retorno =/* "(" + (res.ab == 1 ? "A" : "B") + ") " + */res.id;
            return StringCut(retorno, 24);
        };
        ctrl.rdUnoMasUnoIdClass = function (res) {
            var retorno = res.ses == 0 ? "bg-danger" : "bg-success";
            return retorno;
        };
        ctrl.rdUnoMasUnoRxIdClass = function (res) {
            var retorno = res.ab == 0 ? "bg-secondary" : res.ses == 0 ? "bg-danger" : "bg-success";
            return retorno;
        };
        ctrl.rdUnoMasUnoTxDisabled = function (res) {
            return res.ses == 0;
        };
        ctrl.rdUnoMasUnoSelectMain = function (equ) {
            if ($lserv.RdModuleExist('1+1')) {
                var strQuestion = equ.id + $lserv.translate(" �Desea Elegir el equipo como Seleccionado?");
                alertify.confirm(strQuestion,
                    function () {
                        $serv.radio_11_select(equ.id).then(function (response) {
                            console.log("RD1+1 Post Response => ", response.data);
                            alertify.success($lserv.translate("Operacion Efectuada"));
                        }, function (response) {
                            console.log("RD1+1 Post Error => ", response);
                            alertify.error($lserv.translate(response.data.res));
                        });
                    },
                    function () {
                        alertify.message($lserv.translate("Operacion Cancelada"));
                    });
            } else {
                // Meter un Mensaje.
            }
        };
        ctrl.rdUnoMasUnoEnable = function (equ) {
            if ($lserv.RdModuleExist('1+1')) {
                var enable = equ.ab == 0 ? "enable" : "disable";
                var strOperation = enable == "enable" ? $lserv.translate(" �Desea Habilitar el equipo?") :
                    $lserv.translate(" �Desea Deshabilitar el equipo?");
                var strQuestion = equ.id + strOperation;
                alertify.confirm(strQuestion,
                    function () {
                        $serv.radio_11_enable(equ.id, enable).then(function (response) {
                            console.log("RD1+1 Post Response => ", response.data);
                            alertify.success($lserv.translate("Operacion Ejecutada."));
                        }, function (response) {
                            console.log("RD1+1 Post Error => ", response);
                            alertify.error($lserv.translate(response.data.res));
                        });
                    },
                    function () {
                        alertify.message($lserv.translate("Operacion Cancelada"));
                    });
            } else {
                // Meter un Mensaje.
            }
        };
        ctrl.rdUnoMasUnoFrMode = (fr) => {
            return (!fr || !fr.fdata ) ? "Error" : fr.fdata.tp == 0 ? $lserv.translate("Simple") : fr.fdata.tp == 2 ? $lserv.translate("Multiemplazamiento") : "??" + fr.fdata.tp + "??";
        };
        ctrl.rdUnoMasUnoFrTxMode = (fr) => {
            if (!fr || !fr.fdata)
                return { txt: $lserv.translate("Error"), clx: "" };
            if (fr.fdata.tp == 0)
                return { txt: "", clx: "" };
            var txt = fr.fdata.txm == 0 ? $lserv.translate("CLX") : fr.fdata.txm == 1 ? $lserv.translate("Ultimo RX") : "??" + fr.fdata.txm + "??";
            var clx = fr.fdata.txm == 1 ? "freqDegraded" : "";
            return { txt: txt, clx: clx};
        };
        ctrl.rdUnoMasUnoFrSelectedSiteClass = (fr, site) => {
            if (!fr || !fr.fdata)
                return "";
            if (fr.fdata.tp == 0)
                return "";
            return fr.fdata.txs == site ? "bg-warning" : "";
        };
        ctrl.rdUnoMasUnoHtmlFilter = (item) => item.idDestino === ctrl.dtUnoMasUnoSelectedFrec;


        /** Funcion Periodica del controlador */
        var timer = $interval(function () {
            if (ctrl.pagina == 0) {
                rdSessionsGet();
            } else if (ctrl.pagina == 1 || ctrl.pagina == 2) {
                rdGestormnGet();
            } else if (ctrl.pagina == 3) {
                rdHfGet();
            } else if (ctrl.pagina == 4) {
                rdUnoMasUnoGet();
            }
        }, pollingTime);

        /** */
        $scope.$on('$viewContentLoaded', function () {
            rdSessionsGet();
            rdGestormnGet();
            rdHfGet();
            rdUnoMasUnoGet();
        });

        /** Salida del Controlador. Borrado de Variables */
        $scope.$on("$destroy", function () {
            $interval.cancel(timer);
        });

    });
