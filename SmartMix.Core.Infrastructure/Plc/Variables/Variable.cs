using SmartMix.Core.Infrastructure.Plc.Enums;
using SmartMix.Core.Infrastructure.Plc.Extensions;
using SmartMix.Core.Infrastructure.Plc.Helpers;

namespace SmartMix.Core.Infrastructure.Plc.Variables
{
    public interface IVariableValue<T>
    {
        T Value { get; set; }
        T OldValue { get; }
        /// <summary>
        /// Штамп последнего изменения
        /// </summary>
        long Timestamp { get; }
        /// <summary>
        /// Срабатывает, если значение меняется
        /// </summary>
        event EventHandler<T> Changed;
    }
    interface ICloneable<T>
    {
        /// <summary>
        /// Создать копию объекта
        /// </summary>
        /// <returns></returns>
        T Clone();
    }

    /// <summary>
    /// Класс описания базовой переменной контроллера PLC.
    /// </summary>
    public abstract class Variable
    {
        private byte[] _bytes = new byte[16];
        private byte[] _oldBytes;

        /// <summary>
        /// Переменная PLC
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="address">Начальный адрес</param>
        /// <param name="size">Размер, или количество адресов</param>
        /// <param name="type">Тип переменной <see cref="VariableType"/></param>
        /// <param name="accessLevel">Уровень доступа <see cref="VariableAccessLevel"/></param>
        public Variable(string name, ushort address, byte size, VariableType type, VariableAccessLevel accessLevel, string description = "")
        {
            if (size < 1) throw new ArgumentException($"Неверное значение размерности адреса регистра <{name}>", nameof(size));
            Name = name;
            Address = address;
            Size = size;
            Type = type;
            AccessLevel = accessLevel;
            Description = description;
        }
        public string Name { get; private set; }
        public ushort Address { get; private set; }
        public byte Size { get; private set; }
        public VariableType Type { get; private set; }

        /// <summary>
        /// Возвращает или задаёт уровень доступа к переменной.
        /// </summary>
        public VariableAccessLevel AccessLevel { get; private set; }
        public string Description { get; }
        public byte[] Bytes
        {
            get => _bytes;
            set
            {
                _oldBytes = _bytes;
                _bytes = value;
                OnChanged();
            }
        }
        public byte[] OldBytes => _oldBytes;
        public abstract void SetRawData(ushort[] data);
        public abstract ushort[] GetRawData();
        protected virtual void OnChanged() => SetRawData(Bytes.ToArrayUshorts());
    }

    public abstract class VariableBase<T> : Variable, IVariableValue<T>
    {
        private T _oldValue;
        private T _value;
        private long _timestamp;

        public VariableBase(string name, ushort address, byte size, VariableType type, VariableAccessLevel accessLevel, string description = "")
            : base(name, address, size, type, accessLevel, description)
        { }

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;

