using SmartMix.Core.Infrastructure.Plc.Enums;
using SmartMix.Core.Infrastructure.Plc.Helpers;
using SmartMix.Core.Infrastructure.Plc.Variables;
using System.Text;

namespace SmartMix.Core.Infrastructure.Plc.Parser
{
    public class CsvRegisterParser : IRegisterParser<Variable>
    {
        /// <summary>
        /// Представляет логер ошибок.
        /// </summary>
        private readonly Action<string> _errorLog;

        /// <summary>
        /// Представляет логер информационных сообщений.
        /// </summary>
        private readonly Action<string> _infoLog;

        /// <summary>
        /// Инициализирует новый экземпляр класса с указанными логерами.
        /// </summary>
        /// <param name="errorLog">Логер ошибок.</param>
        /// <param name="infoLog">Логер информационных сообщений.</param>
        public CsvRegisterParser(Action<string> errorLog = null, Action<string> infoLog = null)
        {
            _errorLog = errorLog;
            _infoLog = infoLog;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса для указанного набора сетевых <paramref name="text"/> и начальным адресом опроса регистров <paramref name="startAddress"/>.
        /// </summary>
        /// <param name="text">Список сетевых переменных вида: (WORD|DWORD...);Имя;Адрес;(R|RW);[комментарий] ...</param>
        /// <param name="startAddress">Начальный адрес опроса регистров.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IParserResult<Variable> GetFromCsvText(string text, ushort startAddress)
        {
            // TODO: регулярное выражение для сбора сетевых по шаблону
            //Regex reg = new Regex(@"nciVIBR([0-9]+)_WEIGHT_START");
            //MatchCollection mc = reg.Matches(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            ushort startAddr = startAddress;
            ushort endAddr = 0;
            ushort firstNciAddr = 0;

            Dictionary<string, Variable> registers = new Dictionary<string, Variable>();

            #region Определение первого адреса регистра nci
            bool needFind = true; // флаг остановки поиска
            #endregion

            string line; //строка файла

            using (StreamReader sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(text))))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    try
                    {
                        if (!line.Contains(";;"))
                        {
                            line = line.Trim();

                            string[] conf = line.Split(';');

                            if (conf.Length < 2) continue;

                            string[] type_array = null;
                            if (conf[0].Contains(":"))
                                type_array = conf[0].Split(':');

                            // Чтение типа
                            VariableType type;
                            byte array_size = 0;
                            if (type_array == null)
                            {
                                type = PlcIOHelper.GetEnum4Description<VariableType>(conf[0].Replace("\t", ""), '|');
                            }
                            else
                            {
                                type = PlcIOHelper.GetEnum4Description<VariableType>(type_array[0].Replace("\t", ""), '|');
                                array_size = byte.Parse(type_array[1]);
                            }

                            // Чтение имени
                            if (conf[1].Length == 0) continue;
                            string var_name = conf[1];

                            // Чтение адреса
                            UInt16 adr;
                            byte mask = 0;
                            if (conf[2].Contains("."))
                            {
                                string register_adr = null;
                                string bit_adr = null;
                                int j = 0;
                                while (conf[2][j] != '.') register_adr += conf[2][j++];
                                j++;
                                while (j < conf[2].Length) bit_adr += conf[2][j++];
                                adr = (UInt16)(int.Parse(register_adr));
                                mask = byte.Parse(bit_adr);
                            }
                            else
                            {
                                adr = (UInt16)(int.Parse(conf[2]));
                            }

                            #region Определение первого адреса регистра nci
                            if (needFind && var_name.Contains("nci"))
                            {
                                firstNciAddr = adr;
                                needFind = false;
                            }
                            #endregion

                            if (adr > endAddr)
                            {
                                endAddr = adr;
                                if (type == VariableType.Float) endAddr++;
                            }

                            VariableAccessLevel accessLevel = PlcIOHelper.GetEnum4Description<VariableAccessLevel>(conf[3]);
                            string description = PlcIOHelper.GetFormatDescription(conf[4]);
                            switch (type)
                            {
                                case VariableType.Bool:
                                    registers.Add(var_name, new BoolVariable(var_name, adr, mask, accessLevel, description));
                                    break;
                                case VariableType.Int:
                                    registers.Add(var_name, new IntVariable(var_name, adr, accessLevel, description));
                                    break;
                                case VariableType.Uint:
                                    registers.Add(var_name, new UIntVariable(var_name, adr, accessLevel, description));
                                    break;
                                case VariableType.Float:
                                    registers.Add(var_name, new FloatVariable(var_name, adr, accessLevel, description));
                                    break;
                                case VariableType.Array:
                                    registers.Add(var_name, new ArrayVariable(var_name, adr, array_size, accessLevel, description));
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _errorLog?.Invoke($"При чтении файла регистров возникло исключение: {e.Message}");
                    }
                }
            }
            _infoLog?.Invoke($"Чтение списка регистров произведено успешно. nci start: {firstNciAddr}");

            return new ParserResult<Variable>(startAddr, endAddr, firstNciAddr, registers);
        }
    }
}
