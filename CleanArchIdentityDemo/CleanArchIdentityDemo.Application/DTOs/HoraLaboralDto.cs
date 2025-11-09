using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchIdentityDemo.Application.DTOs
{
    public class HoraLaboralDto
    {
        public int IdHoraLaboral { get; set; }

        public int PersonalProyectoId { get; set; }

        public DateTime FechaTrabajo { get; set; }

        public DateTime HoraEntrada { get; set; }

        public DateTime HoraSalida { get; set; }

        public string NombrePersonal { get; set; } = string.Empty;

        // Campos solo para el reporte
        public int DiasAsistidos { get; set; }

        public int EntradasRegistradas { get; set; }

        public int SalidasRegistradas { get; set; }

        public double HorasLaboradas { get; set; }
    }
}