using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace GestaoDeEventos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////// INICIALIZAÇÃO DE VARIAVEIS /////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Essa variavel será utilizada para gerar os caracteres alfanumericos
            string alfanumericos = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            //Essa primeira array recebera o componente numerico de uma data, a segunda array recebera os componentes de 3 datas (Data de inicio, Data de termino e Data que será comparada)
            string[] dataPartes = new string[3];
            int[] datasTodasPartes = new int[9]; 
            //Essa array será utilizadas ao cadastrar eventos ou novos contatos
            string[] varTemporarias = new string[7];
            //"selec" será utilizado para selecionar as opções nos menus, "fim" definirá o fim dos loops do programa, "auxiliar"
            //será usado como variavel temporaria e "avisos" será utilizada para disparar mensagens de aviso que não atrapalhem o Console.Clear()
            int selec = 0, fim = 0, auxiliar = 0, avisos = 0;
            //A lista contatos armazena os contatos cadastrados no sistema, já a lista eventos armazena os eventos
            List<Contato> contatos = new List<Contato>();
            List<Evento> eventos = new List<Evento>();
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////// INICIALIZAÇÃO DE FUNÇÕES /////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Essa função salva um evento em um arquivo .txt (O caminho deve usar @ para ser uma string literal e não deve conter o nome do arquivo)
            void SalvarEvento(int eventoIndex)
            {
                //Tenta salvar o evento selecionado em um arquivo .txt
                try
                {
                    string nome = $"Evento_{eventos[eventoIndex].IdEvento}.txt";
                    using (StreamWriter sw = new StreamWriter(nome))
                    {
                        sw.WriteLine($"{eventos[eventoIndex].TituloEvento},{eventos[eventoIndex].Data},{eventos[eventoIndex].HoraInicial},{eventos[eventoIndex].HoraFinal},{eventos[eventoIndex].PublicoAlvo},{eventos[eventoIndex].NPessoas},{eventos[eventoIndex].DescricaoEvento},{eventos[eventoIndex].IdContato},{eventos[eventoIndex].IdEvento}");
                    }
                    avisos = 14;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("\nOcorreu um erro ao salvar os dados:" + ex.Message);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("\nOcorreu um erro ao salvar os dados:" + ex.Message);
                }
            }

            //Essa função dá split em uma data especifica, transforma em int e armazena no lugar correto da array datasTodasPartes
            //Operacao igual 0 coloca a data de inicio, a igual a 1 coloca a data de termino e a igual a 2 coloca a data a ser comparada
            //Essa função já espera receber datas no formato correto
            void OrganizarDatas(int operacao, string data)
            {
                dataPartes = data.Split("/");
                switch (operacao)
                {
                    //Data de Inicio
                    case 0:
                        datasTodasPartes[0] = Convert.ToInt32(dataPartes[0]);
                        datasTodasPartes[1] = Convert.ToInt32(dataPartes[1]);
                        datasTodasPartes[2] = Convert.ToInt32(dataPartes[2]);
                        break;
                    //Data de Termino
                    case 1:
                        datasTodasPartes[3] = Convert.ToInt32(dataPartes[0]);
                        datasTodasPartes[4] = Convert.ToInt32(dataPartes[1]);
                        datasTodasPartes[5] = Convert.ToInt32(dataPartes[2]);
                        break;
                    //Data a ser comparada
                    case 2:
                        datasTodasPartes[6] = Convert.ToInt32(dataPartes[0]);
                        datasTodasPartes[7] = Convert.ToInt32(dataPartes[1]);
                        datasTodasPartes[8] = Convert.ToInt32(dataPartes[2]);
                        break;
                }
            }

            //Essa função checa se a data de termino é maior que a data de inicio, se sim, retorna 0
            int DataTerminoMaior(int iniDia, int iniMes, int iniAno, int fimDia, int fimMes, int fimAno)
            {
                //Primeiro checa o ano (Deve ter ao menos o mesmo ano ou o ano do fim ser maior)
                if (iniAno == fimAno || iniAno < fimAno)
                {
                    //Se o ano fim for maior que o ano inicio, já não precisa comparar o resto
                    if (iniAno < fimAno)
                    {
                        return 0;
                    }
                    else
                    {
                        //Checa agora o mes
                        if (iniMes == fimMes || iniMes < fimMes)
                        {
                            //Se o mes fim for maior que o mes inicio, já não precisa comparar o resto
                            if (iniMes < fimMes)
                            {
                                return 0;
                            }
                            else
                            {
                                //Checa agora o dia
                                if (iniDia == fimDia || iniDia < fimDia)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return 1;
                                }
                            }
                        }
                        else
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    return 1;
                }
            }

            //Essa função compara se a data atual está dentro do periodo indicado, se sim, retorna 0
            int DataNoPeriodo(int iniDia, int iniMes, int iniAno, int fimDia, int fimMes, int fimAno, int datDia, int datMes, int datAno)
            {
                //Compara primeiro o ano
                if (datAno > iniAno && datAno < fimAno || datAno == fimAno || datAno == iniAno)
                {
                    //Note que há duas possibilidades aqui: (1) é que o ano desse evento não é igual ao da data final nem inicial
                    //Ou seja, nesse caso, o mes e dia não importam, e (2) é quando o ano é igual ao data final ou inicial, nesse 
                    //Caso, o mes faz toda a diferença
                    //Caso 1
                    if(datAno > iniAno && datAno < fimAno)
                    {
                        return 0;
                    }
                    //Caso 2
                    else
                    {
                        //Agora compara o mes
                        if (datMes > iniMes && datMes < fimMes || datMes == fimMes || datMes == iniMes)
                        {
                            //Aqui também há duas possibilidades identicas ao caso do ano, porém a referencia agora é o mes
                            //Caso 1
                            if(datMes > iniMes && datMes < fimMes)
                            {
                                return 0;
                            }
                            //Caso 2
                            else
                            {
                                //Agora compara o dia
                                if (datDia > iniDia && datDia < fimDia || datDia == fimDia || datDia == iniDia)
                                {
                                    return 0;
                                }
                                else
                                {
                                    return 1;
                                }
                            }
                        }
                        else
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    return 1;
                }
            }

            //Essa função serve para resetar as variaveis temporarias
            void ResetVariaveis()
            {
                for (int i = 0; i < 7; i++)
                {
                    varTemporarias[i] = "";
                }
            }

            //Essa função reseta as variaveis de data
            void ResetVariaveisData()
            {
                for (int i = 0; i < 9; i++)
                {
                    datasTodasPartes[i] = 0;
                }
            }

            //Essas duas funções abaixo são meio que só um capricho pra não ter que ficar digitando ou o trycatch para cada input
            int IntPut() {
                try
                {
                    return Convert.ToInt32(Console.ReadLine());
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("Erro de Nulidade: " + ex.Message);
                    return -1;
                }
                catch (FormatException ex) {
                    Console.WriteLine("Erro de Formato: " + ex.Message);
                    return -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro: " + ex.Message);
                    return -1;
                }
            }

            string StPut()
            {
                try
                {
                    return Console.ReadLine();
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("Erro de Nulidade: " + ex.Message);
                    return "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro: " + ex.Message);
                    return "";
                }
            }

            //Essa função busca o indice de um obj Contato especifico na lista contatos
            int IdContato(string nome)
            {
                return contatos.FindIndex(c => c.Nome == nome);
            }

            //Essa função gera uma string com 6 caracteres alfanumericos
            //(Po Bessa kakak só depois que me falaram que existe uma biblioteca que faz isso, fiz na marra atoa kkkkkk)
            string AlfaNum() {
                //Cria uma variavel do tipo StringBuilder para construir o código
                StringBuilder alfa = new StringBuilder();
                //Cria uma instancia Random para auxiliar no processo
                Random random = new Random();
                
                //Esse loop garante que só vai parar quando gerar um codigo alfanumerico inédito na lista de eventos 
                while (fim == 0)
                {

                    //Garante que o codigo terá 6 caracteres, com indices de 0 a 5
                    for(int i = 0; i < 6; i++)
                    {
                        //Vou usar o metodo Append para aos poucos ir adicionando caractere por caractere no codigo, usando como base
                        //a variavel "alfanumericos" e a função random com o metodo Next (Para garantir que não tenha caracteres repetidos
                        //já que sem o Next iria usar a mesma semente no random, e por fim uso o Length para pegar o numero maximo de caracteres
                        //da variavel "alfanumericos" para usar como máximo do random
                        alfa.Append(alfanumericos[random.Next(Convert.ToInt32(alfanumericos.Length))]);
                    }

                    //Checa se esse codigo alfanumerico já existe (Se existe, refaz o processo)
                    if (eventos.FindIndex(e => e.IdEvento == alfa.ToString()) == -1)
                    {
                        fim = 1;
                    }
                }
                fim = 0;
                return alfa.ToString();
            }

            //Essa Função gera uma pergunta de sim ou não para confirmar uma ação (Ela recebe um numero que indica a origem do problema)
            void TemCerteza(int origem)
            {
                Console.Clear();
                while (fim == 0)
                {
                    try {
                        selec = 1;
                        Console.WriteLine($"Tem certeza que deseja deletar o evento de ID {varTemporarias[0]} e Título {eventos.Find(ev => ev.IdEvento == varTemporarias[0]).TituloEvento}?\n(1) Sim\n(2) Nao\n");
                        if (selec != 1 && selec != 2)
                        {
                            Console.WriteLine("OPCAO INVALIDA");
                        }
                        selec = IntPut();
                        if (selec == 1 || selec == 2)
                        {
                            fim = 1;
                        }
                    }
                    catch(NullReferenceException ex)
                    {
                        Console.WriteLine("Nao foi possivel deletar o evento: " + ex.Message);
                        selec = 2;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Nao foi possivel deletar o evento: " + ex.Message);
                    }
                }
                fim = 0;
                ////////////////////// SIM //////////////////////
                if(selec == 1)
                {
                    switch (origem)
                    {
                        //Deleta o evento
                        case 1:
                            //Só permite deletar eventos que estão no seu nome, a menos que você seja um administrador
                            if(auxiliar == -99)
                            {
                                try
                                {
                                    eventos.Remove(eventos.Find(ev => ev.IdEvento == varTemporarias[0]));
                                    avisos = 3;
                                }
                                catch(ArgumentException ex)
                                {
                                    Console.WriteLine("Nao foi possivel deletar o evento: " + ex.Message);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Nao foi possivel deletar o evento: " + ex.Message);
                                }
                            }
                            else
                            {
                                try
                                {
                                    //Checa se esse evento digitado está no seu nome
                                    if (eventos.Find(ev => ev.IdEvento == varTemporarias[0]).IdContato == auxiliar)
                                    {
                                        eventos.Remove(eventos.Find(ev => ev.IdEvento == varTemporarias[0]));
                                        avisos = 3;
                                    }
                                    //Se não estiver no seu nome, gera aviso
                                    else
                                    {
                                        avisos = 4;
                                    }
                                }
                                catch (NullReferenceException ex){
                                    Console.WriteLine("Nao foi possivel deletar o evento: " + ex.Message);
                                }
                                catch (ArgumentException ex)
                                {
                                    Console.WriteLine("Nao foi possivel deletar o evento: " + ex.Message);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Nao foi possivel deletar o evento: " + ex.Message);
                                }
                            }
                            ResetVariaveis();
                            break;
                    }
                }
                ////////////////////// NÃO //////////////////////
                else
                {
                    switch (origem)
                    {
                        //Cancela o Delete de um evento
                        case 1:
                            selec = 6;
                            ResetVariaveis();
                            break;
                    }
                }
            }

            //Essa função limpa a tela e desenha o cabeçalho
            void Limpar()
            {
                //Limpa a Tela
                Console.Clear();
                //Desenha o Cabeçalho
                Console.WriteLine("///////////////// MULTIVIX EVENTOS /////////////////");
            }

            //Essa função verifica se a Data ou hora estão nos formatos corretos (Recebe o argumento tipo (Define se é data ou hora) e o argumento texto (É o texto a ser analisado)
            static bool FormatoDataTime(int tipo, string texto)
            {
                string formato = "";

                //Datas
                if(tipo == 0)
                {
                    formato = @"^\d{2}/\d{2}/\d{4}$";
                }
                //Horas
                else
                {
                    formato = @"^\d{2}:\d{2}$";
                }
                Match data = Regex.Match(texto, formato);
                return data.Success;
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////


            //////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////// PROGRAMA EM SI /////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////////
            //Inicializa Multivix como um Contato base ligado aos moderadores do aplicativo,e alguns outros contatos para usar a função pesquisa
            contatos.Add(new Contato("Multivix", "multivix@gmail.com", "(28) 99999-9999"));
            contatos.Add(new Contato("Delegacia", "serpmaos22@gmail.com", "190"));
            contatos.Add(new Contato("Alok", "alokasso@gmail.com", "(28) 99911-1111"));
            //Inicializa alguns Eventos para usar a função de pesquisa
            eventos.Add(new Evento("Turma de 99", "10/10/2024", "12:00", "16:30", "Alunos de Direito da Turma de 99", "40", "Reencontro dos alunos de Direito da turma de 99", contatos.FindIndex(c => c.Nome == "Multivix"), AlfaNum()));
            eventos.Add(new Evento("Inauguracao", "03/07/2024", "9:00", "10:30", "Aberto", "1000", "Inauguracao do Novo Polo da Multivix em Mimoso do Sul", contatos.FindIndex(c => c.Nome == "Multivix"), AlfaNum()));
            eventos.Add(new Evento("Calourada", "16/03/2025", "18:00", "00:30", "Alunos de Sistemas", "180", "Calourada que nunca teve no cursos de sistemas", contatos.FindIndex(c => c.Nome == "Multivix"), AlfaNum()));
            eventos.Add(new Evento("Cilada da PM", "10/10/2024", "12:00", "15:30", "Criminosos", "222", "Busca e Apreensao", contatos.FindIndex(c => c.Nome == "Delegacia"), AlfaNum()));
            eventos.Add(new Evento("Festa da Firma", "11/10/2024", "15:00", "17:00", "Policiais", "22", "Comemoracao de 50 anos da delegacia", contatos.FindIndex(c => c.Nome == "Delegacia"), AlfaNum()));
            eventos.Add(new Evento("Show Beneficente", "08/12/2024", "18:00", "00:30", "Aberto", "12000", "Show beneficente para auxiliar a causa dos calvos", contatos.FindIndex(c => c.Nome == "Alok"), AlfaNum()));
            eventos.Add(new Evento("Show de Natal", "25/12/2024", "18:00", "00:30", "Aberto", "32000", "Show de Natal com participacao especial do Rei Roberto Carlos", contatos.FindIndex(c => c.Nome == "Alok"), AlfaNum()));
            eventos.Add(new Evento("Casamento", "01/01/2025", "11:00", "17:30", "Familia e Amigos", "120", "Casamento do meu grande amigo Silvester Stalone", contatos.FindIndex(c => c.Nome == "Alok"), AlfaNum()));
            eventos.Add(new Evento("Solitude", "04/02/2024", "18:00", "00:30", "Eu (vulgo, Alok)", "1", "Vou colocar todo meu dinheiro em uma psicina e nadar até cansar", contatos.FindIndex(c => c.Nome == "Alok"), AlfaNum()));

            //Loop principal do programa
            while (fim != 5){
                //Desenha as opções e direciona aos Menus
                switch (selec) {
                    //Menu Principal
                    case 0:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            Console.WriteLine("****************** Menu Principal ******************\n(1) Manipular Eventos\n(2) Pesquisar Eventos e Contatos\n(3) Sair\n");
                            selec = IntPut();
                            switch (selec)
                            {
                                //Menu de Login ou Menu de Pesquisa
                                case 1:
                                case 2:
                                    ResetVariaveis();
                                    fim = 1;
                                    break;
                                case 3:
                                    fim = 1;
                                    break;
                            }
                        }
                        break;

                    //Menu de Opções de Login
                    case 1:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            Console.WriteLine("****************** Opcoes de Login ******************\n(1) Novo Contato\n(2) Contato Existente\n(3) Acessar como Administrador\n(4) Voltar\nOBS.:É necessário que você seja um contato cadastrado para manipular eventos\n");
                            //Mostra aviso caso tentar logar com um contato não cadastrado
                            if (avisos == 1)
                            {
                                Console.WriteLine("\nCONTATO NAO ESTA CADASTRADO\n");
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Direciona aos menus correspondentes à opção escolhida
                            switch (selec)
                            {
                                //Leva ao Menu de Cadastro de Contato
                                case 1:
                                    fim = 1;
                                    selec = 4;
                                    break;
                                //Tenta Logar, se conseguir leva ao menu de manipulação de eventos
                                case 2:
                                    Console.WriteLine("Digite seu Nome: ");
                                    varTemporarias[0] = StPut();
                                    //Procura se esse contato existe no sistema
                                    if (contatos.FindIndex(c => c.Nome == varTemporarias[0]) != -1)
                                    {
                                        //Auxiliar recebe o indice referente a esse contato
                                        auxiliar = contatos.FindIndex(c => c.Nome == varTemporarias[0]);
                                        selec = 6;
                                        fim = 1;
                                    }
                                    else
                                    {
                                        avisos = 1;
                                    }
                                    break;
                                //Leva ao Menu de Acesso como Administrador
                                case 3:
                                    //Auxiliar recebe o codigo referente aos administradores (Coloquei -99 sem nenhuma razão especial) 
                                    auxiliar = -99;
                                    selec = 6;
                                    fim = 1;
                                    break;
                                //Volta ao Menu Principal
                                case 4:
                                    selec = 0;
                                    fim = 1;
                                    break;
                            }
                        }
                        //Reseta as variaveis temporarias
                        ResetVariaveis();
                        break;

                    //Pesquisar Eventos
                    case 2:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            Console.WriteLine("****************** Todos os Eventos do Sistema ******************\n");
                            Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Contato :: Descricao");
                            foreach (Evento ev in eventos)
                            {
                                Console.WriteLine($"{ev.IdEvento} :: {ev.TituloEvento} :: {ev.Data} :: {ev.HoraInicial} :: {ev.HoraFinal} :: {ev.PublicoAlvo} :: {ev.NPessoas} :: {contatos[ev.IdContato].Nome} :: {ev.DescricaoEvento}\n\n");
                            }
                            Console.WriteLine("****************** Pesquisa de Eventos ******************\n(1) Filtrar por Data\n(2) Filtrar por Contato\n(3) Filtrar por Periodo de Tempo\n(4) Salvar um Evento em Arquivo de Texto\n(5) Voltar\n");
                            if (avisos == 13)
                            {
                                Console.WriteLine("NAO EXISTE EVENTO COM ESSE ID OU ELE FOI DIGITADO INCORRETAMENTE");
                            }
                            if (avisos == 14)
                            {
                                Console.WriteLine("ARQUIVO SALVO COM SUCESSO NO DIRETORIO: " + Directory.GetCurrentDirectory());
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Mantem a janela atual em caso de uma opção invalida
                            //Direciona aos menus correspondentes à opção escolhida
                            switch (selec)
                            {
                                //Leva ao menu que filtra os eventos por Data
                                case 1:
                                    ResetVariaveis();
                                    selec = 9;
                                    fim = 1;
                                    break;
                                //Leva ao menu que filtra os eventos por Contato
                                case 2:
                                    ResetVariaveis();
                                    selec = 10;
                                    fim = 1;
                                    break;
                                //Leva à Janela que filtra eventos por periodo de tempo
                                case 3:
                                    selec = 11;
                                    ResetVariaveisData();
                                    fim = 1;
                                    break;
                                //Salva o evento em um arquivo de Texto
                                case 4:
                                    Console.WriteLine("Digite o ID do Evento: ");
                                    varTemporarias[0] = StPut();
                                    //Checa se existe algum evento com esse id
                                    if (eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0]) == -1)
                                    {
                                        avisos = 13;
                                        varTemporarias[0] = "";
                                    }
                                    //Se sim, salva o arquivo
                                    else
                                    {
                                        SalvarEvento(eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0]));
                                    }
                                    break;
                                //Volta ao Menu Principal
                                case 5:
                                    selec = 0;
                                    fim = 1;
                                    break;
                            }
                        }
                        break;

                    //Fechar o programa
                    case 3:
                        fim = 5;
                        break;

                    //Menu de Cadastro
                    case 4:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            Console.WriteLine("****************** Cadastro de Contato ******************\n                (Selecione para Editar)");
                            //Editar o Nome
                            if (varTemporarias[0] == "")
                            {
                                Console.WriteLine("(1) Nome: --");
                            }
                            else
                            {
                                Console.WriteLine($"(1) Nome: {varTemporarias[0]}");
                            }
                            //Editar o Telefone
                            if (varTemporarias[1] == "")
                            {
                                Console.WriteLine("(2) Telefone: --");
                            }
                            else
                            {
                                Console.WriteLine($"(2) Telefone: {varTemporarias[1]}");
                            }
                            //Editar o Email
                            if (varTemporarias[2] == "")
                            {
                                Console.WriteLine("(3) Email: --");
                            }
                            else
                            {
                                Console.WriteLine($"(3) Email: {varTemporarias[2]}");
                            }

                            //Se ainda não preencheu todos os dados, não mostra a opção de finalizar o cadastro 
                            if (varTemporarias[0] == "" || varTemporarias[1] == "" || varTemporarias[2] == "")
                            {
                                Console.WriteLine("(4) Cancelar Cadastro\nOBS.: Selecione e altere cada um dos campos para finalizar o cadastro\n");
                            }
                            else
                            {
                                Console.WriteLine("(4) Confirmar Cadastro\n(5) Cancelar Cadastro\n");
                            }
                            if (avisos == 5)
                            {
                                Console.WriteLine("ESSE NOME JA ESTA CADASTRADO NO SISTEMA");
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Inserir os valores dos campos
                            switch (selec)
                            {
                                //Nome
                                case 1:
                                    Console.WriteLine("Digite seu Nome: ");
                                    varTemporarias[0] = StPut();
                                    break;
                                //Telefone
                                case 2:
                                    Console.WriteLine("Digite seu Telefone: ");
                                    varTemporarias[1] = StPut();
                                    break;
                                //Email
                                case 3:
                                    Console.WriteLine("Digite seu Email: ");
                                    varTemporarias[2] = StPut();
                                    break;
                            }
                            //Se ainda não preencheu todos os dados, não permite finalizar o cadastro, apenas editar os campo ou cancelar
                            if (varTemporarias[0] == "" || varTemporarias[1] == "" || varTemporarias[2] == "")
                            {
                                //Volta ao Menu de Opções de Login
                                if (selec == 4)
                                {
                                    fim = 1;
                                    selec = 1;
                                }
                            }
                            else
                            {
                                //Confirma o cadastro adicionando um novo obj da classe Contato na lista contatos
                                if (selec == 4)
                                {
                                    //Procura se o nome usado já está cadastrado no sistema (Se estiver, não permite o cadastro)
                                    if (IdContato(varTemporarias[0]) == -1)
                                    {
                                        contatos.Add(new Contato(varTemporarias[0], varTemporarias[2], varTemporarias[1]));
                                        selec = 1;
                                        fim = 1;
                                    }
                                    else
                                    {
                                        varTemporarias[0] = "";
                                        avisos = 5;
                                    }

                                }
                                //Cancelar (Volta ao Menu de Opções de Login)
                                if (selec == 5)
                                {
                                    selec = 1;
                                    fim = 1;
                                }
                            }
                        }
                        break;

                    //Menu de Manipulação de Eventos
                    case 6:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Se for administrador
                            if (auxiliar == -99)
                            {
                                Console.WriteLine("****************** Eventos Cadastrados no Sistema ******************");
                                Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Contato :: Descricao\n");
                                foreach (Evento ev in eventos)
                                {
                                    Console.WriteLine($"{ev.IdEvento} :: {ev.TituloEvento} :: {ev.Data} :: {ev.HoraInicial} :: {ev.HoraFinal} :: {ev.PublicoAlvo} :: {ev.NPessoas} :: {contatos.ElementAt(ev.IdContato).Nome} :: {ev.DescricaoEvento}\n\n");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"****************** Eventos Cadastrados em seu Nome: {contatos.ElementAt(auxiliar).Nome} ******************");
                                Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Descricao\n");
                                foreach (Evento ev in eventos)
                                {
                                    //Mostra só eventos cadastrados nesse nome
                                    if (ev.IdContato == auxiliar)
                                    {
                                        Console.WriteLine($"{ev.IdEvento} :: {ev.TituloEvento} :: {ev.Data} :: {ev.HoraInicial} :: {ev.HoraFinal} :: {ev.PublicoAlvo} :: {ev.NPessoas} :: {ev.DescricaoEvento}\n\n");
                                    }
                                }
                            }
                            //Desenha as Opções
                            Console.WriteLine("****************** Manipular Eventos ******************\n(1) Novo Evento\n(2) Editar Evento\n(3) Deletar Evento\n(4) Voltar ao Menu Principal\n");
                            //Avisos
                            switch (avisos)
                            {
                                //Evento não existe
                                case 2:
                                    Console.WriteLine("EVENTO NAO EXISTE OU FOI DIGITADO INCORRETAMENTE");
                                    break;
                                //Evento deletado
                                case 3:
                                    Console.WriteLine("EVENTO DELETADO COM SUCESSO");
                                    break;
                                //Evento fora do seu dominio
                                case 4:
                                    Console.WriteLine("ESSE EVENTO NAO ESTA NO SOB SEU DOMINIO");
                                    break;
                            }
                            selec = IntPut();
                            //Direciona aos menus correspondentes à opção escolhida
                            switch (selec)
                            {
                                //Leva à janela de Cadastro de Evento
                                case 1:
                                    selec = 7;
                                    ResetVariaveis();
                                    fim = 1;
                                    break;
                                //Leva à janela de Edição de Evento
                                case 2:
                                    ResetVariaveis();
                                    Console.WriteLine("Digite o ID do Evento: ");
                                    varTemporarias[0] = StPut();
                                    //Checa se esse evento existe (Se não, gera um aviso)
                                    if (eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0]) == -1)
                                    {
                                        avisos = 2;
                                    }
                                    else
                                    {
                                        selec = 8;
                                        fim = 1;
                                    }
                                    break;
                                //Deletar Evento
                                case 3:
                                    Console.WriteLine("Digite o ID do Evento: ");
                                    varTemporarias[0] = StPut();
                                    //Checa se esse evento existe (Se não, gera um aviso)
                                    if (eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0]) == -1)
                                    {
                                        avisos = 2;
                                    }
                                    //Deleta o evento
                                    else
                                    {
                                        //Tem certeza?
                                        TemCerteza(1);
                                    }
                                    break;
                                //Volta ao Menu Principal
                                case 4:
                                    selec = 0;
                                    fim = 1;
                                    auxiliar = 0;
                                    break;
                            }
                        }
                        break;

                    //Cadastra Evento
                    case 7:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            if (auxiliar == -99)
                            {
                                Console.WriteLine("BEM VINDO, ADMINISTRADOR!");
                            }
                            else
                            {
                                Console.WriteLine($"BEM VINDO, {contatos[auxiliar].Nome}!");

                            }
                            Console.WriteLine("****************** Cadastro de Evento ******************\n                (Selecione para Editar)");
                            //Editar o Título do evento
                            if (varTemporarias[0] == "")
                            {
                                Console.WriteLine("(1) Titulo do Evento: --");
                            }
                            else
                            {
                                Console.WriteLine($"(1) Titulo do Evento: {varTemporarias[0]}");
                            }
                            //Editar a Data
                            if (varTemporarias[1] == "")
                            {
                                Console.WriteLine("(2) Data (No formato dd/mm/aaaa; Ex.: 01/04/2024): --");
                            }
                            else
                            {
                                Console.WriteLine($"(2) Data (No formato dd/mm/aaaa; Ex.: 22/09/2024): {varTemporarias[1]}");
                            }
                            //Editar a Hora de Inicio
                            if (varTemporarias[2] == "")
                            {
                                Console.WriteLine("(3) Hora de Inicio (No formato hh:mm; Ex.: 00:25): --");
                            }
                            else
                            {
                                Console.WriteLine($"(3) Hora de Inicio (No formato hh:mm; Ex.: 22:30): {varTemporarias[2]}");
                            }
                            //Editar a Hora de Finalização
                            if (varTemporarias[3] == "")
                            {
                                Console.WriteLine("(4) Hora de Finalizacao (No formato hh:mm; Ex.: 04:09): --");
                            }
                            else
                            {
                                Console.WriteLine($"(4) Hora de Finalizacao (No formato hh:mm; Ex.: 18:02): {varTemporarias[3]}");
                            }
                            //Editar a Publico Alvo
                            if (varTemporarias[4] == "")
                            {
                                Console.WriteLine("(5) Publico Alvo: --");
                            }
                            else
                            {
                                Console.WriteLine($"(5) Publico Alvo: {varTemporarias[4]}");
                            }
                            //Editar a Numero de Pessoas
                            if (varTemporarias[5] == "")
                            {
                                Console.WriteLine("(6) Numero Estimado de Pessoas: --");
                            }
                            else
                            {
                                Console.WriteLine($"(6) Numero Estimado de Pessoas: {varTemporarias[5]}");
                            }
                            //Editar a Descrição do Evento
                            if (varTemporarias[6] == "")
                            {
                                Console.WriteLine("(7) Descricao do Evento: --");
                            }
                            else
                            {
                                Console.WriteLine($"(7) Descricao do Evento: {varTemporarias[6]}");
                            }
                            //Se ainda não preencheu todos os dados, não mostra a opção de finalizar o cadastro 
                            if (varTemporarias[0] == "" || varTemporarias[1] == "" || varTemporarias[2] == "" || varTemporarias[3] == "" || varTemporarias[4] == "" || varTemporarias[5] == "" || varTemporarias[6] == "")
                            {
                                Console.WriteLine("(8) Cancelar Cadastro\nOBS.: Selecione e altere cada um dos campos para finalizar o cadastro\n");
                            }
                            else
                            {
                                Console.WriteLine("(8) Confirmar Cadastro\n(9) Cancelar Cadastro\n");
                            }
                            switch (avisos)
                            {
                                case 6:
                                    Console.WriteLine("A DATA DIGITADA NAO ESTA NO FORMATO CORRETO");
                                    break;
                                case 7:
                                    Console.WriteLine("O HORARIO DIGITADO NAO ESTA NO FORMATO CORRETO");
                                    break;
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Inserir os valores dos campos
                            switch (selec)
                            {
                                //Titulo
                                case 1:
                                    Console.WriteLine("Digite o Titulo do Evento: ");
                                    varTemporarias[0] = StPut();
                                    break;
                                //Data
                                case 2:
                                    Console.WriteLine("Digite a Data do Evento (No formato dd/mm/aaaa): ");
                                    varTemporarias[1] = StPut();
                                    //Checa se está na formatção certa
                                    if (FormatoDataTime(0, varTemporarias[1]) == false)
                                    {
                                        varTemporarias[1] = "";
                                        avisos = 6;
                                    }
                                    break;
                                //Hora de Inicio
                                case 3:
                                    Console.WriteLine("Digite a Hora de Inicio (No formato 24:00): ");
                                    varTemporarias[2] = StPut();
                                    //Checa se está na formatção certa
                                    if (FormatoDataTime(1, varTemporarias[2]) == false)
                                    {
                                        varTemporarias[2] = "";
                                        avisos = 6;
                                    }
                                    break;
                                //Hora de Finalização
                                case 4:
                                    Console.WriteLine("Digite a Hora de Finalizacao (No formato 24:00): ");
                                    varTemporarias[3] = StPut();
                                    //Checa se está na formatção certa
                                    if (FormatoDataTime(1, varTemporarias[3]) == false)
                                    {
                                        varTemporarias[3] = "";
                                        avisos = 7;
                                    }
                                    break;
                                //Publico Alvo
                                case 5:
                                    Console.WriteLine("Digite o Publico Alvo: ");
                                    varTemporarias[4] = StPut();
                                    break;
                                //Numero Estimado de Pessoas
                                case 6:
                                    Console.WriteLine("Digite o Numero Estimado de Pessoas: ");
                                    varTemporarias[5] = StPut();
                                    break;
                                //Descrição do Evento
                                case 7:
                                    Console.WriteLine("Digite a Descricao do Evento: ");
                                    varTemporarias[6] = StPut();
                                    break;
                            }

                            //Se ainda não preencheu todos os dados, não permite finalizar o cadastro, apenas editar os campo ou cancelar
                            if (varTemporarias[0] == "" || varTemporarias[1] == "" || varTemporarias[2] == "" || varTemporarias[3] == "" || varTemporarias[4] == "" || varTemporarias[5] == "" || varTemporarias[6] == "")
                            {
                                //Volta para o Menu de Manipulação de Eventos
                                if (selec == 8)
                                {
                                    selec = 6;
                                    fim = 1;
                                }
                            }
                            else
                            {
                                //Confirma o cadastro do evento e volta ao Menu de Manipulação de Eventos
                                if (selec == 8)
                                {
                                    //Se você está logado como Modo Administrador, você pode ser o seu proprio contato ou registrar outro
                                    if (auxiliar == -99)
                                    {
                                        eventos.Add(new Evento(varTemporarias[0], varTemporarias[1], varTemporarias[2], varTemporarias[3], varTemporarias[4], varTemporarias[5], varTemporarias[6], contatos.FindIndex(c => c.Nome == "Multivix"), AlfaNum()));
                                    }
                                    //Adiciona o Novo Evento e volta para o Menu de Manipulação de Eventos
                                    else
                                    {
                                        eventos.Add(new Evento(varTemporarias[0], varTemporarias[1], varTemporarias[2], varTemporarias[3], varTemporarias[4], varTemporarias[5], varTemporarias[6], auxiliar, AlfaNum()));
                                    }
                                    selec = 6;
                                    fim = 1;
                                }

                                //Volta ao Menu de Manipulação de Eventos
                                if (selec == 9)
                                {
                                    selec = 6;
                                    fim = 1;
                                }
                            }
                        }
                        break;

                    //Editar Evento 
                    case 8:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            Console.WriteLine("****************** Edicao de Evento ******************\n                (Selecione para Editar)");
                            //Editar o Título do evento
                            Console.WriteLine($"(1) Titulo do Evento: {eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].TituloEvento}");
                            //Editar a Data
                            Console.WriteLine($"(2) Data (No formato dd/mm/aaaa; Ex.: 22/09/2024): {eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].Data}");
                            //Editar a Hora de Inicio
                            Console.WriteLine($"(3) Hora de Inicio (No formato hh:mm; Ex.: 22:30): {eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].HoraInicial}");
                            //Editar a Hora de Finalização
                            Console.WriteLine($"(4) Hora de Finalizacao (No formato hh:mm; Ex.: 18:02): {eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].HoraFinal}");
                            //Editar a Publico Alvo
                            Console.WriteLine($"(5) Publico Alvo: {eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].PublicoAlvo}");
                            //Editar a Numero de Pessoas
                            Console.WriteLine($"(6) Numero Estimado de Pessoas: {eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].NPessoas}");
                            //Editar a Descrição do Evento
                            Console.WriteLine($"(7) Descricao do Evento: {eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].DescricaoEvento}");
                            Console.WriteLine("(8) Finalizar Edicao\n");
                            switch (avisos)
                            {
                                case 6:
                                    Console.WriteLine("A DATA DIGITADA NAO ESTA NO FORMATO CORRETO");
                                    break;
                                case 7:
                                    Console.WriteLine("O HORARIO DIGITADO NAO ESTA NO FORMATO CORRETO");
                                    break;
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Inserir os valores dos campos
                            switch (selec)
                            {
                                //Titulo
                                case 1:
                                    Console.WriteLine("Digite o Titulo do Evento: ");
                                    eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].TituloEvento = StPut();
                                    break;
                                //Data
                                case 2:
                                    Console.WriteLine("Digite a Data do Evento (No formato dd/mm/aaaa): ");
                                    varTemporarias[1] = StPut();
                                    //Checa se está na formatção certa
                                    if (FormatoDataTime(0, varTemporarias[1]) == false)
                                    {
                                        avisos = 6;
                                    }
                                    else
                                    {
                                        eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].Data = varTemporarias[1];
                                    }
                                    varTemporarias[1] = "";
                                    break;
                                //Hora de Inicio
                                case 3:
                                    Console.WriteLine("Digite a Hora de Inicio (No formato 24:00): ");
                                    varTemporarias[2] = StPut();
                                    //Checa se está na formatção certa
                                    if (FormatoDataTime(1, varTemporarias[2]) == false)
                                    {
                                        avisos = 6;
                                    }
                                    else
                                    {
                                        eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].HoraInicial = varTemporarias[2];
                                    }
                                    varTemporarias[2] = "";
                                    break;
                                //Hora de Finalização
                                case 4:
                                    Console.WriteLine("Digite a Hora de Finalizacao (No formato 24:00): ");
                                    varTemporarias[3] = StPut();
                                    //Checa se está na formatção certa
                                    if (FormatoDataTime(1, varTemporarias[3]) == false)
                                    {
                                        avisos = 7;
                                    }
                                    else
                                    {
                                        eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].HoraFinal = varTemporarias[3];

                                    }
                                    varTemporarias[3] = "";
                                    break;
                                //Publico Alvo
                                case 5:
                                    Console.WriteLine("Digite o Publico Alvo: ");
                                    eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].PublicoAlvo = StPut();
                                    break;
                                //Numero Estimado de Pessoas
                                case 6:
                                    Console.WriteLine("Digite o Numero Estimado de Pessoas: ");
                                    eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].NPessoas = StPut();
                                    break;
                                //Descrição do Evento
                                case 7:
                                    Console.WriteLine("Digite a Descricao do Evento: ");
                                    eventos[eventos.FindIndex(ev => ev.IdEvento == varTemporarias[0])].DescricaoEvento = StPut();
                                    break;
                                //Confirma a Edição e volta ao menu de manipulacao de Eventos
                                case 8:
                                    selec = 6;
                                    fim = 1;
                                    break;
                            }
                        }
                        break;

                    //Pesquisar Eventos por Data
                    case 9:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            Console.WriteLine("****************** Filtro de Evento por Data ******************\n");
                            Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Contato :: Descricao");
                            if (varTemporarias[0] != "")
                            {
                                foreach (Evento ev in eventos)
                                {
                                    if (ev.Data == varTemporarias[0])
                                    {
                                        Console.WriteLine($"{ev.IdEvento} :: {ev.TituloEvento} :: {ev.Data} :: {ev.HoraInicial} :: {ev.HoraFinal} :: {ev.PublicoAlvo} :: {ev.NPessoas} :: {contatos[ev.IdContato].Nome} :: {ev.DescricaoEvento}\n\n");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("         ---- NENHUMA DATA SELECIONADA ----\n");
                            }
                            Console.WriteLine("****************** Opcoes ******************\n(1) Digitar Data\n(2) Voltar\n");
                            if (avisos == 8)
                            {
                                Console.WriteLine("NAO HA NENHUM EVENTO NESSA DATA OU ELA FOI DIGITADA NO FORMATO INCORRETO");
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Direciona aos menus correspondentes à opção escolhida
                            switch (selec)
                            {
                                //Digita Data
                                case 1:
                                    Console.WriteLine("Digite a Data (No formato dd/mm/aaaa; Ex.: 01/05/2024): ");
                                    varTemporarias[0] = StPut();
                                    //Verifica se a data está no formato correto ou se tem algum evento nessa data
                                    if (FormatoDataTime(0, varTemporarias[0]) == false || FormatoDataTime(0, varTemporarias[0]) == true && eventos.FindIndex(ev => ev.Data == varTemporarias[0]) == -1)
                                    {
                                        varTemporarias[0] = "";
                                        avisos = 8;
                                    }
                                    break;
                                //Volta ao Menu de Pesquisa
                                case 2:
                                    varTemporarias[0] = "";
                                    selec = 2;
                                    fim = 1;
                                    break;
                            }
                        }
                        break;

                    //Pesquisar Eventos por Contato
                    case 10:
                        fim = 0;
                        while (fim == 0)
                        {
                            Limpar();
                            //Desenha as Opções
                            Console.WriteLine("****************** Filtro de Evento por Contato ******************\n");
                            Console.WriteLine("****************** Contatos no Sistema ******************\n");
                            Console.WriteLine("Nome :: Telefone :: Email\n");
                            foreach (Contato c in contatos)
                            {
                                Console.WriteLine($"{c.Nome} :: {c.Telefone} :: {c.Email}\n\n");
                            }
                            if (varTemporarias[0] != "")
                            {
                                Console.WriteLine("****************** Eventos no Filtro ******************\n");
                                Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Contato :: Descricao");
                                foreach (Evento ev in eventos)
                                {
                                    if (ev.IdContato == contatos.FindIndex(c => c.Nome == varTemporarias[0]))
                                    {
                                        Console.WriteLine($"{ev.IdEvento} :: {ev.TituloEvento} :: {ev.Data} :: {ev.HoraInicial} :: {ev.HoraFinal} :: {ev.PublicoAlvo} :: {ev.NPessoas} :: {contatos[ev.IdContato].Nome} :: {ev.DescricaoEvento}\n\n");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("****************** Eventos no Filtro ******************\n");
                                Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Contato :: Descricao");
                                Console.WriteLine("         ---- NENHUMA CONTATO SELECIONADO ----\n");
                            }
                            Console.WriteLine("****************** Opcoes ******************\n(1) Digitar Nome do Contato\n(2) Voltar\n");
                            if (avisos == 9)
                            {
                                Console.WriteLine("NAO HA NENHUM CONTATO COM ESSE NOME OU ELE FOI DIGITADO INCORRETAMENTE");
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Direciona aos menus correspondentes à opção escolhida
                            switch (selec)
                            {
                                //Digita o Nome
                                case 1:
                                    Console.WriteLine("Digite o Nome do Contato: ");
                                    varTemporarias[0] = StPut();
                                    //Checa se o nome digitado está no sistema
                                    if (contatos.FindIndex(c => c.Nome == varTemporarias[0]) == -1)
                                    {
                                        varTemporarias[0] = "";
                                        avisos = 9;
                                    }
                                    break;
                                //Volta ao Menu de Pesquisa
                                case 2:
                                    varTemporarias[0] = "";
                                    fim = 1;
                                    selec = 2;
                                    break;
                            }
                        }
                        break;

                    //Filtra a data por periodo de tempo
                    case 11:
                        fim = 0;
                        while (fim == 0)
                        {
                            //Desenha as Opções
                            Limpar();
                            Console.WriteLine("****************** Filtro de Evento por Periodo de Tempo ******************\n");
                            if(datasTodasPartes[0] != 0 && datasTodasPartes[1] != 0 && datasTodasPartes[2] != 0 && datasTodasPartes[3] != 0 && datasTodasPartes[4] != 0 && datasTodasPartes[5] != 0)
                            {
                                varTemporarias[1] = "s";
                            }
                            else
                            {
                                varTemporarias[1] = "";
                            }
                            if (varTemporarias[1] != "")
                            {
                                Console.WriteLine("****************** Eventos no Filtro ******************\n");
                                Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Contato :: Descricao");
                                foreach (Evento ev in eventos)
                                {
                                    OrganizarDatas(2,ev.Data);
                                    if (DataNoPeriodo(datasTodasPartes[0], datasTodasPartes[1], datasTodasPartes[2], datasTodasPartes[3], datasTodasPartes[4], datasTodasPartes[5], datasTodasPartes[6], datasTodasPartes[7], datasTodasPartes[8]) == 0)
                                    {
                                        Console.WriteLine($"{ev.IdEvento} :: {ev.TituloEvento} :: {ev.Data} :: {ev.HoraInicial} :: {ev.HoraFinal} :: {ev.PublicoAlvo} :: {ev.NPessoas} :: {contatos[ev.IdContato].Nome} :: {ev.DescricaoEvento}\n\n");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("****************** Eventos no Filtro ******************\n");
                                Console.WriteLine("ID :: Titulo :: Data :: Hora Inicio :: Hora Fim :: Publico Alvo :: Estimativa de Pessoas :: Contato :: Descricao");
                                Console.WriteLine("         ---- NENHUMA PERIODO SELECIONADO ----\n");
                            }
                            Console.WriteLine("****************** Opcoes ******************");
                            //Data de Inicio
                            if (varTemporarias[2] == "")
                            {
                                Console.WriteLine("(1) Data Inicial (No formato dd/mm/aaaa; Ex.: 01/09/2023): --");
                            }
                            else
                            {
                                Console.WriteLine($"(1) Data Inicial (No formato dd/mm/aaaa; Ex.: 01/09/2023): {varTemporarias[2]}");
                            }
                            //Data Final
                            if (varTemporarias[3] == "")
                            {
                                Console.WriteLine("(2) Data Final (No formato dd/mm/aaaa; Ex.: 01/09/2023): --");
                            }
                            else
                            {
                                Console.WriteLine($"(2) Data Final (No formato dd/mm/aaaa; Ex.: 01/09/2023): {varTemporarias[3]}");
                            }
                            Console.WriteLine("(3) Voltar");
                            switch (avisos)
                            {
                                case 10:
                                    Console.WriteLine("A DATA INFORMADA NAO ESTA NO FORMATO CORRETO");
                                    break;
                                
                                case 11:
                                    Console.WriteLine("A DATA DO FIM DO PERIODO É MENOR QUE A DATA DE INICIO");
                                    break;

                                case 12:
                                    Console.WriteLine("INSIRA PRIMEIRO A DATA DE INICIO");
                                    break;
                            }
                            selec = IntPut();
                            avisos = 0;
                            //Direciona aos menus correspondentes à opção escolhida
                            switch (selec)
                            {
                                //Digita a Data Inicial
                                case 1:
                                    Console.WriteLine("Digite a Data Inicial: ");
                                    varTemporarias[2] = StPut();
                                    //Checa se a data está no formato correto
                                    if (FormatoDataTime(0, varTemporarias[2]) == false)
                                    {
                                        varTemporarias[2] = "";
                                        avisos = 10;
                                    }
                                    //Introduz os compontentes da data no lugar certo
                                    else
                                    {
                                        OrganizarDatas(0, varTemporarias[2]);
                                    }
                                    break;
                                //Volta ao Menu de Pesquisa
                                case 2:
                                    //Checa se a data inicial ja foi inserida
                                    if (varTemporarias[2] != "")
                                    {
                                        Console.WriteLine("Digite a Data Final: ");
                                        varTemporarias[3] = StPut();
                                        //Checa se a data está no formato correto
                                        if (FormatoDataTime(0, varTemporarias[3]) == false)
                                        {
                                            varTemporarias[3] = "";
                                            avisos = 10;
                                        }
                                        //Checa se o periodo final está maior que o inicial
                                        else
                                        {
                                            OrganizarDatas(1, varTemporarias[3]);
                                            if (DataTerminoMaior(datasTodasPartes[0], datasTodasPartes[1], datasTodasPartes[2], datasTodasPartes[3], datasTodasPartes[4], datasTodasPartes[5]) == 1)
                                            {
                                                varTemporarias[3] = "";
                                                avisos = 11;
                                                datasTodasPartes[3] = 0;
                                                datasTodasPartes[4] = 0;
                                                datasTodasPartes[5] = 0;
                                            }
                                            //Introduz os compontentes da data no lugar certo
                                            else
                                            {
                                                OrganizarDatas(1, varTemporarias[3]);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        varTemporarias[3] = "";
                                        avisos = 12;
                                    }
                                    break;
                                //Volta ao Menu de Pesquisa
                                case 3:
                                    fim = 1;
                                    selec = 2;
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}