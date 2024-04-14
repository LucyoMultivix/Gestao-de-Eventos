using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoDeEventos
{
    public class Evento
    {
        //Propriedades Auto-Implementadas
        public string TituloEvento { get; set; }
        public string Data { get; set; }
        public string HoraInicial { get; set; }
        public string HoraFinal { get; set; }
        public string DescricaoEvento { get; set; }
        public string NPessoas { get; set; }
        public string PublicoAlvo { get; set; }
        public int IdContato { get; set; }
        //O id do evento deve sempre ter 6 caracteres alfanumericos
        public string IdEvento { get; set; }

        //Metodo Construtor Personalizado
        public Evento(string tituloEvento, string data, string horaInicial, string horaFinal, string publicoAlvo, string nPessoas, string descricaoEvento, int idContato, string idEvento) { 
            TituloEvento = tituloEvento;
            Data = data;
            HoraInicial = horaInicial;
            HoraFinal = horaFinal;
            DescricaoEvento = descricaoEvento;
            NPessoas = nPessoas;
            PublicoAlvo = publicoAlvo;
            IdContato = idContato;
            IdEvento = idEvento;
        }
    }
}
