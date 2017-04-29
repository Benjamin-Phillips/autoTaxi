using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace autoTaxi {
    public class ExitForm : Form {
        public ExitForm() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            ShowIcon = false;
            Text = "Finished running tests";
            Height = 0;
        }
    }
}
