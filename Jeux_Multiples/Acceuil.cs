using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    public partial class Acceuil : Form
    {
        public Acceuil()
        {
            InitializeComponent();
        }

        private void btnPuissance4_Click(object sender, EventArgs e)
        {
            Puissance_4 Formulaire = new Puissance_4(); Formulaire.Show();
        }

        private void btnMortPion_Click(object sender, EventArgs e)
        {
            mort_Pion Formulaire = new mort_Pion(); Formulaire.Show();
        }

        private void btnSnake_Click(object sender, EventArgs e)
        {
            Snake Formulaire = new Snake(); Formulaire.Show();
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnPker_Click(object sender, EventArgs e)
        {
            Poker Formulaire = new Poker(); Formulaire.Show();
        }

        private void btnBJ_Click(object sender, EventArgs e)
        {
            BlackJack Formulaire = new BlackJack(); Formulaire.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dame Formulaire = new Dame(); Formulaire.Show();
        }

        private void Acceuil_Load(object sender, EventArgs e)
        {

        }

        private void btnEchec_Click(object sender, EventArgs e)
        {
            Echec Formulaire = new Echec(); Formulaire.Show();
        }

        private void btn4cheveaux_Click(object sender, EventArgs e)
        {
            _4Cheveaux Formulaire = new _4Cheveaux(); Formulaire.Show();
        }
    }
}
