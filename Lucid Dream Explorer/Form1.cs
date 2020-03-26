using System;
using System.Windows.Forms;

namespace Lucid_Dream_Explorer
{
    public partial class Form1 : Form
    {
        private static string[] maps = { "Bright Moon Cottage", "Pit & Temple", "Kyoto", "The Natural World", "Happy Town", "Violence District", "Moonlight Tower", "Temple Dojo", "Flesh Tunnels", "Clockwork Machines", "Long Hallway", "Sun Faces Heave", "Black Space", "Monument Park" };
        private static string[] sounds = { "00 - [Jangling]", "01 - [Opera Singer?]", "02 - [Jiggling]", "03 - Grass, Sand, H.T. Footstep", "04 - Train", "05 - Astronaut, Trumpeters", "06 - H.T. Squeak Footstep, Pterodactyl, Kissing Lips", "07 - Flowing Water (Natural World)", "08 - Link", "09 - H.T. Wood Footstep", "10 - Deep Rumbling", "11 - V.D. Dock Footstep", "12 - Wooden Bridge Footstep", "13 - [Tengu?]", "14 - Drumming (Kyoto)", "15 - H.T. Wet Footstep", "16 - [Tengu?]", "17 - [Clockwork Machine?]", "18 - Snarl (Lions, Dojo Dog)", "19 - Wings Flapping (Pterodactyl)", "20 - [Unknown]", "21 - Fetus Noise, Rocket Ship", "22 - Breathing (Sleeper in BMC)", "23 - BMC Birds Tweeting", "24 - [Long Hallway End?]", "25 - Horses Galloping (Natural World)", "26 - [UFO?]", "27 - Kyoto Grass Footstep", "28 - Generic Footstep (Tunnels, V.D., BMC Tile, NW Rock)", "29 - Water Footstep (NW River, Flesh Tunnel Slosh)" };

        private MemoryIO mio;

        public Form1()
        {
            InitializeComponent();
            foreach (string sound in sounds)
            {
                comboBox1.Items.Add(sound);
            }
            comboBox1.SelectedIndex = 0;

            timer1.Start();
        }

        private IntPtr? ReadDEbaseAdress()
        {
            IntPtr? addr = Emulator.baseAddr;

            /**
             * Failed with all emulators
             * Update labels and buttons
             **/
            if (addr == null)
            {
                //Hook failed!
            }
            else
            {
                if (Emulator.emulator == Emulator.PSXFIN) //psxfin: Update labels and buttons
                {
                    //aa
                }
                if (Emulator.emulator == Emulator.EPSXE) //ePSXe: Update labels and buttons
                {
                    //a
                }
            }
            return addr;
        }

        private int? ReadVal(int offset)
        {
            IntPtr? baseAddr = ReadDEbaseAdress();
            byte[] readBuf = new byte[4];

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return null;

            IntPtr mappointer = new IntPtr((int)baseAddr + offset);
            mio.MemoryRead(mappointer, readBuf);
            int map = BitConverter.ToInt32(readBuf, 0);
            if (Range.isInsideRange(Range.map, map)) return map;
            return null;
        }

        private int? UpdateReadValue(int offset)
        {
            if (mio == null)
            {
                mio = new MemoryIO(Emulator.emulator);
            }
            IntPtr? addr = Emulator.baseAddr;
            if (addr == null) return null;
            IntPtr pointer = new IntPtr((int)addr + offset);
            byte[] readBuf = new byte[4];
            mio.MemoryRead(pointer, readBuf);
            int value = BitConverter.ToInt32(readBuf, 0);
            return value;
        }

        //TODO: Use sender or e to get nud and offset
        private void ButtonSet(TextBox textBox, int offset)
        {

            IntPtr? baseAddr = ReadDEbaseAdress();
            if (baseAddr == null)
            {
                return; //Fatal error
            }

            if (textBox.Text == "")
            {
                return;
            }
            byte[] writeBuf = new byte[4];
            writeBuf = BitConverter.GetBytes(Convert.ToInt32(textBox.Text));

            MemoryIO mio = new MemoryIO(Emulator.emulator);
            if (!mio.processOK()) return;

            /**
             * Write position variable
             **/
            IntPtr pointer = new IntPtr((int)baseAddr + offset);
            mio.MemoryWrite(pointer, writeBuf);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //readmemandupdatelabel1
            if (ReadVal(Offset.map).HasValue)
            {
                int k = ReadVal(Offset.map).Value;

                //Map
                if (!textBox1.Focused)
                {
                    textBox1.Text = maps[k];
                }
                if (!textBox2.Focused)
                {
                    textBox2.Text = k.ToString();
                }
            }

            //Day
            if (!textBox3.Focused)
            {
                textBox3.Text = ReadVal(Offset.day).Value.ToString();
            }

            //Timer
            textBox4.Text = UpdateReadValue(0x8AC70).Value.ToString();

            //Events
            textBox8.Text = UpdateReadValue(0x91680).Value.ToString();
            textBox9.Text = UpdateReadValue(0x91674).Value.ToString();
            textBox10.Text = UpdateReadValue(0x91678).Value.ToString();
            textBox11.Text = UpdateReadValue(0x9167c).Value.ToString();

            if (UpdateReadValue(Offset.X).HasValue)
            {
                //Position
                if (!textBox5.Focused)
                {
                    textBox5.Text = UpdateReadValue(Offset.X).Value.ToString();
                }
                if (!textBox6.Focused)
                {
                    textBox6.Text = UpdateReadValue(Offset.Y).Value.ToString();
                }
                if (!textBox7.Focused)
                {
                    textBox7.Text = UpdateReadValue(Offset.Z).Value.ToString();
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.map);
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.map);
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.day);
            }
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.X);
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void textBox7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.Z);
            }
        }

        private void textBox6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSet((TextBox)sender, Offset.Y);
            }
        }
    }
}