using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoDeEventos
{
    public class Contato
    {
        //Propriedades Auto-Implementadas
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }

        //Metodo Construtor Personalizado
        public Contato(string nome, string email, string telefone) {
            Nome = nome;
            Email = email;
            Telefone = telefone;
        }
    }
}