                _oldValue = _value;
                _value = value;
                _timestamp = DateTime.Now.Ticks;
                OnChanged(value);
            }
        }

        public T OldValue => _oldValue;
        public long Timestamp => _timestamp;

        public event EventHandler<T> Changed;

        protected virtual void OnChanged(T value)
        {
            Changed?.Invoke(this, value);
        }
    }

    public class BoolVariable : VariableBase<bool>, ICloneable<BoolVariable>
    {
        public static byte BoolSize = 1;

        public byte BitNumber { get; protected set; }
        public ushort Mask => (ushort)(1 << BitNumber);

        public BoolVariable(string name, ushort address, byte bitNum, VariableAccessLevel accessLevel, string description = "", bool value = false)
            : base(name, address, size: BoolSize, type: VariableType.Bool, accessLevel: accessLevel, description: description)
        {
            if (bitNum < 0 || bitNum > 15) throw new ArgumentException("Некорректное значение номера бита", nameof(bitNum));
            BitNumber = bitNum;
            Value = value;
        }

        public override void SetRawData(ushort[] data)
        {
            Value = (data[0] & Mask) != 0 ? true : false;
        }

        public override ushort[] GetRawData()
        {
            throw new NotImplementedException();
        }

        public BoolVariable Clone()
        {
            return new BoolVariable(Name, Address, BitNumber, AccessLevel, Description, Value);
        }
    }

    public class IntVariable : VariableBase<int>, ICloneable<IntVariable>
    {
        public static byte IntSize = 1;

        public IntVariable(string name, ushort address, VariableAccessLevel accessLevel, string description = "", int value = 0)
            : base(name, address, size: IntSize, type: VariableType.Int, accessLevel: accessLevel, description: description)
        {
            Value = value;
        }

        public override void SetRawData(ushort[] data)
        {
            Value = data[0];
        }

        public override ushort[] GetRawData() => Value.GetRaw();

        public IntVariable Clone()
        {
            return new IntVariable(Name, Address, AccessLevel, Description, Value);
        }
    }

    public class UIntVariable : VariableBase<uint>, ICloneable<UIntVariable>
    {
        public static byte UIntSize = 2;

        public UIntVariable(string name, ushort address, VariableAccessLevel accessLevel, string description = "", uint value = 0)
            : base(name, address, size: UIntSize, type: VariableType.Uint, accessLevel: accessLevel, description: description)
        {
            Value = value;
        }

        public override void SetRawData(ushort[] data)
        {
            Value = (uint)((data[1] << 16) + data[0]);
        }

        public override ushort[] GetRawData() => Value.GetRaw();

        public UIntVariable Clone()
        {
            return new UIntVariable(Name, Address, AccessLevel, Description, Value);
        }
    }

    public class FloatVariable : VariableBase<float>, ICloneable<FloatVariable>
    {
        public static byte FloatSize = 2;

        public FloatVariable(string name, ushort address, VariableAccessLevel accessLevel, string description = "", float value = 0f)
            : base(name, address, size: FloatSize, type: VariableType.Float, accessLevel: accessLevel, description: description)
        {
            Value = value;
        }

        public override void SetRawData(ushort[] data)
        {
            var value = (UInt32)((data[1] << 16) + data[0]);
            byte[] tempArr = BitConverter.GetBytes(value);
            Value = BitConverter.ToSingle(tempArr, 0);
        }

        public override ushort[] GetRawData() => Value.GetRaw();

        public FloatVariable Clone()
        {
            return new FloatVariable(Name, Address, AccessLevel, Description, Value);
        }
    }

    public class ArrayVariable : VariableBase<BitArray>, ICloneable<ArrayVariable>
    {
        private ArrayVariable(string name, ushort address, VariableAccessLevel accessLevel, ushort[] value, string description = "")
            : base(name, address, (byte)value.Length, type: VariableType.Array, accessLevel: accessLevel, description: description)
        {
            Value = new BitArray(value);
        }

        public ArrayVariable(string name, ushort address, byte size, VariableAccessLevel accessLevel, string description = "")
            : base(name, address, size, type: VariableType.Array, accessLevel: accessLevel, description: description)
        {
            Value = new BitArray(size);
        }

        public override void SetRawData(ushort[] data)
        {
            Value = new BitArray(data);
        }

        public void SetRawData(byte[] data)
        {
            SetRawData(PlcIOHelper.ConvertBytesToUshorts(data) ?? Array.Empty<ushort>());
        }

        public override ushort[] GetRawData()
        {
            return Value.GetCopyArray();
        }

        public ArrayVariable Clone()
        {
            return new ArrayVariable(Name, Address, AccessLevel, Value.GetCopyArray(), Description);
        }
    }


    public static class VariableExtensions
    {
        public static ushort[] GetRaw(this int value)
        {
            return new[] { (ushort)value };
        }

        public static ushort[] GetRaw(this uint value)
        {
            return new[] { (ushort)(value & 0xffff), (ushort)(value >> 16) };
        }

        public static ushort[] GetRaw(this float value)
        {
            var t = BitConverter.GetBytes(value);
            var t2 = BitConverter.ToUInt32(t, 0);

            var val_arr = new UInt16[2];
            val_arr[0] = (UInt16)(t2 & 0xffff);
            val_arr[1] = (UInt16)(t2 >> 16);

            return val_arr;
        }
    }
}
