using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using Utilities;

/**
* @author luisangel.lopez
*
* @date - 24/02/2022 16:54:10 
*/
namespace HMI.Model.Module.BusinessEntities
{
    public class PlayState
    {
        public enum estados
        {
            Deshabilitado,
            Reposo,
            Repoduciendo,
            Error,
            Oculto,
            Visto,
        };
        
        private bool _FileRecorded;
        public bool FileRecorded
        {
            get { return _FileRecorded; }
            set
            {
                if (FileRecorded != value)
                {
                    FileRecorded = value;

                    //General.SafeLaunchEvent(PlayStateChanged, this);
                }
            }
        }

        private bool _estadojacks;
        public bool EstadoJacks
        {
            get { return _estadojacks; }
            set
            {
                if (_estadojacks != value)
                {
                    _estadojacks = value;

                    // General.SafeLaunchEvent(SpeakerChanged, this);
                }
            }
        }

        public bool Estado
        {
            get { return (EstadoJacks && FileRecorded); }
        }
    }
}
