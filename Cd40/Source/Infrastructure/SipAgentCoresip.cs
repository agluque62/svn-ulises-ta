using System;
using System.Text;
using System.Runtime.InteropServices;

using U5ki.Infrastructure.Properties;
using System.ComponentModel;

namespace U5ki.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    /// <param name="data"></param>
    /// <param name="len"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void LogCb(int level, string data, int len);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void KaTimeoutCb(int call);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void RdInfoCb(int call, [In] CORESIP_RdInfo info);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="stateInfo"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void CallStateCb(int call, [In] CORESIP_CallInfo info, [In] CORESIP_CallStateInfo stateInfo);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="call2replace"></param>
    /// <param name="info"></param>
    /// <param name="inInfo"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void CallIncomingCb(int call, int call2replace, [In] CORESIP_CallInfo info, [In] CORESIP_CallInInfo inInfo);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="transferInfo"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void TransferRequestCb(int call, [In] CORESIP_CallInfo info, [In] CORESIP_CallTransferInfo transferInfo);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="code"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void TransferStatusCb(int call, int code);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="confInfo"></param>
    /// <param name="from"> Uri del origen del Notify </param>
    /// <param name="lenfrom"> Longitud de from </param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void ConfInfoCb(int call, [In] CORESIP_ConfInfo confInfo, string from, uint lenfrom);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="confInfo"></param>
    /// <param name="from"> Uri del origen del Notify </param>
    /// <param name="lenfrom"> Longitud de from </param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void ConfInfoAccCb(string accountId, [In] CORESIP_ConfInfo confInfo, string from, uint lenfrom);
    /// <summary>
    /// Callback que se llama cuando se recibe un notify al evento de dialogo
    /// </summary>
    /// <param name="xml_body">body del notify</param>
    /// <param name="length">longitud del body</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void DialogNotifyCb(string xml_body, uint length);

    /// <summary>
    /// Callback que se llama cuando se recibe un mensaje de texto
    /// </summary>
    /// <param name="from">URI of the sender</param>
    /// <param name="to">URI of the destination message</param>
    /// <param name="contact">The Contact URI of the sender, if present.</param>
    /// <param name="mime_type">MIME type of the message.</param>
    /// <param name="body">The message content</param>
    /// 
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void PagerCb(string from_uri, uint from_uri_len,
                string to_uri, uint to_uri_len, string contact_uri, uint contact_uri_len,
                string mime_type, uint mime_type_len, string body, uint body_len);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromUri"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void OptionsReceiveCb(string fromUri, string callid, int statusCodem, string supported, string allow);

    /// <summary>
    /// Esta funcion se llama cuando se recibe un options del tipo utilizado para la negociacion Call Forward
    /// </summary>
    /// <param name="accId">Account obtenido de la uri To</param>
    /// <param name="from_uri">Uri de la cabecera From</param>
    /// <param name="cfwr_options_type">Tipo de OPTIONS para la negociacion. Es del tipo CORESIP_CFWR_OPT_TYPE</param>
    /// <param name="body">Es el cuerpo del mensaje, terminado con el caracter '\0'</param>
    /// <param name="hresp">Manejador necesario para enviar la respuesta
    ///
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void CfwrOptReceivedCb(int accId, string from_uri, CORESIP_CFWR_OPT_TYPE cfwr_options_type, string body, uint hresp);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accId">Account obtenido de la uri To</param>
    /// <param name="from_uri">Uri de la cabecera From</param>
    /// <param name="cfwr_options_type">Tipo de OPTIONS para la negociacion. Es del tipo CORESIP_CFWR_OPT_TYPE</param>
    /// <param name="body">Es el cuerpo del mensaje, terminado con el caracter '\0'</param>
    /// <param name="hresp">Manejador necesario para enviar la respuesta
    ///
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void CfwrOptReceivedAccCb(string accId, string from_uri, CORESIP_CFWR_OPT_TYPE cfwr_options_type, string body, uint hresp);


    /// <summary>
    /// Esta funcion se llama cuando se recibe la respuesta a un options del tipo utilizado para la negociacion Call Forward
    /// </summary>
    /// <param name="accId">Account obtenido de la uri From</param>
    /// <param name="dstUri">Uri de la cabecera To. Es decir, es la uri del agente que nos envia la respuesta. Finalizado con '\0'</param>
    /// <param name="callid">Call Id recibido. Finalizado con '\0'</param>
    /// <param name="st_code">Code de la respuesta</param>
    /// <param name="cfwr_options_type">Tipo de OPTIONS para la negociacion. Es del tipo CORESIP_CFWR_OPT_TYPE</param>
    /// <param name="body">Es el cuerpo del mensaje, terminado con el caracter '\0'.</param>
    ///
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void CfwrOptResponseCb(int accId, string dstUri, string callid, int st_code, CORESIP_CFWR_OPT_TYPE cfwr_options_type, string body);

    /// <summary>
    /// Esta funcion se llama cuando se recibe la respuesta a un options del tipo utilizado para la negociacion Call Forward
    /// </summary>
    /// <param name="accId">Account obtenido de la uri From</param>
    /// <param name="dstUri">Uri de la cabecera To. Es decir, es la uri del agente que nos envia la respuesta. Finalizado con '\0'</param>
    /// <param name="callid">Call Id recibido. Finalizado con '\0'</param>
    /// <param name="st_code">Code de la respuesta</param>
    /// <param name="cfwr_options_type">Tipo de OPTIONS para la negociacion. Es del tipo CORESIP_CFWR_OPT_TYPE</param>
    /// <param name="body">Es el cuerpo del mensaje, terminado con el caracter '\0'.</param>
    ///
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void CfwrOptResponseAccCb(string accId, string dstUri, string callid, int st_code, CORESIP_CFWR_OPT_TYPE cfwr_options_type, string body);

    /// <summary>
    /// Esta funcion se llama cuando se recibe un 302 (Moved Temporally) avisando que hay una redireccion pendiente de la llamada
    /// Para aceptar o rechazar la redireccion, la aplicacion debe llamar a la funcion #CallProccessRedirect
    /// </summary>
    /// <param name="call">Identificador de la llamada</param>
    /// <param name="dstUri">Uri a la que se quiere redirigir la llamada. String terminado en cero.</param>
    ///
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void MovedTemporallyCb(int call, string dstUri);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="lenInfo"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void InfoReceivedCb(int call, string info, uint lenInfo);

    /**
    * WG67SubscriptionCb
    * Esta funcion se llama cuando hay un cambio en el estado de una subscripcion al evento WG67KEY-IN.
        Como Suscriptor se llama cuando ha cambiado el estado, o porque se ha recibido un NOTIFY.
        Como Notificador se llama cuando ha cambiado el estado de la suscripcion.
    * @param	info			Estructura con la info
    */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void WG67SubscriptionStateCb(CORESIP_WG67_Subscription_Info wg67Info);

    /**
    * WG67SubscriptionReceivedCb
    * Esta funcion se llama cuando se recibe el primer request de suscripcion al evento WG67KEY-IN.
    * Si dentro de esta callback se llama a la funcion CORESIP_Set_WG67_notify_status se establece un estado inicial de la suscripcion
    * @param	accId. Identificador del account.
    * @param	subscriberUri. uri del suscriptor
    */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void WG67SubscriptionReceivedCb(int accId, string subscriberUri);

    /// <summary>
    ///  Received when subscription to conference arrives
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="lenInfo"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void IncomingSubscribeConfCb(int call, string from, uint lenInfo);
    /// <summary>
    ///  Received when subscription to conference arrives
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="lenInfo"></param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void IncomingSubscribeConfAccCb(string accountId, string from, uint lenInfo);

    /*Callback para recibir notificaciones por la subscripcion de presencia*/
    /*	dst_uri: uri del destino cuyo estado de presencia ha cambiado.
        *	subscription_status: vale 0 la subscripcion al evento no ha tenido exito. 
        *	presence_status: vale 0 si no esta presente. 1 si esta presente.
        */
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void SubPresCb(string dst_uri, int subscription_status, int presence_status);

    /*Callback para recibir notificaon de fin de reproduccion de un wav. Lo utiliza el ETM*/
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void FinWavCb(int Code);

    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_CallType
    {
        [Description("IA CALL")]
        CORESIP_CALL_IA,
        [Description("MON CALL")]
        CORESIP_CALL_MONITORING,
        [Description("G/G MON CALL")]
        CORESIP_CALL_GG_MONITORING,
        [Description("A/G MON CALL")]
        CORESIP_CALL_AG_MONITORING,
        [Description("DA CALL")]
        CORESIP_CALL_DIA,
        CORESIP_CALL_RD,
        CORESIP_CALL_UNKNOWN
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_Priority
    {
        [Description("Emergencia")]
        CORESIP_PR_EMERGENCY,
        [Description("Urgente")]
        CORESIP_PR_URGENT,
        [Description("Normal")]
        CORESIP_PR_NORMAL,
        [Description("No Urgente")]
        CORESIP_PR_NONURGENT,
        CORESIP_PR_UNKNOWN
    }
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum CORESIP_CallFlags
    {
        [Description("Type: Radio txrxmode=TxRx")]
        CORESIP_CALL_NO_FLAGS = 0,
        CORESIP_CALL_NINGUNO = 0x0,         //Type: Radio-TxRx txrxmode=TxRx
        [Description("Conference focus")]
        CORESIP_CALL_CONF_FOCUS = 0x1,
        [Description("Tipo de sesion Radio Coupling. Type: Coupling")]
        CORESIP_CALL_RD_COUPLING = 0x2,
        [Description("Como VCS	txrxmode=Rx. Como GRS indica que es del tipo receptor")]
        CORESIP_CALL_RD_RXONLY = 0x4,
        [Description("Como VCS	txrxmode=Tx. Como GRS indica que es del tipo transmisor")]
        CORESIP_CALL_RD_TXONLY = 0x8,
        [Description("Echo Canceller")]
        CORESIP_CALL_EC = 0x10,
        [Description("Through external central IP")]
        CORESIP_CALL_EXTERNAL_IP = 0x20,
        [Description("Tipo de sesion Radio IDLE. Type: Radio-idle")]
        CORESIP_CALL_RD_IDLE = 0x40,
        [Description("Tipo de sesion Radio-Rxonly. Type: Radio-rxonly")]
        CORESIP_CALL_RD_RADIO_RXONLY = 0x80,
        [Description("No el sdp no incluira txrxmode. ")]
        CORESIP_CALL_NO_TXRXMODE = 0x100    //Sin txrxmode. 
                                            //Como VCS, si este flag es activado, CORESIP_CALL_RD_RXONLY y CORESIP_CALL_RD_TXONLY no pueden estar activados.
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_CallState
    {
        [Description("Null")]
        CORESIP_CALL_STATE_NULL,                    /**< Before INVITE is sent or received  */
        [Description("Calling")]
        CORESIP_CALL_STATE_CALLING,             /**< After INVITE is sent		    */
        [Description("Incoming")]
        CORESIP_CALL_STATE_INCOMING,                /**< After INVITE is received.	    */
        [Description("Early")]
        CORESIP_CALL_STATE_EARLY,                   /**< After response with To tag.	    */
        [Description("Connecting")]
        CORESIP_CALL_STATE_CONNECTING,          /**< After 2xx is sent/received.	    */
        [Description("Confirmed")]
        CORESIP_CALL_STATE_CONFIRMED,           /**< After ACK is sent/received.	    */
        [Description("Disconnected")]
        CORESIP_CALL_STATE_DISCONNECTED     /**< Session is terminated.		    */
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_CallRole
    {
        CORESIP_CALL_ROLE_UAC,
        CORESIP_CALL_ROLE_UCS
    }
    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_MediaStatus
    {
        CORESIP_MEDIA_NONE,
        CORESIP_MEDIA_ACTIVE,
        CORESIP_MEDIA_LOCAL_HOLD,
        CORESIP_MEDIA_REMOTE_HOLD,
        CORESIP_MEDIA_ERROR
    }
    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_MediaDir
    {
        CORESIP_DIR_NONE,
        CORESIP_DIR_SENDONLY,
        CORESIP_DIR_RECVONLY,
        CORESIP_DIR_SENDRECV
    }
    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_PttType
    {
        CORESIP_PTT_OFF,
        [Description("Normal")]
        CORESIP_PTT_NORMAL,
        [Description("Coupling")]
        CORESIP_PTT_COUPLING,
        [Description("Prioritario")]
        CORESIP_PTT_PRIORITY,
        [Description("Emergencia")]
        CORESIP_PTT_EMERGENCY,
        [Description("Test")]
        CORESIP_PTT_TEST                //Solo valido para ED137C
    }
    /// <summary>
    /// 
    /// </summary>
    public enum CORESIP_SndDevType
    {
        CORESIP_SND_INSTRUCTOR_MHP, //La aplicacion crea 2. El In es la entrada de microfono, Out es la salida del altavoz
        CORESIP_SND_ALUMN_MHP, //La aplicacion crea 2. El In es la entrada de microfono, Out es la salida del altavoz
        CORESIP_SND_MAX_IN_DEVICES,
        CORESIP_SND_MAIN_SPEAKERS = CORESIP_SND_MAX_IN_DEVICES,
        CORESIP_SND_LC_SPEAKER, //La aplicacion crea 2. In Es la linea de retorno del altavoz LC. Out es la salida
        CORESIP_SND_RD_SPEAKER, //La aplicacion crea 2. In Es la linea de retorno del altavoz RD. Out es la salida
        CORESIP_SND_INSTRUCTOR_RECORDER, //La aplicacion crea 2 (in o out). El In es el retorno del altavoz del instructor y el out es el del grabador analogico
        CORESIP_SND_ALUMN_RECORDER, //La aplicacion crea 2 (in o out). El In es el retorno del altavoz del alumno y el Out es el del grabador analogico
        CORESIP_SND_RADIO_RECORDER, //La aplicacion crea solo 1. Que es el Out al grabador analogico
        CORESIP_SND_LC_RECORDER, //La aplicacion crea solo 1. Que es el Out al grabador analogico
        CORESIP_SND_HF_SPEAKER, //La aplicacion crea 2. In Es la linea de retorno del altavoz HF. Out es la salida
        CORESIP_SND_HF_RECORDER, //Es la linea Out hacia el grabador analogico
        CORESIP_SND_INSTRUCTOR_BINAURAL,
        CORESIP_SND_ALUMNO_BINAURAL,
        CORESIP_SND_UNKNOWN
    }

    //EDU 20170223
    public enum CORESIP_FREQUENCY_TYPE { Simple = 0, Dual = 1, FD = 2, ME = 3 }         // 0. Normal, 1: 1+1, 2: FD, 3: EM
    public enum CORESIP_FREQUENCY_MODO_TRANSMISION {
        Climax = 0,
        UltimoReceptor = 1,
        Manual = 2,
        Ninguno = 3
    }
    public enum CORESIP_CLD_CALCULATE_METHOD { Relative, Absolute }
    public enum CORESIP_PttMuteType
    {
        DESACTIVADO = 0,
        ACTIVADO = 1
    }
    public enum CORESIP_CFWR_OPT_TYPE
    {
        CORESIP_CFWR_OPT_REQUEST,
        CORESIP_CFWR_OPT_RELEASE,
        CORESIP_CFWR_OPT_UPDATE
    }

    public enum CORESIP_REDIRECT_OP
    {
        CORESIP_REDIRECT_REJECT,
        CORESIP_REDIRECT_ACCEPT
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ReinviteType
    {
        Coupling = 9,
        RadioRxonly = 7,
        RadioTxRx = 5,
        Idle = 6
    }

    public class WG67Info
    {
        public struct SubscriberInfo
        {
            public ushort PttId;
            public string SubsUri;
        }

        public string DstUri;

        public bool SubscriptionTerminated;
        public uint SubscribersCount;
        public string LastReason;

        public SubscriberInfo[] Subscribers;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_CallInfo
    {
        public int AccountId;
        public CORESIP_CallType Type;                   //Tipo de llamada
        public CORESIP_Priority Priority;               //Prioridad. Se refiere a los valores soportados en la cabecera Priority
        public uint CallFlags;

        public int SourcePort;                          //UNIFETM: Este campo falta en ULISES. En el ETM no se utiliza. Se le asigna valor, pero para nada. 
        public int DestinationPort;                     //UNIFETM: Este campo falta en ULISES. Se asigna el valor en onIncomingCall. es un valor que se retorna en la callback. No es de entrada.

        public int PreferredCodec = 0;

        //EDU 20170223
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_ZONA_LENGTH + 1)]
        public string Zona;                             //UNIFETM: Este campo falta en ETM. Asignarle el valor 0 en ETM

        public CORESIP_FREQUENCY_TYPE FrequencyType;    //UNIFETM: Este campo falta en ETM. Asignarle el valor Simple en ETM
        public CORESIP_FREQUENCY_MODO_TRANSMISION ModoTransmision;
        public CORESIP_CLD_CALCULATE_METHOD CLDCalculateMethod; //UNIFETM: Este campo falta en ETM. Asignarle el valor Relative en ETM
        public int BssWindows;                          //UNIFETM: Este campo falta en ETM. Asignarle el valor 0 en ETM
        public bool AudioSync;                          //UNIFETM: Este campo falta en ETM. Asignarle el valor 0 en ETM
        public bool AudioInBssWindow;                   //UNIFETM: Este campo falta en ETM. Asignarle el valor 0 en ETM
        public int cld_supervision_time;            //Tiempo de supervision CLD en segundos. Si el valor es 0 entonces no hay supervison de CLD. //UNIFETM: Este campo falta en ETM. Asignarle el valor 0 en ETM
        public int forced_cld = -1;                 //Para ETM, es el CLD que se envia de forma forzada en ms. 
                                                    //Si vale -1 entonces no se fuerza y se envia el CLD calculado o el Tn1 en el caso del ETM
                                                    //EN ULISES SE IGNORA

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_BSS_LENGTH + 1)]
        public string bss_method;                   //Solo para VCS ULISES, es el metodo BSS. Para ETM deber ser un string de longitud cero
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_BSS_LENGTH * 3 + 1)]
        public string etm_vcs_bss_methods;          //Para ETM, string con los literales de los metodos BSS separados por comas, enviados por VCS. En ulises string long 0

        public uint porcentajeRSSI;                 //Peso del valor de Qidx del tipo RSSI en el calculo del Qidx final. 0 indica que el calculo es interno (centralizado). 9 que el calculo es solo el RSSI.  //UNIFETM: Este campo falta en ETM. Asignarle el valor 0 en ETM

        public int R2SKeepAlivePeriod = -1;         //Valor entre 20 y 1000 del periodo de los KeepAlives. Si el valor es -1, se ignora y se utilizará el valor por defecto (200). EN ULISES poner -1
        public int R2SKeepAliveMultiplier = -1;     //Valor entre 2 y 50 del numbero de R2S-Keepalive no recibidos antes producirse un Keep Alive time-out. Si el valor es -1, se ignora y se utilizará el valor por defecto (10). EN ULISES PONER -1

        public int NoFreqDisconn;                   //Si vale distinto de cero para llamadas hacia GRS, indica que la sesion no se desconecte cuando se modifica 
                                                    //el identificador de la frecuencia (Fid) en el GRS
                                                    //Y se envia Notify al evento WG67 cuando se modifica le Fid. Solo es valido en ED137C

    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_CallOutInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string DstUri;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string ReferBy;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
        public string RdFr;                 //Con valor "000.000" no se envia el fid. Debe estar terminado cn el caracter '\0'

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
        public string IdDestino;             //Identificador del destino de radio. Si esta campo tiene una longitud mayor que cero
                                             //entonces, el identificador para agrupar las radios es este.
                                             //Si no, entonces el identificador para agrupar es RdFr

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
        public string RdMcastAddr;
        public uint RdMcastPort;

        //Referente al replaces. Esto no se necesita cuando el DstUri se obtiene de un REFER y ya tiene la info de replaces
        public bool RequireReplaces;		//Vale true si requiere replaces
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_CALLID_LENGTH + 1)]
        public string CallIdToReplace;	    //Call id de la llamada a reemplazar
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_TAG_LENGTH + 1)]
        public string ToTag;				//Tag del To de la llamada a reemplazar
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_TAG_LENGTH + 1)]
        public string FromTag;              //Tag del From de la llamada a reemplazar
        public bool EarlyOnly;              //Vale true si se requiere el parametro early-only en el replaces
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_CallInInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
        public string SrcId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
        public string SrcIp;
        public uint SrcPort;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
        public string SrcSubId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
        public string SrcRs;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
        public string DstId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
        public string DstIp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
        public string DstSubId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_NAME_LENGTH + 1)]
        public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
        public string RdFr;                             //EN ETM Valor de fid de la llamada entrante hacia el GRS
                                                        //EN ULISES SE IGNORA
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_BSS_LENGTH + 1)]
        public string etm_grs_bss_method_selected;      //Para ETM, como GRS receptor/transceptor, Es el metodo BSS seleccionado para enviar el Qidx. 
                                                        //EN ULISES SE IGNORA
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_CallTransferInfo
    {
        public IntPtr TxData;
        public IntPtr EvSub;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
        public string TsxKey;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string ReferBy;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2 * SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string ReferTo;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
        public string DstId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
        public string DstIp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
        public string DstSubId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
        public string DstRs;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_CallStateInfo
    {
        public CORESIP_CallState State;
        public CORESIP_CallRole Role;
        public uint isRadReinvite;                  //Si vale distinto de 0 indica que el estado ha isdo provocado por un reinvite del ripo radio
        public uint radReinvite_accepted;           //Este parametro solo se tiene en cuenta si isRadReinvite=1. si reinvite_accepted=1 entonces ha habido un reinvite aceptado
                                                    //si reinvite_accepted=0 entonces ha habido un reinvite rechazado
        public uint radRreinviteCallFlags;          //Este parametro solo se tiene en cuenta si isRadReinvite=1. Son los flags del re-invite.

        public int LastCode;                                        // Util cuando State == PJSIP_INV_STATE_DISCONNECTED
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_REASON_LENGTH + 1)]
        public string LastReason;

        public int LocalFocus;
        public int RemoteFocus;
        public CORESIP_MediaStatus MediaStatus;
        public CORESIP_MediaDir MediaDir;

        // CORESIP_CALL_RD y PJSIP_INV_STATE_CONFIRMED
        public ushort PttId;
        public uint ClkRate;
        public uint ChannelCount;
        public uint BitsPerSample;
        public uint FrameTime;

        public bool remote_grs_supports_ED137C_Selcal;		//es true si la sesion con el grs remoto soporta selcal de ED137C
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_PttInfo
    {
        public CORESIP_PttType PttType;
        public ushort PttId;
        public uint PttMute;
        public uint Squ;
        public uint RssiQidx;           //Valor de Qidx del tipo RSSI que se envia cuando es un agente simulador de radio y Squ es activo. Ingonar en Ulises
    }
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_RdInfo
    {
        public CORESIP_PttType PttType;
        public ushort PttId;
        public int PttMute;
        public int Squelch;
        public int Sct;

        public int tx_ptt_mute_changed;     //Si es distinto de cero indica que ha habido un cambio en el PTT mute que el servidor de radio transmite a la radio.
                                            //No es el valor del Ptt Mute que viene en la extension de cabecera, el cual es el campo PttMute

        //EDU 20170224
        public int rx_rtp_port;
        public int rx_qidx;
        public bool rx_selected;
        public int tx_rtp_port;
        public int tx_cld;
        public int tx_owd;

        public uint MAM_received;          //Si es distinto de cero entonces se ha recibido un MAM	y los siguientes campos son validos	
        public uint Tn1_ms;                //Tn1 en ms calculado del MAM recibido
        public uint Tj1_ms;                //Tj1 en ms calculado del MAM recibido
        public uint Tid_ms;                //Tid en ms calculado del MAM recibido
        public uint Tsd_ms;                //Tsd en ms calculado del MAM recibido
        public int Ts2_ms;                 //Ts2 en ms calculado del MAM recibido. Un valor negativo indica que no se ha recibido.
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_ConfInfo
    {
        public uint Version;
        public uint UsersCount;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_CONF_STATE_LENGTH + 1)]
        public string State;

        public struct ConfUser
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
            public string Id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
            public string Name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
            public string Role;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_CONF_STATE_LENGTH + 1)]
            public string State;
        }
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SipAgent.CORESIP_MAX_CONF_USERS)]
        public ConfUser[] Users;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_tone_digit
    {
        public char digit;         /**< The ASCI identification for the digit.	
						        En caso de que quiera reproducir una pausa, digit debe ser una coma, on_msec debe valer cero y off_msec valdra el tiempo de la pausa */
        public short on_msec;      /**< Playback ON duration, in miliseconds.	    */
        public short off_msec;     /**< Playback OFF duration, ini miliseconds.    */
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_tone_digit_map
    {
        public struct strdigits
        {
            public char digit;     /**< The ASCI identification for the digit.	*/
            public short freq1;    /**< First frequency.			*/
            public short freq2;    /**< Optional second frequency.		*/
        }

        public uint count;     /**< Number of digits in the map. 16 maximo	*/

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public strdigits[] digits;
    }


    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_Callbacks
    {
        public LogCb OnLog;
        public KaTimeoutCb OnKaTimeout;
        public RdInfoCb OnRdInfo;
        public CallStateCb OnCallState;
        public CallIncomingCb OnCallIncoming;
        public TransferRequestCb OnTransferRequest;
        public TransferStatusCb OnTransferStatus;
        public ConfInfoCb OnConfInfo;
        public OptionsReceiveCb OnOptionsReceive;
        public WG67SubscriptionStateCb OnWG67SubscriptionState;
        public WG67SubscriptionReceivedCb OnWG67SubscriptionReceived;
        public InfoReceivedCb OnInfoReceived;
        public IncomingSubscribeConfCb OnIncomingSubscribeConf;
        public SubPresCb OnSubPres;
        public FinWavCb OnFinWavCb;
        public DialogNotifyCb OnDialogNotify;
        public PagerCb OnPager;

        public CfwrOptReceivedCb OnCfwrOptReceived;
        public CfwrOptResponseCb OnCfwrOptResponse;
        public MovedTemporallyCb OnMovedTemporally;

    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_WG67_Subscription_Info
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CORESIP_WG67SessionsInfo
        {
            public ushort PttId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
            public string Uri;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_SESSTYPE_LENGTH + 1)]
            public string SessionType;
        }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string Role;                     //Puede valer "subscriber" o "notifier"
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string SubscriberUri;            //Uri del subscriptor
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string NotifierUri;              //Uri del GRS notificador

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_REASON_LENGTH + 1)]
        public string SubscriptionState;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
        public string WG67_Version;             //Es el valor de la cabecera WG67-version

        public int NotifyReceived;              //Si es distinto de cero entonces la llamada a la callback se debe a la recepción de un Notify

        //Los siguientes campos solo son validos si se ha recibido un Notify. Es decir NotifyReceived es distinto de cero
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_REASON_LENGTH + 1)]
        public string Reason;                 /*< Optional termination reason. */
        public int Expires;                   /*< Expires param, or -1 if not present. */
        public int Retry_after;               /*< Retry after param, or -1 if not present. */

        public int Found_Parse_Errors;        //Si el valor es distinto de cero entonces indica que se han encontrado errores parseando.

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
        public string Fid;                    //Identificador de la frecuencia, en ED137C es obligatorio y en la ED137B siempre llegara vacio

        public uint SessionsCount;            //Numero de sesiones que contiene el Notify
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SipAgent.CORESIP_MAX_WG67_SUBSCRIBERS)]
        public CORESIP_WG67SessionsInfo[] SessionInfo;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_BODY_LEN)]
        public string RawBody;                //Es el cuerpo del NOTIFY sin parsear  
    }

    //Estructura que define el cuerpo de los Notify al evento WG67KEY-IN
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_WG67Notify_Body_Config
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CORESIP_WG67NotifySessionsInfo
        {
            public uint ptt_id;                         //Un valor numérico con el ptt-id. 

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH)]
            public string sip_from_uri;                 //Uri del tipo sip:user@host:port. cadena acabada en '\0'

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH)]
            public string call_type;                    //Posibles valores "coupling", "Radio-Rxonly", "Radio-TxRx", "Radio-Idle". cadena acabada en '\0'
        }

        public int exclude_real_sessions;               //Si el valor es distinto de cero, entonces en el NOTIFY se excluyen las sesiones reales y 
                                                        //solo aparecen las definidas en esta estructura.
        public int num_sessions;                        //Número de sesiones del array SessionsInfo

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SipAgent.CORESIP_MAX_WG67_NOTIFY_SESSIONS)]
        public CORESIP_WG67NotifySessionsInfo[] SessionsInfo;
    }

    //Estructura que define la cabecera Subscription-State y Expires.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class CORESIP_WG67Notify_SubscriptionState_Config
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH)]
        public string subscription_state;               //Es obligatorio y puede tener los valores: "pending", "active", "terminated". Cadena terminada en cero.

        public int expires;                             //Valor en segundos del tiempo en que expira la subscripcion. 
                                                        //Es opcional, si es negativo se ignora. Con estado "terminated" tambien se ignora.

        public int retry_after;                         //Valor en segundos del tiempo durante el cual no se permite una resubscripcion. 
                                                        //Es opcional, si es negativo se ignora. Con un estado distinto de "terminated" tambien se ignora.

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH)]
        public string reason;                           //Es opcional y puede ser de longitud cero. Puede tener uno de estos valores. (Cadena terminada en cero):
                                                        // "deactivated", "probation", "rejected", "timeout", "giveup", "noresource"
                                                        // Se explica en RFC3265, apartado 3.2.4
    }
    

    /// <summary>
    /// 
    /// </summary>
    public static partial class SipAgent
    {
        public const int SIP_TRYING = 100;
        public const int SIP_RINGING = 180;
        public const int SIP_QUEUED = 182;
        public const int SIP_INTRUSION_IN_PROGRESS = 183;
        public const int SIP_INTERRUPTION_IN_PROGRESS = 184;
        public const int SIP_INTERRUPTION_END = 185;
        public const int SIP_OK = 200;
        public const int SIP_ACCEPTED = 202;
        public const int SIP_MOVED_TEMPORARILY = 302;
        public const int SIP_BAD_REQUEST = 400;
        public const int SIP_NOT_FOUND = 404;
        public const int SIP_NOT_ACCEPTABLE = 406;
        public const int SIP_REQUEST_TIMEOUT = 408;
        public const int SIP_GONE = 410;
        public const int SIP_TEMPORARILY_UNAVAILABLE = 480;
        public const int SIP_LOOP_DETECTED = 482;
        public const int SIP_BUSY = 486;
        public const int SIP_NOT_ACCEPTABLE_HERE = 488;
        public const int SIP_ERROR = 500;
        public const int SIP_CONGESTION = 503;
        public const int SIP_SERVER_TIMEOUT = 504;
        public const int SIP_DECLINE = 603;
        public const int SIP_UNWANTED = 607;

        /** 20180717. Para los codigos BYE de las sesiones RADIO */
        public const int WG67Reason_KATimeout = 2001;

        #region Dll Interface

        public const int CORESIP_MAX_USER_ID_LENGTH = 100;
        public const int CORESIP_MAX_FILE_PATH_LENGTH = 256;
        public const int CORESIP_MAX_ERROR_INFO_LENGTH = 512;
        public const int CORESIP_MAX_HOSTID_LENGTH = 32;
        public const int CORESIP_MAX_IP_LENGTH = 25;
        public const int CORESIP_MAX_URI_LENGTH = 256;
        public const int CORESIP_MAX_TAG_LENGTH = 256;
        public const int CORESIP_MAX_SOUND_DEVICES = 20;
        public const int CORESIP_MAX_WAV_PLAYERS = 50;
        public const int CORESIP_MAX_WAV_RECORDERS = 50;
        public const int CORESIP_MAX_RDRX_PORTS = 128;
        public const int CORESIP_MAX_SOUND_RX_PORTS = 128;
        public const int CORESIP_MAX_GENERIC_PORTS = 16;
        public const int CORESIP_MAX_RS_LENGTH = 128;
        public const int CORESIP_MAX_REASON_LENGTH = 128;
        public const int CORESIP_MAX_WG67_SUBSCRIBERS = 25;
        public const int CORESIP_MAX_WG67_NOTIFY_SESSIONS = 7;
        public const int CORESIP_MAX_CODEC_LENGTH = 50;
        public const int CORESIP_MAX_CONF_USERS = 25;
        public const int CORESIP_MAX_CONF_STATE_LENGTH = 25;
        public const int CORESIP_MAX_SLOTSTOSNDPORTS = 50;
        public const int CORESIP_MAX_ZONA_LENGTH = 128;
        public const int CORESIP_MAX_BSS_LENGTH = 32;
        public const int CORESIP_MAX_SUPPORTED_LENGTH = 512;
        public const int CORESIP_MAX_NAME_LENGTH = 20;
        public const int CORESIP_MAX_CALLID_LENGTH = 256;
        public const int CORESIP_MAX_RSSI_QIDX = 15;            //Valor maximo de QIDX RSSI
        public const int CORESIP_MAX_QIDX = 31;                 //Valor maximo de other QIDX 
        public const int CORESIP_MAX_ATTENUATION_DB = 100;
        public const int CORESIP_MAX_BODY_LEN = 1024;
        public const int CORESIP_MAX_SESSTYPE_LENGTH = 32;
        public const int CORESIP_MAX_SELCAL_LENGTH = 4;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CORESIP_Error
        {
            public int Code;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_FILE_PATH_LENGTH + 1)]
            public string File;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_ERROR_INFO_LENGTH + 1)]
            public string Info;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class CORESIP_SndDeviceInfo
        {
            public CORESIP_SndDevType Type;
            public int OsInDeviceIndex;
            public int OsOutDeviceIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class CORESIP_RdRxPortInfo
        {
            public uint ClkRate;
            public uint ChannelCount;
            public uint BitsPerSample;
            public uint FrameTime;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_IP_LENGTH + 1)]
            public string Ip;
            public uint Port;
        }        

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class CORESIP_Config
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_HOSTID_LENGTH + 1)]
            public string HostId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_IP_LENGTH + 1)]
            public string IpAddress;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_USER_ID_LENGTH + 1)]
            public string UserAgent;            //Nombre del agente SIP. Si es un string de longitud cero 
                                                //entonces se usa el de por defecto que es "U5K-UA/1.0.0". Por tanto en ULISES no se usa para que se quede un string de long cero
            public uint Port;                   //Puerto SIP
            public uint RtpPorts;               //Valor por el que empiezan a crearse los puertos RTP

            public uint UseDefaultSoundDevices; //Si es distinto de cero entonces se utilizan los dispositivos de microfono y altavoz 
                                                //por defecto en el sistema automáticamente, sin que lo tenga que manejar en la aplicacion.


            public CORESIP_Callbacks Cb;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_CODEC_LENGTH + 1)]
            public string DefaultCodec;
            public uint DefaultDelayBufPframes;
            public uint DefaultJBufPframes;
            public uint SndSamplingRate;
            public float RxLevel;
            public float TxLevel;
            public uint LogLevel;

            public uint TsxTout;
            public uint InvProceedingIaTout;
            public uint InvProceedingMonitoringTout;
            public uint InvProceedingDiaTout;
            public uint InvProceedingRdTout;

            /* AGL 20131121. Variables para la configuracion del Cancelador de Eco */
            public uint EchoTail;
            public uint EchoLatency;
            /* FM */

            /// <summary>
            /// JCAM 18/01/2016
            /// Grabación según norma ED-137
            /// </summary>
            public uint RecordingEd137;

            public uint max_calls;      //Máximo número de llamadas que soporta el agente
            public uint Radio_UA;       //Con valor distinto de 0, indica que se comporta como un agente de radio

            /// <summary>
            /// Tiempo para inhibir el envio a Nodebox de eventos de RdInfo tras PTT off propio
            /// Evita que el nodebox reciba falsos SQ de avion provocados por PTT propio
            /// </summary>
            public uint TimeToDiscardRdInfo;

            public uint DIA_TxAttenuation_dB;       //Atenuacion de las llamadas DIA en Tx (Antes de transmistir por RTP). En dB
            public uint IA_TxAttenuation_dB;        //Atenuacion de las llamadas IA en Tx (Antes de transmistir por RTP). En dB
            public uint RD_TxAttenuation_dB;        //Atenuacion del Audio que se transmite hacia el multicas al hacer PTT en el HMI. En dB
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class CORESIP_Impairments   //Necesario para ETM
        {
            public int Perdidos;
            public int Duplicados;
            public int LatMin;
            public int LatMax;
        }

        public enum CORESIP_RecCmdType : int
        {
            CORESIP_REC_RESET = 0, // Resetea el servicio de grabacion
            CORESIP_REC_ENABLE = 1, //Activa la grabacion
            CORESIP_REC_DISABLE = 2//Desactiva la grabacion
        };

        public const uint CORESIP_CALL_ID = 0x40000000;
        public const uint CORESIP_SNDDEV_ID = 0x20000000;
        public const uint CORESIP_WAVPLAYER_ID = 0x10000000;
        public const uint CORESIP_RDRXPORT_ID = 0x08000000;
        public const uint CORESIP_SNDRXPORT_ID = 0x04000000;
        public const uint CORESIP_ACC_ID = 0x02000000;
        public const uint CORESIP_WAVRECORDER_ID = 0x01000000;

        public const uint CORESIP_ID_TYPE_MASK = 0xFF800000;
        public const uint CORESIP_ID_MASK = 0x007FFFFF;

        public const string coresip = "coresip-voter";

        /**
            *	CORESIP_Init Rutina de Inicializacion del Modulo. @ref SipAgent::Init
            *	@param	cfg		Puntero @ref CORESIP_Config a la configuracion.
            *	@param	error	Puntero @ref CORESIP_Error a la estructura de Error
            *	@return			Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Init([In] CORESIP_Config info, out CORESIP_Error error);

        /**
        *	CORESIP_Start Rutina de Arranque del Modulo. @ref SipAgent::Start
        *	@param	error	Puntero a la estructura de Error. @ref CORESIP_Error
        *	@return			Codigo de Error
        */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Start(out CORESIP_Error error);

        /**
        *	CORESIP_End Rutina de Parada del Modulo. @ref SipAgent::Stop
        *	@return			Sin Retorno
        */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern void CORESIP_End();

        /**
        *	CORESIP_Set_Ed137_version Establece la version de la ED137. El agente arranca por defecto con la ED137B
        *	@param	ED137Radioversion	Version para radio. Vale 'B' para ED137B y 'C' para ED137C
        *	@param	ED137Phoneversion	Version para telefonia. Vale 'B' para ED137B y 'C' para ED137C
        *	@param	error	Puntero a la estructura de Error. @ref CORESIP_Error
        *	@return			Codigo de Error
        */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Set_Ed137_version(char ED137Radioversion, char ED137Phoneversion, out CORESIP_Error error);

        /**
        *	CORESIP_Get_Ed137_version Obtiene la version de la ED137.
        *	@param	ED137Radioversion	Se retorna un caracter con la Version del agente para radio. Vale 'B' para ED137B y 'C' para ED137C
        *	@param	ED137Phoneversion	Se retorna un caracter con la Version del agente para telefonia. Vale 'B' para ED137B y 'C' para ED137C
        *	@param	error	Puntero a la estructura de Error. @ref CORESIP_Error
        *	@return			Codigo de Error
        */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Get_Ed137_version(out char ED137Radioversion, out char ED137Phoneversion, out CORESIP_Error error);

        /**
            *	CORESIP_SetLogLevel Establece el nivel de LOG del Modulo. @ref SipAgent::SetLogLevel
            *	@param	level	Nivel de LOG solicitado. Recomendable el valor 3
            *	@param	error	Puntero @ref CORESIP_Error a la estructura de Error.
            *	@return			Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetLogLevel(uint level, out CORESIP_Error error);

        /**
            * CORESIP_SetSipPort. Establece el puerto SIP
            * @param	port	Puerto SIP
            * @return			Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetSipPort(int port, out CORESIP_Error error);

        /**
            *	CORESIP_SetParams Establece los Parametros del Modulo. @ref SipAgent::SetParams
            *	@param	MaxForwards	Valor de la cabecera Max-Forwards. Si vale NULL se ignora.
            *	@param	Options_answer_code		Codigo de la respuesta a los mensajes OPTIONS (200, 404, etc.)
            *									Si el codigo es 0, entonces no se envia respuesta
            *									Si se pasa un NULL, este parametro se ignora.
            *	@param	error	Puntero @ref CORESIP_Error a la estructura de Error.
            *	@return			Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetParams([In] int MaxForwards, [In] int Options_answer_code, out CORESIP_Error error);

        /**
            *	CORESIP_SetJitterBuffer. Establece el buffer Jitter. Esta basado en el Jitter Buffer implementado en la libreria PJSIP
            *						Si nunca se llama a esta funcion, los valores del buffer son: 
                                adaptativo
                                initial_prefetch = 0
                                min_prefetch = 10 ms
                                max_prefetch = DefaultJBufPframes * 10ms * 4 / 5;
            * @param	adaptive	Si es distinto de cero, entonces el buffer es adaptativo, si es 0 entonces es fijo
            * @param	initial_prefetch Prefetch del buffer de jitter. En ms. 
            *						Si es adaptativo:	Precarga inicial aplicada al jitter buffer. 
            *											El prefetch es una funcion del jitter buffer que se aplica cada vez que queda vacio o desde el inicio cuando initial_prefetch no es 0.
                                                    Si el valor es mayor que 0 activara la precarga del jitter 
                                                    retorna un frame hasta que su longitud alcanza el numero de frames especificados en este parametro.
                                Si es fijo: The fixed delay value, in ms. Por ejemplo 40ms.
            * @param	min_prefetch	Para buffer adaptativo (en fijo se ignora): El minimo prefetch que se aplica, in ms. Ej: 10ms
            * @param	max_prefetch	Para buffer adaptativo (en fijo se ignora): El maximo prefetch que se aplica, in ms. Ej: 60 ms
            * @param	discard			Para buffer adaptativo (en fijo se ignora):
            *							Si su valor es distinto de cero, buffer descarta paquetes para minimizar el retardo progresivamente, incluso por debajo de min_prefetch.
            *							El prefetch es una funcion del jitter buffer que se aplica cada vez que queda vacio.
            *							Si su valor es cero entonces no se descarta ningun paquete.
            *							En cualquiera de los casos, cuando el buffer se llena se descarta un paquete.
            *	@param	error	Puntero @ref CORESIP_Error a la estructura de Error.
            *	@return			Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetJitterBuffer(uint adaptive, uint initial_prefetch, uint min_prefetch, uint max_prefetch, uint discard, out CORESIP_Error error);

        /**
            *	CORESIP_CreateAccount. Registra una cuenta SIP en el Módulo. @ref SipAgent::CreateAccount
            *	@param	acc			Puntero a la sip URI que se crea como agente.
            *	@param	defaultAcc	Marca si esta cuenta pasa a ser la Cuenta por Defecto.
            *	@param	accId		Puntero a el identificador de cuenta asociado.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateAccount([In] string acc, int defaultAcc, out int accId, out CORESIP_Error error);

        /**
            *	CORESIP_CreateAccountProxyRouting. Registra una cuenta SIP en el Módulo y los paquetes sip se enrutan por el proxy. @ref SipAgent::CreateAccount
            *	@param	acc			Puntero a la sip URI que se crea como agente.
            *	@param	defaultAcc	Marca si esta cuenta pasa a ser la Cuenta por Defecto.
            *	@pa ram	accId		Puntero a el identificador de cuenta asociado.
            *  @param	proxy_ip	Si es distinto de NULL. IP del proxy Donde se quieren enrutar los paquetes.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateAccountProxyRouting([In] string acc, int defaultAcc, out int accId, [In] string proxy_ip, out CORESIP_Error error);

        /**
            *	CreateAccountAndRegisterInProxy. Crea una cuenta y se registra en el SIP proxy. Los paquetes sip se rutean por el SIP proxy también.
            *	@param	acc			Puntero al Numero de Abonado (usuario). NO a la uri.
            *	@param	defaultAcc	Si es diferente a '0', indica que se creará la cuenta por Defecto.
            *	@param	accId		Puntero a el identificador de cuenta asociado que retorna.
            *	@param	proxy_ip	IP del proxy.
            *	@param	expire_seg  Tiempo en el que expira el registro en segundos.
            *	@param	username	Si no es necesario autenticación, este parametro será NULL
            *	@param  pass		Password. Si no es necesario autenticación, este parametro será NULL
            *	@param  DisplayName	Display name que va antes de la sip URI, se utiliza para como nombre a mostrar
            *	@param	isfocus		Si el valor es distinto de cero, indica que es Focus, para establecer llamadas multidestino
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateAccountAndRegisterInProxy([In] string acc, int defaultAcc, out int accId, string proxy_ip,
                                                                uint expire_seg, string username, string pass, string displayName, int isfocus, out CORESIP_Error error);

        /**
            *	CORESIP_DestroyAccount. Elimina una cuenta SIP del modulo. @ref SipAgent::DestroyAccount
            *	@param	accId		Identificador de la cuenta.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroyAccount(int accId, out CORESIP_Error error);

        /**
            *	CORESIP_AddSndDevice		Añade un dispositvo de audio al módulo. @ref SipAgent::AddSndDevice
            *	@param	info		Puntero @ref CORESIP_SndDeviceInfo a la Informacion asociada al dispositivo.
            *	@param	dev			Puntero donde se recorre el identificador del dispositivo.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_AddSndDevice([In] CORESIP_SndDeviceInfo info, out int dev, out CORESIP_Error error);

        /**
            *	CORESIP_CreateWavPlayer		Crea un 'Reproductor' WAV. @ref SipAgent::CreateWavPlayer
            *	@param	file		Puntero al path del fichero.
            *	@param	loop		Marca si se reproduce una sola vez o indefinidamente.
            *	@param	wavPlayer	Puntero donde se recorre el identificador del 'reproductor'.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateWavPlayer([In] string file, int loop, out int wavPlayer, out CORESIP_Error error);

        /**
            *	CORESIP_DestroyWavPlayer	Elimina un Reproductor WAV. @ref SipAgent::DestroyWavPlayer
            *	@param	wavPlayer	Identificador del Reproductor.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroyWavPlayer(int wavPlayer, out CORESIP_Error error);

        /**
            *	CORESIP_CreateWavRecorder	Crea un 'grabador' en formato WAV. @ref SipAgent::CreateWavRecorder
            *	@param	file		Puntero al path del fichero, donde guardar el sonido.
            *	@param	wavRecorder	Puntero donde se recoge el identificador del 'grabador'
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateWavRecorder([In] string file, out int wavPlayer, out CORESIP_Error error);

        /**
            *	CORESIP_DestroyWavRecorder	Elimina un 'grabador' WAV. @ref SipAgent::DestroyWavRecorder
            *	@param	wavRecorder	Identificador del Grabador.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroyWavRecorder(int wavPlayer, out CORESIP_Error error);

        /**
            *	CORESIP_CreateRdRxPort		Crea un 'PORT' @ref RdRxPort de Recepcion Radio. @ref SipAgent::CreateRdRxPort
            *	@param	info		Puntero @ref CORESIP_RdRxPortInfo a la informacion del puerto
            *	@param	localIp		Puntero a la Ip Local.
            *	@param	rdRxPort	Puntero que recoge el identificador del puerto.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateRdRxPort([In] CORESIP_RdRxPortInfo info, string localIp, out int rdRxPort, out CORESIP_Error error);

        /**
            *	CORESIP_DestroyRdRxPort		Elimina un Puerto @ref RdRxPort. @ref SipAgent::DestroyRdRxPort
            *	@param	rdRxPort	Identificador del Puerto.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroyRdRxPort(int rdRxPort, out CORESIP_Error error);

        /**
            *	CORESIP_CreateSndRxPort.	Crea un puerto @ref SoundRxPort. @ref SipAgent::CreateSndRxPort
            *	@param	id			Puntero al nombre del puerto.
            *	@param	sndRxPort	Puntero que recoge el identificador del puerto.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateSndRxPort(string id, out int sndRxPort, out CORESIP_Error error);

        /**
            *	CORESIP_DestroySndRxPort	Eliminar un puerto @ref SoundRxPort. 
            *	@param	sndRxPort	Identificador del puerto.
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroySndRxPort(int sndRxPort, out CORESIP_Error error);

        /**
            *	CORESIP_BridgeLink	Configura un enlace de conferencia. Conecta y desconecta puertos.
            *	@param	src			Tipo e Identificador de Puerto Origen. @ref CORESIP_ID_TYPE_MASK, @ref CORESIP_ID_MASK
            *	@param	dst			Tipo e Identificador de Puerto Destino. @ref CORESIP_ID_TYPE_MASK, @ref CORESIP_ID_MASK
            *	@param	on			Indica Conexión o Desconexión.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_BridgeLink(int src, int dst, int on, out CORESIP_Error error);

        /**
            *	CORESIP_SendToRemote		Configura El puerto de Sonido apuntado para los envios UNICAST de Audio. @ref SipAgent::SendToRemote
            *	@param	dev			...
            *	@param	on			...
            *	@param	id			Puntero a ...
            *	@param	ip			Puntero a ...
            *	@param	port		...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendToRemote(int dev, int on, string id, string ip, uint port, out CORESIP_Error error);

        /**
            *	CORESIP_ReceiveFromRemote
            *	@param	localIp		Puntero a ...
            *	@param	mcastIp		Puntero a ...
            *	@param	mcastPort	...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_ReceiveFromRemote(string localIp, string mcastIp, uint mcastPort, out CORESIP_Error error);

        /**
            *	CORESIP_SetVolume
            *	@param	id			...
            *	@param	volume		...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetVolume(int id, int volume, out CORESIP_Error error);

        /**
            *	CORESIP_GetVolume
            *	@param	id			...
            *	@param	volume		Puntero a ...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_GetVolume(int dev, out int volume, out CORESIP_Error error);

        /**
            *	CORESIP_CallMake
            *	@param	info		Puntero a la informacion de llamada
            *	@param	outInfo
            *	@param	call		Puntero a ...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallMake([In] CORESIP_CallInfo info, [In] CORESIP_CallOutInfo outInfo, out int call, out CORESIP_Error error);

        /**
            *	CORESIP_CallHangup
            *	@param	call		Identificador de Llamada
            *	@param	code		...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallHangup(int call, int code, out CORESIP_Error error);

        /**
            *	CORESIP_CallAnswer
            *	@param	call		Identificador de Llamada
            *	@param	code		...
            *	@param	addToConference		...
            *	@param	reason_code. Es el codigo del campo cause de la cabecera reason. En caso radio y el codigo esta entre 2000 y 2099 reason_text podria ser NULL porque se pone internamente.
            *						En el caso de que no se utilice este parametro entonces su valor debera ser cero
            *	@param	reason_text. Es el texto del campo text de la cabecera Reason. En caso de ser NULL no se incluira el campo text.
            *						Debe de ser un string acabado con el caracter cero. 
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallAnswer(int call, int code, int addToConference, int reason_code, string reason_text, out CORESIP_Error error);

        /**
            *	CORESIP_CallMovedTemporallyAnswer
            *	@param	call		Identificador de Llamada
            *	@param	dst			Uri del usuario al que la llamada es desviada
            *	@param	reason		Es la razon del desvio. Posibles valores "unconditional", "user-busy", etc.
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallMovedTemporallyAnswer(int call, [In] string dst, [In] string reason, out CORESIP_Error error);

        /**
            *	CORESIP_CallProccessRedirect
            *	Esta funcion debe llamarse despues de recibirse la callback MovedTemporallyCb para 
            *	aceptar o rechazar la redireccion de la llamada.
            *	@param	call		Identificador de Llamada
            *	@param  dstUri		Nueva request uri hacia donde se desvia la llamada. Terminado en '\0', Si se rechaza entonces este parametro se ignora.
            *	@param	op			Opcion (aceptar o rechazar)
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallProccessRedirect(int call, string dstUri, CORESIP_REDIRECT_OP op, out CORESIP_Error error);

        /**
            *	CORESIP_CallHold
            *	@param	call		Identificador de llamada
            *	@param	hold		...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallHold(int call, int hold, out CORESIP_Error error);

        /**
            *	CallReinvite
            *	@param	call		Identificador de llamada
            *	@param	error		Puntero a la Estructura de error
            *	@param	CallType_SDP	9 couplig, 7 Radio-Rxonly, 5 Radio-TxRx, 6 Radio-Idle
            *	@param	TxRx_SDP		4 Rx, 8 Tx, 0 TxRx, 22 Vacio
            *  @param	etm_vcs_bss_methods	Para ETM, como VCS, string con los literales de los metodos BSS separados por comas. El string debe terminar caracter '\0'. Si vale NULL se ignora
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallReinvite(int call, out CORESIP_Error error, int CallType_SDP, int TxRx_SDP, string etm_vcs_bss_methods);

        /**
            *	CORESIP_CallTransfer
            *	@param	call		Identificador de llamada
            *	@param	dstCall		...
            *	@param	dst			Puntero a ...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallTransfer(int call, int dstCall, [In] string dst, [In] string displayName, out CORESIP_Error error);

        /**
            *	CORESIP_CallPtt
            *	@param	call		Identificador de llamada
            *	@param	info		Puntero a la Informacion asociada al PTT
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallPtt(int call, [In] CORESIP_PttInfo info, out CORESIP_Error error);

        /**
            *	CORESIP_CallPtt_Delayed. Envía PTT retardado
            *	@param	call		Identificador de llamada
            *	@param	info		Puntero a la Informacion asociada al PTT
            *	@param	delay_ms	Tiempo del retardo en ms
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallPtt_Delayed(int call, [In] CORESIP_PttInfo info, uint delay_ms, out CORESIP_Error error);

        /**
	     *	CORESIP_GetRdQidx
	     *	@param	call		Identificador de llamada
	     *	@param	Qidx		Qidx del recurso de radio receptor que se retorna. Sera el manejado por el BSS.
	     *	@param	error		Puntero a la Estructura de error
	     *	@return				Codigo de Error
	     */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_GetRdQidx(int call, ref int Qidx, out CORESIP_Error error);

        /**
            *	CORESIP_CallConference
            *	@param	call		Identificador de llamada
            *	@param	conf		Identificador de conferencia
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallConference(int call, int conf, out CORESIP_Error error);

        /**
            *	CORESIP_CallSendConfInfo
            *	@param	call		Identificador de llamada
            *	@param	info		Puntero a ...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallSendConfInfo(int call, [In] CORESIP_ConfInfo info, out CORESIP_Error error);

        /**
            *	CORESIP_SendConfInfoFromAcc
            *	@param	accId		Identificador del account del agente
            *	@param	info		Puntero a ...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendConfInfoFromAcc(int accId, [In] CORESIP_ConfInfo info, out CORESIP_Error error);

        /**
            *	CORESIP_CallSendInfo. envia paquete SIP INFO
            *	@param	call		Identificador de llamada
            *	@param	info		Puntero a ...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CallSendInfo(int call, [In] string info, out CORESIP_Error error);

        /**
            *	CORESIP_TransferAnswer. Envia la respuesta al paquete SIP REFER utilizado para la transferencia
            *	@param	tsxKey		Puntero a ...
            *	@param	txData		Puntero a ...
            *	@param	evSub		Puntero a ...
            *	@param	code		...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_TransferAnswer(string tsxKey, IntPtr txData, IntPtr evSub, int code, out CORESIP_Error error);

        /**
            *	CORESIP_TransferNotify. Envia el SIP NOTIFY que se utiliza en las transferencias de llamadas
            *	@param	evSub		Puntero a ...
            *	@param	code		...
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_TransferNotify(IntPtr evSub, int code, out CORESIP_Error error);

        /**
            *	SendOptionsMsg
            *  Esta función no envia OPTIONS a traves del proxy
            *	@param	dst			Puntero a uri donde enviar OPTIONS
            *  @param	callid		callid que retorna.
            *  @param	isRadio		Si tiene valor distinto de cero el agente se identifica como radio. Si es cero, como telefonia.
            *						Sirve principalmente para poner radio.01 o phone.01 en la cabecera WG67-version
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendOptionsMsg([In] string dst, StringBuilder callid, int isRadio, out CORESIP_Error error);

        /**
            *	CORESIP_SendOptionsMsgProxy
            *  Esta función envia OPTIONS a traves del proxy
            *	@param	dst			Puntero a uri donde enviar OPTIONS
            *  @param	callid		callid que retorna.
            *  @param	isRadio		Si tiene valor distinto de cero el agente se identifica como radio. Si es cero, como telefonia.
            *						Sirve principalmente para poner radio.01 o phone.01 en la cabecera WG67-version
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendOptionsMsgProxy([In] string dst, StringBuilder callid, int isRadio, out CORESIP_Error error);
        //Envía OPTIONS pasando por el proxy

        /**
            * SendOptionsCFWD.	...
            * Envia mensaje OPTIONS necesario para la negociacion Call Forward
            * @param	accId				Account de la Coresip que utilizamos.
            * @param	dst					Uri a la que se envia OPTIONS
            * @param	cfwr_options_type	Tipo de OPTIONS para la negociacion. Es del tipo CORESIP_CFWR_OPT_TYPE
            * @param	body				Contenido del body (XML). Acabado en '\0'
            * @param	callid				callid que se retorna, acabado en '\0'.
            * @param	by_proxy			TRUE si queremos que se envie a través del proxy. Agregara cabecera route
            * @param	error		Puntero a la Estructura de error
            * @return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendOptionsCFWD(int accId, [In] string dst, CORESIP_CFWR_OPT_TYPE cfwr_options_type, [In] string body, StringBuilder callid, bool by_proxy, out CORESIP_Error error);

        /**
            * CORESIP_SendResponseCFWD.	...
            * Envia la respuesta al options utilizado para la negociacion de call forward
            * @param	st_code				Code de la respuesta. Si no es 200 entonces se ignora el parametro del body
            * @param	body				Contenido del body (XML). Acabado en '\0'
            * @param	hresp				Manejador necesario para enviar la respuesta
            * @param	error		Puntero a la Estructura de error
            * @return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendResponseCFWD(int st_code, [In] string body, uint hresp, out CORESIP_Error error);

        /** AGL */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WavRemoteEnd(IntPtr obj);
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Wav2RemoteStart([In] string file, string id, string ip, int port, ref WavRemoteEnd cbend, ref CORESIP_Error error);
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Wav2RemoteEnd(IntPtr obj, ref CORESIP_Error error);

        /* GRABACION VOIP START */
        /**
            *	CORESIP_RdPttEvent. Se llama cuando hay un evento de PTT en el HMI. Sirve sobretodo para enviar los metadata de grabacion VoIP en el puesto
            *  @param  on			true=ON/false=OFF
            *	@param	freqId		Identificador de la frecuencia
            *  @param  dev			Indice del array _SndPorts. Es dispositivo (microfono) fuente del audio.
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_RdPttEvent(bool on, [In] string freqId, int dev, out CORESIP_Error error, CORESIP_PttType priority);

        /**
            *	CORESIP_RdSquEvent. Se llama cuando hay un evento de Squelch en el HMI. Sirve sobretodo para enviar los metadata de grabacion VoIP en el puesto
            *  @param  on			true=ON/false=OFF
            *	@param	freqId		Identificador de la frecuencia
            *	@param	resourceId  Identificador del recurso seleccionado en el bss
            *	@param	bssMethod	Método bss
            *	@param  bssQidx		Indice de calidad
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_RdSquEvent(bool on, [In] string freqId, [In] string resourceId, [In] string bssMethod, uint bssQidx, out CORESIP_Error error);

        //Metodo para enviar un comando al grabador
        /**
            *	CORESIP_RecorderCmd. Se pasan comandos para realizar acciones sobre el grabador VoIP
            *  @param  cmd			Comando
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_RecorderCmd(CORESIP_RecCmdType cmd, out CORESIP_Error error);
        /* GRABACION VOIP END */

        /*Funciones para gestion de presencia por subscripcion al evento de presencia*/
        /**
            *	CORESIP_CreatePresenceSubscription. Crea una subscripcion por evento de presencia
            *  @param  dest_uri.	Uri del destino al que nos subscribimos
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreatePresenceSubscription(string dest_uri, out CORESIP_Error error);

        /**
            *	CORESIP_DestroyPresenceSubscription. destruye una subscripcion por evento de presencia
            *  @param  dest_uri.	Uri del destino al que nos desuscribimos
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroyPresenceSubscription(string dest_uri, out CORESIP_Error error);

        /*Funciones para gestion de subscripcion al evento de conferencia*/
        /**
            *	CORESIP_CreateConferenceSubscription. Crea una subscripcion por evento de conferencia
            *	@param	accId		Identificador del account.
            *  @param  dest_uri.	Uri del destino a monitorizar
            *  @param	by_proxy.   Si true entonces el subscribe se envia a traves del proxy
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateConferenceSubscription(int accId, string dest_uri, bool byProxy, out CORESIP_Error error);

        /**
            *	CORESIP_DestroyConferenceSubscription. Destruye una subscripcion por evento de presencia
            *  @param  dest_uri.	Uri del destino a monitorizar
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroyConferenceSubscription(string dest_uri, out CORESIP_Error error);

        /*Funciones para gestion de subscripcion al evento de dialogo*/
        /**
            *	CORESIP_CreateDialogSubscription. Crea una subscripcion por evento de dialogo
            *	@param	accId		Identificador del account.
            *  @param  dest_uri.	Uri del destino a monitorizar
            *  @param	by_proxy.   Si true entonces el subscribe se envia a traves del proxy
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_CreateDialogSubscription(int accId, string dest_uri, bool byProxy, out CORESIP_Error error);

        /**
            *	CORESIP_DestroyDialogSubscription. Destruye una subscripcion por evento de dialogo
            *  @param  dest_uri.	Uri del destino a monitorizar
            *	@param	error		Puntero a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_DestroyDialogSubscription(string dest_uri, out CORESIP_Error error);

        /**
	     *	CORESIP_SendWG67Subscription. Crea y envia una subscripcion por evento WG67KE-IN. 
	     *	Una vez creada la suscripcion se puede enviar un nuevo request de la susbcripcion llamando a esta funcion, la suscripcion se identifica con:
			    acc_id + dest_uri. 
	     *	@param	accId		Identificador del account. Si es -1, se utiliza la default
	     *  @param  dest_uri.	Uri del destino GRS al que nos subscribimos. Si callId es disinto de -1 entonces este parametro se ignora y puede ser NULL.
	     *	@param	expires.	Valor del expires. Si vale -1 entonces toma el valor por defecto, si vale 0 entonces se terminará la subscripcion
	     *	@param	noRefresh	Si es 1 entonces la suscripcion no se refresca automaticamente a partir del momento en que se envia. Si es 0 entonces si refresca
	     *	@param	error		Puntero a la Estructura de error
	     *	@return				CORESIP_OK si no hay error.
	     */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendWG67Subscription(int accId, string dest_uri, int expires, int noRefresh, out CORESIP_Error error);

        /**
	     *	CORESIP_SetWG67SubscriptionParameters. Modifica parametros de la subscripcion por evento WG67KE-IN. 
	     *						Esta funcion no puede llamarse si previamente no se ha llamado a CORESIP_SendWG67Subscription
	     *	Una vez creada la suscripcion se puede enviar un nuevo request de la susbcripcion llamando a esta funcion, la suscripcion se identifica con:
			    acc_id + dest_uri. 
	     *	@param	accId		Identificador del account. Si es -1, se utiliza la default
	     *  @param  dest_uri.	Uri del destino GRS al que nos subscribimos. 
	     *						Debe ser el mismo parametro que se utilizo con CORESIP_SendWG67Subscription
	     *	@param	noRefresh	Si es 1 entonces la suscripcion no se refresca automaticamente. Si es 0 entonces si refresca. Si es -1 entonces no tiene efecto
	     *	@param	error		Puntero a la Estructura de error
	     *	@return				CORESIP_OK si no hay error.
	     */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetWG67SubscriptionParameters(int accId, string dest_uri, int noRefresh, out CORESIP_Error error);

        /**
	     *	CORESIP_Set_WG67_Notifier_Parameters. Configura algunos parametros del WG67 notifier
	     *  @param	notify_enabled. Si es 1 entonces los Notify son aceptados, 0 son rechazados, con -1 este parametro se ignora.
	     *	@param	manual_notify. Si es 1 entonces las notificaciones son manuales, 0 automatico, -1 no cambia. Si no se llama a esta función, por defecto los notify se envía automáticamente
	     *  @param  minimum_expires. Tiempo minimo de expires soportado. Si el valor es -1, entonces no tiene efecto. Si el subscriptor envia un valor menor entonces se rechaza
 			    y hay ue enviar cabecera con el minimo expires soportado. El valor mínimo de este parámetro es de 30.
	     *  @param  maximum_expires. Tiempo maximo de expires soportado. Si el valor es -1, entonces no tiene efecto. Si el subscriptor envia un valor mayor al valor de este 
			    parametro entonces en el 200 OK envia este valor.
	     *	@param error. Si hay error contiene la descripcion
	     *	@return	CORESIP_OK si no hay error.
	     */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Set_WG67_Notifier_Parameters(int notify_enabled, int manual, int minimum_expires, int maximum_expires, out CORESIP_Error error);

        /**
            * CORESIP_Set_WG67_notify_status: Establece el estado de la suscripcion y las sesiones ficticias, ademas de enviar el Notify correspondiente.
            * Esta funcion puede ser llamada en la callback WG67SubscriptionReceivedCb para establecer el estado inicial de la suscripcion
            * La función CORESIP_Set_WG67_notify_status establece el estado de una subscripción (si el parámetro subscriberUri no es NULL), o de todas
            las subscripciones activas o futuras (si el parámetro subscriberUri es NULL).
            Es decir, que si se quiere establecer las sesiones y estados ficticios para todas las subscripciones activas o las futuras,
            se llamaría a esta función con el parámetro subscriberUri a NULL. En este caso, para las suscripciones activas se envía en ese momento un 
            NOTIFY. Y para las nuevas subscripciones el primer NOTIFY ya lleva las sesiones configuradas con esta función.

            En el caso de que el parametro subscriberUri no sea NULL, afecta solo al subscriptor de esa uri. Y esta funcion se podria llamar dentro 
            de la callback WG67SubscriptionReceivedCb para que el primero notify de esa subscripción concreta ya lleva las funciones ficticias.
            Y si se llama cuando la suscripción ya está activa entonces se envía un NOTIFY.
            * @param	accId		Identificador del account. Si es -1, se utiliza la default
            * @param	subscriberUri.	Uri del suscriptor recibido en la callback WG67SubscriptionReceivedCb. Si es NULL establece el estado y envia notify a todos los subscriptores.
            * @param	subsState. Establece el estado de la subscripcion. Puede valer NULL si no queremos modificar el estado.
            *						Si el campo subscription_state de la estructura tiene longitud cero, entonces tampoco se modifica el estado.
            * @param	wG67Notify_Body. Configura el body (las sesiones activas) que se envia en los NOTIFY. Puede ser NULL si no queremos modificar la lista de sesiones
            *	@param error. Si hay error contiene la descripcion
            *	@return	CORESIP_OK si no hay error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Set_WG67_notify_status(int accId, string subscriberUri, [In] CORESIP_WG67Notify_SubscriptionState_Config subsState,
            [In] CORESIP_WG67Notify_Body_Config wG67Notify_Body, out CORESIP_Error error);

        /**
            * CORESIP_Get_GRS_WG67SubscriptionsList: Retorna las subscripciones al evento WG67 en el GRS
            * @param	accId.	Identificador del account. Si es -1, se utiliza la default
            * @param	nSubscriptions. Retorna el número de subscripciones.
            * @param   WG67Subscriptions. Se retorna un puntero a un array de elementos del tipo CORESIP_WG67_Subscription_Info. Si es NULL entonces no hay subscripciones	     *          
            * OJO! Esta funcion esta sin probar en C#, pego aqui la funcion prototipo de C/C++ 
            * CORESIP_API int CORESIP_Get_GRS_WG67SubscriptionsList(int accId, int* nSubscriptions, CORESIP_WG67_Subscription_Info* WG67Subscriptions[], CORESIP_Error* error);
            * @return	CORESIP_OK si no hay error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        //static extern int CORESIP_Get_GRS_WG67SubscriptionsList(int accId, out int nSubscriptions, [Out] CORESIP_WG67_Subscription_Info[] WG67Subscriptions, out CORESIP_Error error);
        public static extern int CORESIP_Get_GRS_WG67SubscriptionsList(int accId, out int nSubscriptions, ref IntPtr WG67Subscriptions, out CORESIP_Error error);

        /**
            * CORESIP_SendInstantMessage. Envia un mensaje instantaneo
            *
            * @param	acc_id		Account ID to be used to send the request.
            * @param	dest_uri	Uri del destino del mensaje. Acabado en 0.
            * @param	text		Texto plano a enviar. Acabado en 0
            * @param	by_proxy	Si es true el mensaje se envia por el proxy
            * @return	Codigo de Error
            *
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendInstantMessage(int acc_id, string dest_uri, string text, bool by_proxy, out CORESIP_Error error);

        /**
            * CORESIP_EchoCancellerLCMic.	...
            * Activa/desactiva cancelador de eco altavoz LC y Microfonos. Sirve para el modo manos libres 
            * Por defecto esta desactivado en la coresip
            * @param	on						true - activa / false - desactiva
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_EchoCancellerLCMic(bool on, out CORESIP_Error error);

        /**
	     *	CORESIP_SetTipoGRS. Configura el tipo de GRS. El ETM lo llama cuando crea un account tipo GRS.
	     *	@param	accId		Identificador de la cuenta.
	     *	@param	FlagGRS	Tipo de GRS.
	     *	@param	on			Indicamos que este account es de una radio GRS
	     *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
	     *	@return				Codigo de Error
	     */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetTipoGRS(int accId, uint Flag, int on, CORESIP_Error error);

        /**
            *	CORESIP_SetGRSParams. Configura parametros del GRS
            *	@param	accId		Identificador de la cuenta. Si es -1 entonces se utiliza la default.
            *	@param	RdFr		Frecuencia del GRS (fid). String acabado en '\0'. Con NULL se ignora
            *	@param	Tj1			Jitter buffer delay en milisegundos. Con NULL se ignora.
            *	@param	Ts1			GRS System Delay. En milisegundos. Con NULL se ignora
            *  @param	Ts2			GRS system RX delay (including packetizing time). En milisegundos. Con NULL se ignora
            *	@param	preferred_grs_bss_method   Metodo BSS preferido. Si en la lista de metodo recibido del VCS aparece entonces se selecciona, 
                                            si no entonces se selecciona "RSSI". Es un string terminado com caracter cero o si es NULL se ignora este parametro
                                            Su longitud maxima es CORESIP_MAX_BSS_LENGTH
            *	@param  preferred_grs_bss_method_code		Si #preferred_grs_bss_method no es "RSSI", "AGC", "C/N" ni "PSD", 
                                                        este parametro es el valor del codigo del Vendor specific method. Debera ser entre 4 y 7.
                                                        Si es NULL se ignora este parametro
            *	@param  forced_pttid	Si es NULL se ignora este parametro. Si el valor es -1, entonces el GRS asigna automaticamente el ptt-id.
                                    Si el valor es distinto de -1 entonces es el valor de ptt-id que se fuerza cuando se establece una sesion.
                                    Si el valor es cero entonces no aparece el atribueto ptt-id.
            *	@param	selcal_supported	Si es NULL se ignore. Si el valor es 1 entonces el GRS soporta SELCAL, y vale 0 no.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetGRSParams(int accId, string RdFr, [In] int Tj1, [In] int Ts1, [In] int Ts2,
            string preferred_grs_bss_method, [In] int preferred_grs_bss_method_code,
            [In] int forced_ptt_id, [In] int selcal_supported, out CORESIP_Error error);

        /**
            *	CORESIP_GRS_Force_Ptt_Mute. Como GRS Fuerza PTT mute en R2S Keepalives hacia VCS. Sirve para simular un PTT mute de otra sesion inventada.
            *	@param	call		Identificador de la llamada/sesion SIP
            *	@param	PttType		Tipo de PTT. PTT que activa el Ptt mute
            *	@param	PttId		Ptt ID. PTT id del ptt que activa el mute.
            *	@param	on			on. Si true lo activa, si false lo desactiva y los keepalives son los normales. 
            *						En caso de false se ignoran los parametros anteriores excepto el call.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_GRS_Force_Ptt_Mute(int call, CORESIP_PttType PttType, ushort PttId, bool on, out CORESIP_Error error);

        /**
            *	CORESIP_GRS_Force_Ptt. Como GRS Fuerza PTT en R2S Keepalives hacia VCS. Sirve para simular un PTT de otra sesion inventada.
            *	@param	call		Identificador de la llamada/sesion SIP
            *	@param	PttType		Tipo de PTT. PTT que activa el Ptt mute
            *	@param	PttId		Ptt ID. PTT id del ptt que activa el mute.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_GRS_Force_Ptt(int call, CORESIP_PttType PttType, ushort PttId, out CORESIP_Error error);

        /**
            *	CORESIP_GRS_Force_SCT. Como GRS Fuerza el bit SCT en el RTPRx enviado desde un GRS
            *	@param	call		Identificador de la llamada/sesion SIP
            *	@param	on			on. Si true lo activa, si false lo desactiva.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_GRS_Force_SCT(int call, bool on, out CORESIP_Error error);

        /**
            *	CORESIP_Force_PTTS. Fuerza el bit PTTS en el RTPRx o RTPTx
            *	@param	call		Identificador de la llamada/sesion SIP
            *	@param	on			on. Si true lo activa, si false lo desactiva.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_Force_PTTS(int call, bool on, out CORESIP_Error error);


        /**
            * CORESIP_SetImpairments.	...
            * Funcion para activar inperfecciones en la señal de audio que se envia por rtp 
            * @param	call          Call id de la llamada
            * @param	impairments   Define las inperfecciones que queremos que se ejecuten
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetImpairments(int call, [In] CORESIP_Impairments impairments, CORESIP_Error error);

        /**
            *	CORESIP_SetCallParameters. Configura parametros para una sesion SIP activa.
            *	@param	call		Call Id que identifica la llamada
            *	@param	disableKeepAlives. Si vale 1 los Keepalives dejan de enviarse. con valor 0 se envian. Si el puntero es NULL se ignora.
            *	@param	forced_cld. Valor forzado del CLD en ms. Si el valor es negativo, entonces se envia el calculado (Tn1 en el caso del ETM). Si el puntero es NULL se ignora.
            *	@param	error		Puntero @ref CORESIP_Error a la Estructura de error
            *	@return				Codigo de Error
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetCallParameters(int call, [In] int disableKeepAlives, [In] int forced_cld, out CORESIP_Error error);

        /**
            * CORESIP_SetConfirmPtt.	...
            * Activa/desactiva la confirmacion de PTT cuando es un agente de radio
            * Por defecto esta activado en la CORESIP
            * @param	on						true - activa / false - desactiva
            * @param	error		Puntero @ref CORESIP_Error a la Estructura de error
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetConfirmPtt(int call, bool val, out CORESIP_Error error);

        /**
            * CORESIP_GetNetworkDelay.	...
            * Retorna el retardo de red para una llamada VoIP
            * @param	call		Call Id que identifica la llamada
            * @param	delay_ms	Retardo en ms
            * @param	error		Puntero @ref CORESIP_Error a la Estructura de error
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_GetNetworkDelay(int call, [In] uint delay_ms, out CORESIP_Error error);

        /**
            * CORESIP_SendToneToCall
            * Envia un tono por una llamada establecida. Si se llama a esta funcion varias veces para la misma llamada los tonos se suman.
            * @param	call		Call Id que identifica la llamada
            * @param	frequency	Frecuencia en Hz. Si vale 0 entonces se aplica a todos los tonos activos. 
            * @param	volume_dbm0		en dBm0. Rango valores (-60 a +3.14)
            * @param	on			Si vale 1 el tono se emite, si vale 0 el tono deja de emitirse
            * @param	error		Puntero @ref CORESIP_Error a la Estructura de error
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendToneToCall(int call, [In] uint frequency, float volume_dbm0, int on, out CORESIP_Error error);

        /**
            * CORESIP_SendNoiseToCall
            * Envia un ruido blanco por una llamada establecida. 
            * @param	call		Call Id que identifica la llamada
            * @param	volume_dbm0		en dBm0. Rango valores (-60 a +3.14)
            * @param	on			Si vale 1 el tono se emite, si vale 0 el tono deja de emitirse
            * @param	error		Puntero @ref CORESIP_Error a la Estructura de error
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendNoiseToCall(int call, float volume_dbm0, int on, out CORESIP_Error error);

        /**
            * CORESIP_SendNoiseToCall
            * Envia un ruido rosa por una llamada establecida.
            * @param	call		Call Id que identifica la llamada
            * @param	volume_dbm0		en dBm0. Rango valores (-60 a +3.14)
            * @param	on			Si vale 1 el tono se emite, si vale 0 el tono deja de emitirse
            * @param	error		Puntero @ref CORESIP_Error a la Estructura de error
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendPinkNoiseToCall(int call, float volume_dbm0, int on, out CORESIP_Error error);

        /**
            * CORESIP_SendDTMF
            * Envia secuencia de digitos DTMF
            * @param	call		Call Id que identifica la llamada
            * @param	digit_map	Definicion de los digitos.
            * @param	count		Numero de digitos
            * @param	tones		Array con la secuencia de digitos.
            * @param	error		Puntero @ref CORESIP_Error a la Estructura de error
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendDTMF(int call, [In] CORESIP_tone_digit_map digit_map, uint count, CORESIP_tone_digit[] digits, float volume_dbm0, out CORESIP_Error error);

        /**
            * CORESIP_SendSELCAL
            * Envia secuencia SELCAL
            * @param	call		Call Id que identifica la llamada
            * @param	selcalValue	string con los identificadores de los tonos. La longitud debe ser #CORESIP_MAX_SELCAL_LENGTH
            * @param	error		Puntero @ref CORESIP_Error a la Estructura de error
            * @return	CORESIP_OK OK, CORESIP_ERROR  error.
            */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SendSELCAL(int call, string selcalValue, out CORESIP_Error error);

        public const int CORESIP_MAX_SOUND_NAME_LENGTH = 512;
        public const int CORESIP_MAX_SOUND_NAMES = 16;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CORESIP_SndWindowsDevices
        {
            public uint ndevices_found;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_SOUND_NAME_LENGTH * CORESIP_MAX_SOUND_NAMES)]
            public string DeviceNames; //array con los nombres, separados por '<###>'.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_SOUND_NAME_LENGTH * CORESIP_MAX_SOUND_NAMES)]
            public string FriendlyName; //array con los nombres, separados por '<###>'.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_SOUND_NAME_LENGTH * CORESIP_MAX_SOUND_NAMES)]
            public string GUID; //array con los nombres, separados por '<###>'.
        }

        /*
	    Funcion que retorna los dispositivos de sonido en Windows (no en asio)
	    @param captureType. Si vale distinto de cero retorna los de tipo entrada (capture), si vale cero los de tipo salida (play)
	    @param Devices. Retorna la cantidad La lista de dispositivos encontrados.	 
	    @return	CORESIP_OK OK, CORESIP_ERROR  error.
	    */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_GetWindowsSoundDeviceNames(int captureType, out CORESIP_SndWindowsDevices Devices, out CORESIP_Error error);

        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetSNDDeviceWindowsName(CORESIP_SndDevType UlisesDev, string DevWinName, out CORESIP_Error error);

        /*
	    Funcion que establece volumen de un dispositivo de salida
	    @param dev. dispositivo. 
	    @param volume. Valor entre MinVolume y MaxVolume de HMI.exe.config
	    */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern int CORESIP_SetVolumeOutputDevice(CORESIP_SndDevType dev, uint volume, out CORESIP_Error error);

        #endregion

    }
}
