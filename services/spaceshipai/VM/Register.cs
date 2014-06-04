namespace SpaceshipAI.VM
{
    public class Register
    {
        private int _value;

        public int Lb
        {
            get { return (byte) (_value & 0xFF); }
            set { _value = (int) ((_value & 0xFFFFFF00) + (value & 0xFF)); }
        }

        public int Hb
        {
            get { return (byte) ((_value & 0xFF00) >> 8); }
            set { _value = (int) ((_value & 0xFFFF00FF) + ((value & 0xFF) << 8)); }
        }

        public int W
        {
            get { return (ushort) (_value & 0xFFFF); }
            set { _value = (int) ((_value & 0xFFFF0000) + (value & 0xFFFF)); }
        }

        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}