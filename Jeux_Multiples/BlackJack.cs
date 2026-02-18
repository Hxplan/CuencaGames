using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    #region Modeles
    public enum BJCouleur { Trefle, Carreau, Coeur, Pique }

    public class BJCarte
    {
        public BJCouleur Couleur { get; private set; }
        public int Valeur { get; private set; }
        public bool FaceVisible { get; set; }
        public float AnimX { get; set; }
        public float AnimY { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public bool Animating { get; set; }

        public BJCarte(BJCouleur c, int v) { Couleur = c; Valeur = v; }

        public string Symbole
        {
            get
            {
                if (Couleur == BJCouleur.Trefle) return "\u2663";
                if (Couleur == BJCouleur.Carreau) return "\u2666";
                if (Couleur == BJCouleur.Coeur) return "\u2665";
                return "\u2660";
            }
        }

        public string ValeurTexte
        {
            get
            {
                if (Valeur == 1) return "A";
                if (Valeur == 11) return "V";
                if (Valeur == 12) return "D";
                if (Valeur == 13) return "R";
                return Valeur.ToString();
            }
        }

        public bool EstRouge => Couleur == BJCouleur.Coeur || Couleur == BJCouleur.Carreau;

        public int ValeurBJ
        {
            get
            {
                if (Valeur == 1) return 11;
                if (Valeur >= 10) return 10;
                return Valeur;
            }
        }
    }

    public class BJPaquet
    {
        private readonly List<BJCarte> _cartes = new List<BJCarte>();
        private readonly Random _rng = new Random();

        public BJPaquet(int nbPaquets = 2)
        {
            for (int p = 0; p < nbPaquets; p++)
                foreach (BJCouleur c in Enum.GetValues(typeof(BJCouleur)))
                    for (int v = 1; v <= 13; v++)
                        _cartes.Add(new BJCarte(c, v));
        }

        public void Melanger()
        {
            for (int i = _cartes.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var t = _cartes[i]; _cartes[i] = _cartes[j]; _cartes[j] = t;
            }
        }

        public BJCarte Tirer(bool faceVisible = true)
        {
            BJCarte c = _cartes[0]; _cartes.RemoveAt(0); c.FaceVisible = faceVisible; return c;
        }

        public int NbCartes => _cartes.Count;
    }

    public static class BJCalcul
    {
        public static int Score(List<BJCarte> main)
        {
            int total = 0, aces = 0;
            foreach (BJCarte c in main)
            {
                if (!c.FaceVisible) continue;
                if (c.Valeur == 1) { aces++; total += 11; }
                else if (c.Valeur >= 10) total += 10;
                else total += c.Valeur;
            }
            while (total > 21 && aces > 0) { total -= 10; aces--; }
            return total;
        }

        public static int ScoreComplet(List<BJCarte> main)
        {
            int total = 0, aces = 0;
            foreach (BJCarte c in main)
            {
                if (c.Valeur == 1) { aces++; total += 11; }
                else if (c.Valeur >= 10) total += 10;
                else total += c.Valeur;
            }
            while (total > 21 && aces > 0) { total -= 10; aces--; }
            return total;
        }

        public static bool EstBlackjack(List<BJCarte> main) =>
            main.Count == 2 && ScoreComplet(main) == 21;
    }

    public class PartieRecord
    {
        public string Resultat { get; set; }
        public int Mise { get; set; }
        public int Gain { get; set; }
    }
    #endregion

    #region Formulaire
    public partial class BlackJack : Form
    {
        // ── Palette casino raffinée ───────────────────────────────
        static readonly Color C_FOND = Color.FromArgb(5, 20, 5);
        static readonly Color C_TAPIS = Color.FromArgb(18, 78, 32);
        static readonly Color C_TAPIS2 = Color.FromArgb(10, 50, 18);
        static readonly Color C_OR = Color.FromArgb(255, 215, 70);
        static readonly Color C_OR2 = Color.FromArgb(195, 150, 20);
        static readonly Color C_TEXTE = Color.FromArgb(245, 240, 220);
        static readonly Color C_ROUGE = Color.FromArgb(220, 50, 50);
        static readonly Color C_VERT = Color.FromArgb(50, 210, 80);
        static readonly Color C_BLEU = Color.FromArgb(60, 140, 230);
        static readonly Color C_VIOLET = Color.FromArgb(155, 75, 220);
        static readonly Color C_ORANGE = Color.FromArgb(235, 150, 30);

        // ── Polices ───────────────────────────────────────────────
        static readonly Font F_TITRE = new Font("Georgia", 22, FontStyle.Bold | FontStyle.Italic);
        static readonly Font F_UI = new Font("Segoe UI", 11, FontStyle.Bold);
        static readonly Font F_UI_SM = new Font("Segoe UI", 10);
        static readonly Font F_MSG = new Font("Georgia", 19, FontStyle.Bold | FontStyle.Italic);
        static readonly Font F_VAL = new Font("Georgia", 13, FontStyle.Bold);
        static readonly Font F_SYM_SM = new Font("Segoe UI Symbol", 9);
        static readonly Font F_SYM_LG = new Font("Segoe UI Symbol", 16, FontStyle.Bold);
        static readonly Font F_BTN = new Font("Segoe UI", 10, FontStyle.Bold);
        static readonly Font F_STAT = new Font("Segoe UI", 9);
        static readonly Font F_HIST = new Font("Consolas", 9);
        static readonly Font F_CHIP = new Font("Segoe UI", 9, FontStyle.Bold);

        // ── Dimensions ───────────────────────────────────────────
        const int CW = 72;
        const int CH = 108;
        const int CGAP = 10;
        const int HIST_W = 220;
        const int BAS_H = 160;   // hauteur panneau bas

        // ── État ─────────────────────────────────────────────────
        private BJPaquet _paquet;
        private List<BJCarte> _mainJoueur = new List<BJCarte>();
        private List<BJCarte> _mainJoueur2 = new List<BJCarte>();
        private List<BJCarte> _mainCroupier = new List<BJCarte>();
        private bool _splitActif = false;
        private bool _joueurSurMain2 = false;
        private int _miseMain2 = 0;
        private int _jetons = 1000;
        private int _mise = 0;
        private string _message = "";
        private string _sousMessage = "";
        private bool _enCours = false;
        private bool _attenteMise = true;
        private bool _peutDoubler = false;
        private bool _peutSplitter = false;
        private bool _peutAssurance = false;
        private bool _assurancePrise = false;
        private int _miseAssurance = 0;

        // Stats
        private int _parties = 0;
        private int _victoires = 0;
        private int _defaites = 0;
        private int _egalites = 0;
        private int _blackjacks = 0;

        private readonly List<PartieRecord> _historique = new List<PartieRecord>();
        private bool _showHistorique = false;

        // Timers
        private Timer _timerIA;
        private Timer _timerAnim;
        private bool _iaEnCours = false;

        // ── Contrôles ─────────────────────────────────────────────
        private Button _btnAccueil;
        private Button _btnMise10, _btnMise25, _btnMise50, _btnMise100, _btnMise200;
        private Button _btnEffacer, _btnDeal;
        private Button _btnTirer, _btnRester, _btnDoubler, _btnSplit;
        private Button _btnAssurance, _btnRefuserAssurance;
        private Button _btnRejouer;
        private Button _btnHistorique;
        private Panel _pnlBas;       // fond du bas
        private Panel _pnlMiseZone;  // chips + deal (visible avant deal)
        private Panel _pnlJeuZone;   // tirer/rester/doubler/split (visible pendant jeu)
        private Panel _pnlHistorique;

        [DllImport("kernel32.dll")]
        static extern bool Beep(int freq, int dur);

        // ════════════════════════════════════════════════════════
        public string Joueur = "Joueur";

        // ════════════════════════════════════════════════════════
        public BlackJack(string joueur)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(joueur)) Joueur = joueur;
            Text = $"\u2660 BlackJack Pro ({Joueur}) \u2665";
            Size = new Size(1060, 760);
            MinimumSize = new Size(1060, 760);
            BackColor = C_FOND;
            DoubleBuffered = true;
            BuildUI();
            BuildTimers();
            _message = "Placez votre mise pour commencer !";
            _sousMessage = "Jetons disponibles : " + _jetons;
            AfficherPhase(Phase.Mise);
        }

        public BlackJack() : this("Joueur") { }

        private enum Phase { Mise, Jeu, Assurance, Fin }

        private void BlackJack_Load(object sender, EventArgs e) { }
        #endregion

    #region Construction UI
        private void BuildUI()
        {
            // ── Fond bas ──────────────────────────────────────────
            _pnlBas = new Panel
            {
                Bounds = new Rectangle(0, Height - BAS_H, Width, BAS_H),
                BackColor = Color.FromArgb(8, 20, 8)
            };
            _pnlBas.Paint += PaintPanneauBas;
            Controls.Add(_pnlBas);

            // ── Zone mise (chips + deal) ──────────────────────────
            _pnlMiseZone = new Panel
            {
                Bounds = new Rectangle(0, 0, Width, BAS_H),
                BackColor = Color.Transparent
            };
            _pnlBas.Controls.Add(_pnlMiseZone);

            // Chips dans la zone mise
            int bx = 20;
            _btnMise10 = CreerChip("+10", bx, 48, Color.FromArgb(185, 45, 45)); bx += 76;
            _btnMise25 = CreerChip("+25", bx, 48, Color.FromArgb(45, 75, 185)); bx += 76;
            _btnMise50 = CreerChip("+50", bx, 48, Color.FromArgb(155, 100, 10)); bx += 76;
            _btnMise100 = CreerChip("+100", bx, 48, Color.FromArgb(45, 130, 50)); bx += 76;
            _btnMise200 = CreerChip("+200", bx, 48, Color.FromArgb(135, 55, 195)); bx += 76;
            _btnEffacer = CreerChip("\u232b Effacer", bx, 48, Color.FromArgb(80, 30, 30));

            _btnMise10.Click += (s, e) => AjouterMise(10);
            _btnMise25.Click += (s, e) => AjouterMise(25);
            _btnMise50.Click += (s, e) => AjouterMise(50);
            _btnMise100.Click += (s, e) => AjouterMise(100);
            _btnMise200.Click += (s, e) => AjouterMise(200);
            _btnEffacer.Click += (s, e) => EffacerMise();

            foreach (var b in new[] { _btnMise10, _btnMise25, _btnMise50, _btnMise100, _btnMise200, _btnEffacer })
                _pnlMiseZone.Controls.Add(b);

            // Bouton DEAL — bien visible à droite
            _btnDeal = CreerBoutonAction("\u25ce  DEAL", 580, 38, 150, 68,
                Color.FromArgb(210, 170, 20), Color.Black);
            _btnDeal.Font = new Font("Georgia", 14, FontStyle.Bold);
            _btnDeal.Click += (s, e) => Distribuer();
            _pnlMiseZone.Controls.Add(_btnDeal);

            // ── Zone jeu (tirer / rester / doubler / split) ───────
            _pnlJeuZone = new Panel
            {
                Bounds = new Rectangle(0, 0, Width, BAS_H),
                BackColor = Color.Transparent,
                Visible = false
            };
            _pnlBas.Controls.Add(_pnlJeuZone);

            // Boutons d'action jeu — grands et contrastés
            int gx = 20;
            _btnTirer = CreerBoutonAction("\u25bc TIRER", gx, 40, 165, 72, Color.FromArgb(40, 185, 65), Color.Black); gx += 178;
            _btnRester = CreerBoutonAction("\u25a0 RESTER", gx, 40, 165, 72, Color.FromArgb(205, 45, 45), Color.White); gx += 178;
            _btnDoubler = CreerBoutonAction("\u00d72 DOUBLER", gx, 40, 165, 72, Color.FromArgb(45, 125, 220), Color.White); gx += 178;
            _btnSplit = CreerBoutonAction("\u2194 SPLIT", gx, 40, 165, 72, Color.FromArgb(145, 60, 210), Color.White);

            _btnTirer.Font = new Font("Georgia", 13, FontStyle.Bold);
            _btnRester.Font = new Font("Georgia", 13, FontStyle.Bold);
            _btnDoubler.Font = new Font("Georgia", 12, FontStyle.Bold);
            _btnSplit.Font = new Font("Georgia", 13, FontStyle.Bold);

            _btnTirer.Click += (s, e) => Tirer();
            _btnRester.Click += (s, e) => Rester();
            _btnDoubler.Click += (s, e) => Doubler();
            _btnSplit.Click += (s, e) => Split();

            foreach (var b in new[] { _btnTirer, _btnRester, _btnDoubler, _btnSplit })
                _pnlJeuZone.Controls.Add(b);

            // Boutons assurance — dans la zone jeu
            _btnAssurance = CreerBoutonAction("\u2714 ASSURANCE", 600, 38, 165, 52,
                Color.FromArgb(175, 145, 25), Color.Black);
            _btnRefuserAssurance = CreerBoutonAction("\u2716 REFUSER", 775, 38, 145, 52,
                Color.FromArgb(130, 45, 45), Color.White);
            _btnAssurance.Click += (s, e) => PrendreAssurance();
            _btnRefuserAssurance.Click += (s, e) => RefuserAssurance();
            _btnAssurance.Visible = false;
            _btnRefuserAssurance.Visible = false;
            _pnlJeuZone.Controls.Add(_btnAssurance);
            _pnlJeuZone.Controls.Add(_btnRefuserAssurance);

            // Bouton Nouvelle Partie — dans zone jeu (fin de round)
            _btnRejouer = CreerBoutonAction("\u21bb NOUVELLE PARTIE", 580, 38, 230, 72,
                Color.FromArgb(185, 145, 30), Color.Black);
            _btnRejouer.Font = new Font("Georgia", 13, FontStyle.Bold);
            _btnRejouer.Visible = false;
            _btnRejouer.Click += (s, e) => NouvellePartie();
            _pnlJeuZone.Controls.Add(_btnRejouer);

            // ── Boutons persistants (toujours visibles) ───────────
            _btnHistorique = CreerBoutonAction("\u2630 HISTORIQUE", Width - 155, 10, 138, 32,
                Color.FromArgb(30, 60, 30), Color.FromArgb(180, 210, 150));
            _btnHistorique.Font = F_BTN;
            _btnHistorique.Click += (s, e) => ToggleHistorique();
            Controls.Add(_btnHistorique);

            _btnAccueil = CreerBoutonAction("\u2302 ACCUEIL", 10, 10, 120, 34,
                Color.FromArgb(50, 80, 50), Color.FromArgb(200, 230, 180));
            _btnAccueil.Font = F_BTN;
            _btnAccueil.Click += BtnAccueil_Click;
            Controls.Add(_btnAccueil);
            _btnAccueil.BringToFront();
            _btnHistorique.BringToFront();

            // ── Panneau historique ────────────────────────────────
            _pnlHistorique = new Panel
            {
                Bounds = new Rectangle(Width - HIST_W - 10, 52, HIST_W, Height - 220),
                BackColor = Color.FromArgb(10, 28, 10),
                Visible = false
            };
            _pnlHistorique.Paint += PaintHistorique;
            Controls.Add(_pnlHistorique);

            Resize += (s, e) => RecalcLayout();
        }

        private void RecalcLayout()
        {
            _pnlBas.Width = Width;
            _pnlBas.Top = Height - BAS_H;
            _pnlMiseZone.Width = Width;
            _pnlJeuZone.Width = Width;
            _btnHistorique.Left = Width - 155;
            _pnlHistorique.Left = Width - HIST_W - 10;
            _pnlHistorique.Height = Height - 220;
            Invalidate();
            _pnlHistorique.Invalidate();
        }

        // ── Chip ronde ────────────────────────────────────────────
        private Button CreerChip(string txt, int x, int y, Color bg)
        {
            var b = new Button
            {
                Text = txt,
                Location = new Point(x, y),
                Size = new Size(66, 66),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = F_CHIP,
                BackColor = bg,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = ControlPaint.Light(bg, 0.7f);
            b.FlatAppearance.BorderSize = 3;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.45f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bg, 0.2f);
            b.Region = new Region(EllipsePath(b.ClientRectangle));
            return b;
        }

        // ── Bouton action rectangulaire avec coins arrondis ───────
        private Button CreerBoutonAction(string txt, int x, int y, int w, int h, Color bg, Color fg)
        {
            var b = new Button
            {
                Text = txt,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                ForeColor = fg,
                Font = F_BTN,
                BackColor = bg,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.35f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bg, 0.15f);
            // Coins légèrement arrondis via Region
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int r = 10;
            path.AddArc(0, 0, r * 2, r * 2, 180, 90);
            path.AddArc(w - r * 2, 0, r * 2, r * 2, 270, 90);
            path.AddArc(w - r * 2, h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(0, h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            b.Region = new Region(path);
            return b;
        }

        private GraphicsPath EllipsePath(Rectangle r)
        {
            var p = new GraphicsPath(); p.AddEllipse(r); return p;
        }

        private void BuildTimers()
        {
            _timerIA = new Timer { Interval = 750 };
            _timerAnim = new Timer { Interval = 16 };
            _timerIA.Tick += TimerIA_Tick;
            _timerAnim.Tick += TimerAnim_Tick;
        }

        // ── Gestion des phases (quelle zone est visible) ──────────
        private void AfficherPhase(Phase phase)
        {
            _pnlMiseZone.Visible = false;
            _pnlJeuZone.Visible = false;
            _btnTirer.Visible = false;
            _btnRester.Visible = false;
            _btnDoubler.Visible = false;
            _btnSplit.Visible = false;
            _btnAssurance.Visible = false;
            _btnRefuserAssurance.Visible = false;
            _btnRejouer.Visible = false;

            switch (phase)
            {
                case Phase.Mise:
                    _pnlMiseZone.Visible = true;
                    break;
                case Phase.Jeu:
                    _pnlJeuZone.Visible = true;
                    _btnTirer.Visible = true;
                    _btnRester.Visible = true;
                    _btnDoubler.Visible = _peutDoubler;
                    _btnSplit.Visible = _peutSplitter;
                    break;
                case Phase.Assurance:
                    _pnlJeuZone.Visible = true;
                    _btnAssurance.Visible = true;
                    _btnRefuserAssurance.Visible = true;
                    break;
                case Phase.Fin:
                    _pnlJeuZone.Visible = true;
                    _btnRejouer.Visible = true;
                    break;
            }
        }
        #endregion

    #region Animation
        private void LancerAnimation(BJCarte carte, float fromX, float fromY, float toX, float toY)
        {
            carte.AnimX = fromX; carte.AnimY = fromY;
            carte.TargetX = toX; carte.TargetY = toY;
            carte.Animating = true;
            if (!_timerAnim.Enabled) _timerAnim.Start();
        }

        private void TimerAnim_Tick(object sender, EventArgs e)
        {
            bool anyActive = false; float speed = 0.22f;
            foreach (var c in _mainJoueur) AnimStep(c, speed, ref anyActive);
            foreach (var c in _mainJoueur2) AnimStep(c, speed, ref anyActive);
            foreach (var c in _mainCroupier) AnimStep(c, speed, ref anyActive);
            Invalidate();
            if (!anyActive) _timerAnim.Stop();
        }

        private void AnimStep(BJCarte c, float speed, ref bool anyActive)
        {
            if (!c.Animating) return;
            float dx = c.TargetX - c.AnimX, dy = c.TargetY - c.AnimY;
            if (Math.Abs(dx) < 1.5f && Math.Abs(dy) < 1.5f)
            { c.AnimX = c.TargetX; c.AnimY = c.TargetY; c.Animating = false; }
            else
            { c.AnimX += dx * speed; c.AnimY += dy * speed; anyActive = true; }
        }

        private void AjouterCarteAnimee(List<BJCarte> main, BJCarte carte, bool estCroupier)
        {
            main.Add(carte);
            int idx = main.Count - 1;
            float tx, ty;

            if (_splitActif && !estCroupier)
            {
                int sx = (main == _mainJoueur) ? Width / 2 - 280 : Width / 2 + 20;
                tx = sx + idx * (CW + CGAP);
            }
            else
            {
                int total = main.Count * CW + (main.Count - 1) * CGAP;
                int startX = Width / 2 - total / 2;
                tx = startX + idx * (CW + CGAP);
            }
            ty = estCroupier ? Height / 2 - 200f : Height / 2 + 30f;
            LancerAnimation(carte, Width / 2f, -60f, tx, ty);
        }
        #endregion

    #region Dessin
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Tapis dégradé
            using (var br = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), C_TAPIS, C_TAPIS2, 125f))
                g.FillRectangle(br, 0, 0, Width, Height);

            // Points décoratifs
            using (var brPt = new SolidBrush(Color.FromArgb(12, 255, 255, 255)))
                for (int px = 30; px < Width; px += 45)
                    for (int py = 60; py < Height - 170; py += 45)
                        g.FillEllipse(brPt, px, py, 3, 3);

            // Ellipse centrale décorative
            using (var pen = new Pen(Color.FromArgb(40, C_OR), 2.5f))
                g.DrawEllipse(pen, Width / 2 - 340, Height / 2 - 200, 680, 400);
            using (var pen = new Pen(Color.FromArgb(18, C_OR), 1f))
                g.DrawEllipse(pen, Width / 2 - 360, Height / 2 - 218, 720, 436);

            // Titre
            string titre = "\u2660 BLACK JACK \u2665";
            SizeF szT = g.MeasureString(titre, F_TITRE);
            float txP = Width / 2f - szT.Width / 2f;
            using (var brG = new LinearGradientBrush(new RectangleF(txP, 8, szT.Width, szT.Height), C_OR, C_OR2, 90f))
                g.DrawString(titre, F_TITRE, brG, txP, 8);

            // Badges jetons et mise
            DrawBadge(g, "\u25cf Jetons : " + _jetons, 148, 14, _jetons < 200 ? C_ROUGE : C_VERT);
            if (_mise > 0)
            {
                string mt = "\u25b2 Mise : " + _mise;
                SizeF smSz = g.MeasureString(mt, F_UI);
                DrawBadge(g, mt, Width - smSz.Width - 55, 14, C_OR);
            }

            // Statistiques
            DrawStats(g);

            // Indicateur split actif
            if (_splitActif && _enCours)
            {
                string indic = _joueurSurMain2 ? "Main 2 \u25ba" : "\u25c4 Main 1";
                SizeF si = g.MeasureString(indic, F_UI_SM);
                DrawBadge(g, indic, Width / 2f - si.Width / 2f, Height / 2f - 12, C_VIOLET);
            }

            // Mains croupier
            DrawLabel(g, "CROUPIER", Height / 2 - 230);
            DrawMains(g, _mainCroupier, Height / 2 - 200, true);
            if (_mainCroupier.Count > 0)
                DrawScoreBadge(g, _mainCroupier, Height / 2 - 200);

            // Mains joueur
            if (_splitActif)
                DrawSplitMains(g);
            else
            {
                DrawLabel(g, Joueur.ToUpper(), Height / 2 + 10);
                DrawMains(g, _mainJoueur, Height / 2 + 30, false);
                if (_mainJoueur.Count > 0)
                    DrawScoreBadge(g, _mainJoueur, Height / 2 + 30);
            }

            // Messages
            if (!string.IsNullOrEmpty(_message))
                DrawMessage(g, _message, Height / 2f - 22, F_MSG, C_OR);
            if (!string.IsNullOrEmpty(_sousMessage))
                DrawMessage(g, _sousMessage, Height / 2f + 18, F_UI_SM, C_TEXTE);
            if (_peutAssurance && !_assurancePrise && _btnAssurance.Visible)
                DrawMessage(g, "\u26a0 Croupier a un As ! Prendre l'assurance ?", Height / 2f - 54, F_UI_SM, C_ORANGE);
        }

        private void DrawBadge(Graphics g, string txt, float x, float y, Color col)
        {
            SizeF sz = g.MeasureString(txt, F_UI);
            FillRoundedRect(g, new SolidBrush(Color.FromArgb(140, 0, 0, 0)), x - 10, y - 5, sz.Width + 20, sz.Height + 10, 10);
            using (var pen = new Pen(Color.FromArgb(60, col), 1f))
                DrawRoundedRect(g, pen, x - 10, y - 5, sz.Width + 20, sz.Height + 10, 10);
            g.DrawString(txt, F_UI, new SolidBrush(col), x, y);
        }

        private void DrawStats(Graphics g)
        {
            float wr = _parties > 0 ? (_victoires * 100f / _parties) : 0f;
            string stats = $"V:{_victoires}  D:{_defaites}  E:{_egalites}  BJ:{_blackjacks}  WR:{wr:0.0}%";
            SizeF sz = g.MeasureString(stats, F_STAT);
            FillRoundedRect(g, new SolidBrush(Color.FromArgb(110, 0, 0, 0)),
                Width / 2f - sz.Width / 2f - 12, Height - BAS_H - 26, sz.Width + 24, sz.Height + 8, 8);
            g.DrawString(stats, F_STAT, new SolidBrush(Color.FromArgb(190, 255, 255, 255)),
                Width / 2f - sz.Width / 2f, Height - BAS_H - 23);
        }

        private void DrawLabel(Graphics g, string txt, float y)
        {
            g.DrawString(txt, F_UI_SM, new SolidBrush(Color.FromArgb(120, 255, 255, 255)), 82, y);
        }

        private void DrawMains(Graphics g, List<BJCarte> main, int y, bool estCroupier)
        {
            if (main.Count == 0) return;
            int total = main.Count * CW + (main.Count - 1) * CGAP;
            int startX = Width / 2 - total / 2;
            for (int i = 0; i < main.Count; i++)
            {
                var c = main[i];
                float cx = c.Animating ? c.AnimX : startX + i * (CW + CGAP);
                float cy = c.Animating ? c.AnimY : y;
                DrawCarte(g, c, (int)cx, (int)cy);
            }
        }

        private void DrawSplitMains(Graphics g)
        {
            int y = Height / 2 + 30;
            int x1 = Width / 2 - 285, x2 = Width / 2 + 18;

            int s1 = _mainJoueur.Count * CW + (_mainJoueur.Count - 1) * CGAP;
            int s2 = _mainJoueur2.Count * CW + (_mainJoueur2.Count - 1) * CGAP;

            for (int i = 0; i < _mainJoueur.Count; i++)
            {
                var c = _mainJoueur[i];
                float cx = c.Animating ? c.AnimX : x1 + i * (CW + CGAP);
                DrawCarte(g, c, (int)cx, y, !_joueurSurMain2 && _enCours);
            }
            if (_mainJoueur.Count > 0) DrawScoreBadgeSplit(g, _mainJoueur, x1, y, s1);

            // Séparateur
            using (var pen = new Pen(Color.FromArgb(55, C_OR), 1.5f))
                g.DrawLine(pen, Width / 2, y - 12, Width / 2, y + CH + 12);

            for (int i = 0; i < _mainJoueur2.Count; i++)
            {
                var c = _mainJoueur2[i];
                float cx = c.Animating ? c.AnimX : x2 + i * (CW + CGAP);
                DrawCarte(g, c, (int)cx, y, _joueurSurMain2 && _enCours);
            }
            if (_mainJoueur2.Count > 0) DrawScoreBadgeSplit(g, _mainJoueur2, x2, y, s2);

            g.DrawString("MAIN 1", F_UI_SM, new SolidBrush(Color.FromArgb(120, 255, 255, 255)), x1, Height / 2 + 8);
            g.DrawString("MAIN 2", F_UI_SM, new SolidBrush(Color.FromArgb(120, 255, 255, 255)), x2, Height / 2 + 8);
        }

        private void DrawScoreBadge(Graphics g, List<BJCarte> main, int carteY)
        {
            int sc = BJCalcul.Score(main);
            if (sc == 0) return;
            bool bust = sc > 21;
            bool bj = BJCalcul.EstBlackjack(main);
            Color col = bj ? C_OR : (bust ? C_ROUGE : C_TEXTE);
            string txt = bj ? "21 \u2605 BJ!" : (bust ? sc + " Bust!" : sc.ToString());
            int total = main.Count * CW + (main.Count - 1) * CGAP;
            int bx = Width / 2 - total / 2 + total + 14;
            SizeF sz = g.MeasureString(txt, F_UI);
            FillRoundedRect(g, new SolidBrush(Color.FromArgb(195, 0, 0, 0)), bx - 6, carteY + CH / 2 - 14, sz.Width + 12, sz.Height + 8, 8);
            g.DrawString(txt, F_UI, new SolidBrush(col), bx, carteY + CH / 2 - 10);
        }

        private void DrawScoreBadgeSplit(Graphics g, List<BJCarte> main, int startX, int carteY, int totalW)
        {
            int sc = BJCalcul.Score(main);
            if (sc == 0) return;
            bool bust = sc > 21, bj = BJCalcul.EstBlackjack(main);
            Color col = bj ? C_OR : (bust ? C_ROUGE : C_TEXTE);
            string txt = bj ? "21\u2605" : (bust ? sc + "!" : sc.ToString());
            SizeF sz = g.MeasureString(txt, F_UI_SM);
            FillRoundedRect(g, new SolidBrush(Color.FromArgb(195, 0, 0, 0)), startX + totalW + 6, carteY + CH / 2 - 12, sz.Width + 10, sz.Height + 6, 6);
            g.DrawString(txt, F_UI_SM, new SolidBrush(col), startX + totalW + 8, carteY + CH / 2 - 9);
        }

        private void DrawMessage(Graphics g, string txt, float y, Font font, Color col)
        {
            SizeF sz = g.MeasureString(txt, font);
            float x = Width / 2f - sz.Width / 2f;
            FillRoundedRect(g, new SolidBrush(Color.FromArgb(210, 0, 0, 0)), x - 20, y - 10, sz.Width + 40, sz.Height + 20, 14);
            using (var pen = new Pen(Color.FromArgb(50, col), 1f))
                DrawRoundedRect(g, pen, x - 20, y - 10, sz.Width + 40, sz.Height + 20, 14);
            g.DrawString(txt, font, new SolidBrush(col), x, y);
        }

        private void DrawCarte(Graphics g, BJCarte carte, int x, int y, bool surligner = false)
        {
            // Ombre
            using (var brO = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                FillRoundedRect(g, brO, x + 4, y + 6, CW, CH, 8);

            if (!carte.FaceVisible)
            {
                using (var brD = new LinearGradientBrush(new Rectangle(x, y, CW, CH),
                    Color.FromArgb(20, 40, 150), Color.FromArgb(8, 16, 80), 45f))
                    FillRoundedRect(g, brD, x, y, CW, CH, 8);
                // Motif dos
                using (var pm = new Pen(Color.FromArgb(50, 100, 210), 1.3f))
                    for (int iy = y + 8; iy < y + CH - 6; iy += 12)
                        for (int ix = x + 5; ix < x + CW - 4; ix += 12)
                            g.DrawRectangle(pm, ix, iy, 5, 5);
                using (var pb = new Pen(Color.FromArgb(75, 120, 225), 2f))
                    DrawRoundedRect(g, pb, x, y, CW, CH, 8);
                return;
            }

            // Face
            Color bgc = surligner ? Color.FromArgb(255, 248, 215) : Color.FromArgb(252, 248, 242);
            FillRoundedRect(g, new SolidBrush(bgc), x, y, CW, CH, 8);

            var border = surligner ? new Pen(C_OR, 2.8f) : new Pen(Color.FromArgb(90, 0, 0, 0), 1.5f);
            DrawRoundedRect(g, border, x, y, CW, CH, 8);
            border.Dispose();

            if (surligner)
            {
                using (var penGlow = new Pen(Color.FromArgb(80, C_OR), 5f))
                    DrawRoundedRect(g, penGlow, x - 2, y - 2, CW + 4, CH + 4, 10);
            }

            Color cC = carte.EstRouge ? Color.FromArgb(200, 25, 25) : Color.FromArgb(15, 15, 15);
            using (var brC = new SolidBrush(cC))
            {
                g.DrawString(carte.ValeurTexte, F_VAL, brC, x + 4, y + 2);
                g.DrawString(carte.Symbole, F_SYM_SM, brC, x + 4, y + 20);
                SizeF szS = g.MeasureString(carte.Symbole, F_SYM_LG);
                g.DrawString(carte.Symbole, F_SYM_LG, brC,
                    x + (CW - szS.Width) / 2f, y + (CH - szS.Height) / 2f);
                // Coin bas-droit (retourné)
                var st = g.Save();
                g.TranslateTransform(x + CW - 4, y + CH - 4);
                g.RotateTransform(180);
                g.DrawString(carte.ValeurTexte, F_VAL, brC, 0, 0);
                g.DrawString(carte.Symbole, F_SYM_SM, brC, 0, 19);
                g.Restore(st);
            }
        }

        private void PaintPanneauBas(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            using (var br = new LinearGradientBrush(_pnlBas.ClientRectangle,
                Color.FromArgb(12, 30, 12), Color.FromArgb(5, 14, 5), 90f))
                g.FillRectangle(br, _pnlBas.ClientRectangle);

            // Ligne séparatrice dorée
            using (var pen = new Pen(Color.FromArgb(70, C_OR), 1.5f))
                g.DrawLine(pen, 0, 0, _pnlBas.Width, 0);

            // Indicateurs mise en phase mise
            if (_pnlMiseZone.Visible && _mise > 0)
            {
                string mt = "Mise actuelle : " + _mise;
                SizeF sz = g.MeasureString(mt, F_UI);
                float bx = _pnlBas.Width - 200;
                g.DrawString(mt, F_UI_SM, new SolidBrush(C_OR), bx, 22);
            }
        }

        private void PaintHistorique(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            FillRoundedRect(g, new SolidBrush(Color.FromArgb(225, 6, 20, 6)),
                0, 0, _pnlHistorique.Width, _pnlHistorique.Height, 10);
            using (var pen = new Pen(Color.FromArgb(65, C_OR), 1.5f))
                DrawRoundedRect(g, pen, 0, 0, _pnlHistorique.Width, _pnlHistorique.Height, 10);
            g.DrawString("HISTORIQUE", F_UI, new SolidBrush(C_OR), 12, 8);

            int yPos = 34, start = Math.Max(0, _historique.Count - 14);
            for (int i = _historique.Count - 1; i >= start; i--)
            {
                var rec = _historique[i];
                Color col = rec.Gain > 0 ? C_VERT : (rec.Gain < 0 ? C_ROUGE : Color.Gray);
                string gainStr = (rec.Gain >= 0 ? "+" : "") + rec.Gain;
                string line = $"{rec.Resultat,-7} M:{rec.Mise,-4} {gainStr}";
                g.DrawString(line, F_HIST, new SolidBrush(col), 8, yPos);
                yPos += 18;
            }
            if (_historique.Count == 0)
                g.DrawString("Aucune partie.", F_UI_SM, new SolidBrush(Color.Gray), 8, 36);
        }
        #endregion

    #region Logique de jeu
        private void AjouterMise(int montant)
        {
            if (!_attenteMise || _enCours) return;
            if (_mise + montant > _jetons)
            { _sousMessage = "\u26a0 Pas assez de jetons !"; SonErreur(); Invalidate(); return; }
            _mise += montant;
            _sousMessage = "Mise : " + _mise + "  \u2502  Restant : " + (_jetons - _mise) + "  \u2502  Cliquez DEAL";
            SonClic(); Invalidate();
        }

        private void EffacerMise()
        {
            if (!_attenteMise || _enCours) return;
            _mise = 0; _sousMessage = "Mise effacée."; Invalidate();
        }

        private void Distribuer()
        {
            if (_mise <= 0)
            { _message = "Placez une mise d'abord !"; SonErreur(); Invalidate(); return; }

            _jetons -= _mise;
            _paquet = new BJPaquet(2); _paquet.Melanger();
            _mainJoueur.Clear(); _mainJoueur2.Clear(); _mainCroupier.Clear();
            _splitActif = false; _joueurSurMain2 = false; _miseMain2 = 0;
            _enCours = true; _attenteMise = false; _assurancePrise = false; _peutAssurance = false;
            _message = ""; _sousMessage = "";
            AfficherPhase(Phase.Jeu);
            _btnTirer.Visible = false;
            _btnRester.Visible = false;

            var c1 = _paquet.Tirer(true); var c2 = _paquet.Tirer(true);
            var c3 = _paquet.Tirer(true); var c4 = _paquet.Tirer(false);
            AjouterCarteAnimee(_mainJoueur, c1, false);
            AjouterCarteAnimee(_mainCroupier, c2, true);
            AjouterCarteAnimee(_mainJoueur, c3, false);
            AjouterCarteAnimee(_mainCroupier, c4, true);

            _peutDoubler = (_jetons >= _mise);
            _peutSplitter = (c1.ValeurBJ == c3.ValeurBJ) && (_jetons >= _mise);
            _peutAssurance = (c2.ValeurTexte == "A");

            var t = new Timer { Interval = 650 };
            t.Tick += (s, ev) => { ((Timer)s).Stop(); ((Timer)s).Dispose(); PostDistribution(); };
            t.Start();
        }

        private void PostDistribution()
        {
            if (_peutAssurance && !_assurancePrise)
            {
                _message = "\u26a0 Assurance ?";
                _sousMessage = "Le croupier a un As. Coût : " + (_mise / 2) + " jetons";
                AfficherPhase(Phase.Assurance);
                Invalidate(); return;
            }
            VerifierBlackjackInitial();
        }

        private void VerifierBlackjackInitial()
        {
            if (BJCalcul.EstBlackjack(_mainJoueur))
            {
                _mainCroupier[1].FaceVisible = true;
                SonBlackjack();
                if (BJCalcul.EstBlackjack(_mainCroupier))
                { Egalite("Egalité ! Double Blackjack !"); return; }
                int gain = (int)(_mise * 2.5);
                _jetons += gain; _blackjacks++; _parties++; _victoires++;
                EnregistrerPartie("BJ!", _mise, gain - _mise);
                _message = "\u2605 BLACKJACK ! +" + (gain - _mise) + " jetons";
                _sousMessage = "Jetons : " + _jetons;
                AfficherPhase(Phase.Fin);
                Invalidate(); return;
            }
            _message = "Tirez ou Restez ?";
            _sousMessage = "Votre score : " + BJCalcul.Score(_mainJoueur);
            AfficherPhase(Phase.Jeu);
            Invalidate();
        }

        private void PrendreAssurance()
        {
            int cout = _mise / 2;
            if (cout > _jetons) { _sousMessage = "Pas assez de jetons !"; Invalidate(); return; }
            _jetons -= cout; _miseAssurance = cout; _assurancePrise = true;
            SonClic();
            VerifierBlackjackInitial();
        }

        private void RefuserAssurance()
        {
            _assurancePrise = false; _miseAssurance = 0;
            VerifierBlackjackInitial();
        }

        private void Tirer()
        {
            if (!_enCours || _iaEnCours) return;
            var mc = _joueurSurMain2 ? _mainJoueur2 : _mainJoueur;
            AjouterCarteAnimee(mc, _paquet.Tirer(true), false);
            _peutDoubler = false; _peutSplitter = false;
            _btnDoubler.Visible = false; _btnSplit.Visible = false;
            SonCarte();
            int sc = BJCalcul.Score(mc);
            _sousMessage = "Votre score : " + sc;

            if (sc > 21)
            {
                SonBust();
                if (_splitActif && !_joueurSurMain2)
                {
                    _message = "Main 1 Bust ! Jouez la main 2...";
                    _joueurSurMain2 = true;
                    _peutDoubler = (_jetons >= _miseMain2);
                    _btnDoubler.Visible = _peutDoubler;
                }
                else
                {
                    _mainCroupier[1].FaceVisible = true;
                    FinRound("\u2639 Bust !", false);
                }
            }
            else if (sc == 21)
            { _sousMessage = "21 ! Parfait !"; Invalidate(); Rester(); }
            else
            { _message = "Tirez ou Restez ?"; Invalidate(); }
        }

        private void Rester()
        {
            if (!_enCours || _iaEnCours) return;
            if (_splitActif && !_joueurSurMain2)
            {
                _joueurSurMain2 = true;
                _peutDoubler = (_jetons >= _miseMain2);
                _btnDoubler.Visible = _peutDoubler;
                _btnSplit.Visible = false;
                _message = "Main 2 \u25ba Tirez ou Restez ?";
                _sousMessage = "Score M2 : " + BJCalcul.Score(_mainJoueur2);
                Invalidate(); return;
            }
            _mainCroupier[1].FaceVisible = true;
            _message = "Tour du croupier..."; _sousMessage = "";
            _btnTirer.Visible = false; _btnRester.Visible = false;
            _btnDoubler.Visible = false; _btnSplit.Visible = false;
            _iaEnCours = true; _timerIA.Start(); Invalidate();
        }

        private void Doubler()
        {
            if (!_enCours || _iaEnCours) return;
            var mc = _joueurSurMain2 ? _mainJoueur2 : _mainJoueur;
            int miseCourante = _joueurSurMain2 ? _miseMain2 : _mise;
            if (_jetons < miseCourante) { _sousMessage = "Pas assez de jetons !"; Invalidate(); return; }
            _jetons -= miseCourante;
            if (_joueurSurMain2) _miseMain2 *= 2; else _mise *= 2;
            AjouterCarteAnimee(mc, _paquet.Tirer(true), false);
            SonCarte(); _peutDoubler = false;
            _btnDoubler.Visible = false; _btnSplit.Visible = false;
            int sc = BJCalcul.Score(mc);
            _sousMessage = "Mise doublée ! Score : " + sc;
            Invalidate();
            if (sc > 21)
            {
                SonBust();
                if (_splitActif && !_joueurSurMain2)
                { _joueurSurMain2 = true; _btnDoubler.Visible = _jetons >= _miseMain2; }
                else
                { _mainCroupier[1].FaceVisible = true; FinRound("\u2639 Bust après double !", false); }
            }
            else Rester();
        }

        private void Split()
        {
            if (!_enCours || _iaEnCours || !_peutSplitter) return;
            if (_jetons < _mise) { _sousMessage = "Pas assez de jetons !"; Invalidate(); return; }
            _jetons -= _mise; _miseMain2 = _mise; _splitActif = true;
            var c2 = _mainJoueur[1]; _mainJoueur.RemoveAt(1); _mainJoueur2.Add(c2);
            AjouterCarteAnimee(_mainJoueur, _paquet.Tirer(true), false);
            AjouterCarteAnimee(_mainJoueur2, _paquet.Tirer(true), false);
            SonCarte();
            _peutSplitter = false;
            _peutDoubler = (_jetons >= _mise);
            _joueurSurMain2 = false;
            _btnSplit.Visible = false;
            _btnDoubler.Visible = _peutDoubler;
            _message = "SPLIT ! Main 1 active.";
            _sousMessage = "Score M1 : " + BJCalcul.Score(_mainJoueur);
            Invalidate();
        }

        private void TimerIA_Tick(object sender, EventArgs e)
        {
            int scIA = BJCalcul.ScoreComplet(_mainCroupier);
            if (scIA < 17)
            {
                AjouterCarteAnimee(_mainCroupier, _paquet.Tirer(true), true);
                SonCarte();
                scIA = BJCalcul.ScoreComplet(_mainCroupier);
                _sousMessage = "Croupier : " + scIA;
                Invalidate();
                if (scIA > 21)
                { _timerIA.Stop(); _iaEnCours = false; SonVictoire(); FinRound("\u2665 Croupier Bust ! Vous gagnez !", true); }
            }
            else
            { _timerIA.Stop(); _iaEnCours = false; Comparer(); }
        }

        private void Comparer()
        {
            if (_splitActif) { ComparerSplit(); return; }
            int scJ = BJCalcul.Score(_mainJoueur);
            int scIA = BJCalcul.ScoreComplet(_mainCroupier);
            if (scJ > 21) FinRound("\u2639 Bust !", false);
            else if (scJ > scIA) { SonVictoire(); FinRound($"\u265a Vous gagnez ! {scJ} vs {scIA}", true); }
            else if (scIA > scJ) FinRound($"\u2639 Croupier gagne. {scJ} vs {scIA}", false);
            else Egalite($"Égalité ! {scJ} vs {scIA}");
        }

        private void ComparerSplit()
        {
            int scIA = BJCalcul.ScoreComplet(_mainCroupier);
            int sc1 = BJCalcul.Score(_mainJoueur);
            int sc2 = BJCalcul.Score(_mainJoueur2);
            int gain = 0; string msg = "";

            if (sc1 <= 21 && sc1 > scIA) { gain += _mise * 2; msg += "M1:\u265a "; }
            else if (sc1 > 21 || scIA > sc1) { msg += "M1:\u2639 "; }
            else { gain += _mise; msg += "M1:= "; }

            if (sc2 <= 21 && sc2 > scIA) { gain += _miseMain2 * 2; msg += "M2:\u265a"; }
            else if (sc2 > 21 || scIA > sc2) { msg += "M2:\u2639"; }
            else { gain += _miseMain2; msg += "M2:="; }

            _jetons += gain;
            int netGain = gain - _mise - _miseMain2;
            _parties++; if (netGain > 0) _victoires++; else if (netGain < 0) _defaites++; else _egalites++;
            EnregistrerPartie("SPLIT", _mise + _miseMain2, netGain);
            _message = (netGain >= 0 ? "\u265a " : "\u2639 ") + msg;
            _sousMessage = $"Croupier:{scIA} | Jetons : {_jetons}";
            AfficherPhase(Phase.Fin); Invalidate();
        }

        private void FinRound(string msg, bool joueurGagne)
        {
            _enCours = false; _iaEnCours = false; _message = msg;
            int gain = joueurGagne ? _mise : -_mise;
            if (joueurGagne) _jetons += _mise * 2;
            if (_assurancePrise && BJCalcul.EstBlackjack(_mainCroupier))
            { _jetons += _miseAssurance * 3; gain += _miseAssurance * 2; }
            _sousMessage = "Jetons : " + _jetons;
            _parties++; if (joueurGagne) _victoires++; else _defaites++;
            EnregistrerPartie(joueurGagne ? "Gagne" : "Perdu", _mise, gain);
            if (_jetons <= 0)
            { _message = "\u26a0 GAME OVER !"; _sousMessage = "Cliquez Nouvelle Partie pour recommencer."; }
            AfficherPhase(Phase.Fin); Invalidate();
        }

        private void Egalite(string msg)
        {
            _enCours = false; _iaEnCours = false; _message = msg;
            _jetons += _mise; _sousMessage = "Remboursement. Jetons : " + _jetons;
            _parties++; _egalites++;
            EnregistrerPartie("Egalite", _mise, 0);
            AfficherPhase(Phase.Fin); Invalidate();
        }

        private void NouvellePartie()
        {
            _mise = 0; _enCours = false; _iaEnCours = false; _attenteMise = true;
            _splitActif = false; _joueurSurMain2 = false;
            _mainJoueur.Clear(); _mainJoueur2.Clear(); _mainCroupier.Clear();
            _message = "Placez votre mise !";
            _sousMessage = "Jetons : " + _jetons;
            if (_jetons <= 0)
            { _jetons = 1000; _message = "Nouveau départ ! 1000 jetons."; _sousMessage = ""; }
            AfficherPhase(Phase.Mise); Invalidate();
        }

        private void ToggleHistorique()
        {
            _showHistorique = !_showHistorique;
            _pnlHistorique.Visible = _showHistorique;
            _pnlHistorique.Invalidate();
        }

        private void EnregistrerPartie(string res, int mise, int gain)
        {
            _historique.Add(new PartieRecord { Resultat = res, Mise = mise, Gain = gain });
            if (_historique.Count > 50) _historique.RemoveAt(0);
            if (_pnlHistorique.Visible) _pnlHistorique.Invalidate();
        }

        private void BtnAccueil_Click(object sender, EventArgs e)
        {
            _timerIA.Stop(); _timerAnim.Stop();
            try
            {
                var accueil = new Accueil();
                accueil.Show();
                this.Close();
            }
            catch
            {
                MessageBox.Show("Impossible d'ouvrir Accueil.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Sons
        private void SonClic() { try { Beep(880, 55); } catch { } }
        private void SonCarte() { try { Beep(660, 75); } catch { } }
        private void SonVictoire() { try { Beep(880, 90); Beep(1100, 140); } catch { } }
        private void SonBlackjack() { try { Beep(880, 70); Beep(1100, 70); Beep(1320, 190); } catch { } }
        private void SonBust() { try { Beep(300, 190); Beep(220, 280); } catch { } }
        private void SonErreur() { try { Beep(200, 140); } catch { } }

        // ── Helpers graphiques ────────────────────────────────────
        private static GraphicsPath CreateRoundedPath(float x, float y, float w, float h, float r)
        {
            var p = new GraphicsPath();
            p.AddArc(x, y, r * 2, r * 2, 180, 90);
            p.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            p.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            p.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static void FillRoundedRect(Graphics g, Brush br, float x, float y, float w, float h, float r)
        {
            using (var path = CreateRoundedPath(x, y, w, h, r)) g.FillPath(br, path);
        }

        private static void DrawRoundedRect(Graphics g, Pen pen, float x, float y, float w, float h, float r)
        {
            using (var path = CreateRoundedPath(x, y, w, h, r)) g.DrawPath(pen, path);
        }

        private void MettreAJourStats() { }
    }
    #endregion
}