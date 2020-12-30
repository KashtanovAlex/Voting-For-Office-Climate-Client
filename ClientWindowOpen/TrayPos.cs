using System.Windows;

namespace ClientWindowOpen
{
    class TrayPos
    {
        public tpos pos;
        private int top;
        private int left;
        private DependencyProperty prop;
        private int end = 0;

        int pw = (int)SystemParameters.PrimaryScreenWidth;
        int ph = (int)SystemParameters.PrimaryScreenHeight;
        int waw = (int)SystemParameters.WorkArea.Width;
        int wah = (int)SystemParameters.WorkArea.Height;

        public void getXY(int width, int height, out int _top, out int _left, out DependencyProperty _prop, out int _end)
        {

            switch (pos)
            {
                case tpos.top:
                    left = pw - width;
                    top = (int)SystemParameters.WorkArea.Top - height;
                    prop = System.Windows.Window.TopProperty;
                    end = (int)SystemParameters.WorkArea.Top;
                    break;
                case tpos.left:
                    left = (int)SystemParameters.WorkArea.Left - width;
                    top = ph - height;
                    prop = System.Windows.Window.LeftProperty;
                    end = (int)SystemParameters.WorkArea.Left;
                    break;
                case tpos.right:
                    left = waw;
                    top = ph - height;
                    prop = System.Windows.Window.LeftProperty;
                    end = waw - width;
                    break;
                case tpos.bottom:
                default:
                    left = pw - width;
                    top = wah;
                    prop = System.Windows.Window.TopProperty;
                    end = wah - height;
                    break;
            }

            _left = left;
            _top = top;
            _prop = prop;
            _end = end;

        }

        public TrayPos()
        {



            if (SystemParameters.WorkArea.Top > 0)
            {
                pos = tpos.top;
            }
            else if (SystemParameters.WorkArea.Left > 0)
            {
                pos = tpos.left;
            }
            else if (pw > waw)
            {
                pos = tpos.right;
            }
            else pos = tpos.bottom;

            //MessageBox.Show(pos.ToString());

        }

    }
}
