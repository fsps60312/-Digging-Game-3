using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Digging_Game_3
{
    public static class Keyboard
    {
        public static HashSet<Key> keyPressed = new HashSet<Key>();
        public static bool IsDown(Key key){ lock (keyPressed) return keyPressed.Contains(key); }
    }
}
