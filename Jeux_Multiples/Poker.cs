using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    #region Modeles
    public enum THCouleur { Trefle, Carreau, Coeur, Pique }
    public enum THValeur { Deux = 2, Trois, Quatre, Cinq, Six, Sept, Huit, Neuf, Dix, Valet, Dame, Roi, As }
    public enum THMainType { CartesHautes, Paire, DoublePaire, Brelan, Suite, Couleur, Full, Carre, QuinteFlush }
    public enum THPhase { Attente, PreFlop, Flop, Turn, River, Showdown }
    public enum IAAction { Check, Call, Raise, Fold }

    public class THCarte
    {
        public THCouleur Couleur { get; private set; }
        public THValeur Valeur { get; private set; }
        public bool FaceVisible { get; set; }
        public THCarte(THCouleur c, THValeur v) { Couleur = c; Valeur = v; }

        public string Symbole
        {
            get
            {
                if (Couleur == THCouleur.Trefle) return "\u2663";
                if (Couleur == THCouleur.Carreau) return "\u2666";
                if (Couleur == THCouleur.Coeur) return "\u2665";
                return "\u2660";
            }
        }
        public string ValeurTexte
        {
            get
            {
                if (Valeur == THValeur.As) return "A";
                if (Valeur == THValeur.Roi) return "K";
                if (Valeur == THValeur.Dame) return "Q";
                if (Valeur == THValeur.Valet) return "J";
                if (Valeur == THValeur.Dix) return "10";
                return ((int)Valeur).ToString();
            }
        }
        public bool EstRouge => Couleur == THCouleur.Coeur || Couleur == THCouleur.Carreau;
    }

    public class THPaquet
    {
        private readonly List<THCarte> _cartes = new List<THCarte>();
        private readonly Random _rng = new Random();
        public THPaquet()
        {
            foreach (THCouleur c in Enum.GetValues(typeof(THCouleur)))
                foreach (THValeur v in Enum.GetValues(typeof(THValeur)))
                    _cartes.Add(new THCarte(c, v));
        }
        public void Melanger()
        {
            for (int i = _cartes.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1); var t = _cartes[i]; _cartes[i] = _cartes[j]; _cartes[j] = t;
            }
        }
        public THCarte Tirer(bool face = true) { var c = _cartes[0]; _cartes.RemoveAt(0); c.FaceVisible = face; return c; }
    }

    public class THEvaluation { public THMainType Type { get; set; } public int Score { get; set; } public string Description { get; set; } }

    public static class THEvaluateur
    {
        public static THEvaluation MeilleureDe(List<THCarte> cartes)
        {
            if (cartes.Count < 5) return Evaluer(cartes);
            THEvaluation best = null;
            Combine(cartes, 5, 0, new List<THCarte>(), combo => {
                var e = Evaluer(combo);
                if (best == null || e.Score > best.Score) best = e;
            });
            return best ?? Evaluer(cartes.GetRange(0, 5));
        }
        private static void Combine(List<THCarte> src, int k, int start, List<THCarte> curr, Action<List<THCarte>> fn)
        {
            if (curr.Count == k) { fn(new List<THCarte>(curr)); return; }
            for (int i = start; i < src.Count; i++) { curr.Add(src[i]); Combine(src, k, i + 1, curr, fn); curr.RemoveAt(curr.Count - 1); }
        }
        public static THEvaluation Evaluer(List<THCarte> cartes)
        {
            var vals = new List<int>(); foreach (var c in cartes) vals.Add((int)c.Valeur);
            vals.Sort((a, b) => b.CompareTo(a));
            var cpt = new Dictionary<int, int>();
            foreach (int v in vals) { if (!cpt.ContainsKey(v)) cpt[v] = 0; cpt[v]++; }
            var grp = new List<KeyValuePair<int, int>>(cpt);
            grp.Sort((a, b) => b.Value != a.Value ? b.Value.CompareTo(a.Value) : b.Key.CompareTo(a.Key));
            bool flush = cartes.Count == 5 && TousMemes(cartes);
            bool suite = grp.Count == 5 && (vals[0] - vals[4] == 4);
            bool petite = vals.Count >= 5 && vals[0] == 14 && vals[1] == 5 && vals[2] == 4 && vals[3] == 3 && vals[4] == 2;
            int sc = 0; foreach (int v in vals) sc = sc * 15 + v;
            if ((suite || petite) && flush) return Mk(THMainType.QuinteFlush, 8000000 + (petite ? 5 : vals[0]), "Quinte Flush !");
            if (grp[0].Value == 4) return Mk(THMainType.Carre, 7000000 + grp[0].Key * 100 + grp[1].Key, "Carré !");
            if (grp[0].Value == 3 && grp.Count > 1 && grp[1].Value == 2)
                return Mk(THMainType.Full, 6000000 + grp[0].Key * 100 + grp[1].Key, "Full House !");
            if (flush) return Mk(THMainType.Couleur, 5000000 + sc, "Couleur !");
            if (suite || petite) return Mk(THMainType.Suite, 4000000 + (petite ? 5 : vals[0]), "Suite !");
            if (grp[0].Value == 3) return Mk(THMainType.Brelan, 3000000 + grp[0].Key * 100, "Brelan !");
            if (grp[0].Value == 2 && grp.Count > 1 && grp[1].Value == 2)
                return Mk(THMainType.DoublePaire, 2000000 + grp[0].Key * 100 + grp[1].Key, "Double Paire !");
            if (grp[0].Value == 2) return Mk(THMainType.Paire, 1000000 + grp[0].Key * 100 + sc, "Paire de " + Nom(grp[0].Key));
            return Mk(THMainType.CartesHautes, sc, "Carte haute : " + Nom(vals[0]));
        }
        private static THEvaluation Mk(THMainType t, int s, string d) => new THEvaluation { Type = t, Score = s, Description = d };
        private static bool TousMemes(List<THCarte> c) { for (int i = 1; i < c.Count; i++) if (c[i].Couleur != c[0].Couleur) return false; return true; }
        private static string Nom(int v)
        {
            if (v == 14) return "As"; if (v == 13) return "Roi"; if (v == 12) return "Dame"; if (v == 11) return "Valet";
            if (v == 10) return "10"; return v.ToString();
        }
    }

    public class THRecord { public string Phase { get; set; } public int Mise { get; set; } public int Gain { get; set; } }
    #endregion

    #region Formulaire
    public partial class Poker : Form
    {
        // ── Palette ───────────────────────────────────────────────────
        static readonly Color C_FOND = Color.FromArgb(4, 18, 4);
        static readonly Color C_TAPIS = Color.FromArgb(15, 68, 26);
        static readonly Color C_TAPIS2 = Color.FromArgb(7, 44, 14);
        static readonly Color C_OR = Color.FromArgb(255, 215, 65);
        static readonly Color C_OR2 = Color.FromArgb(192, 148, 18);
        static readonly Color C_TEXTE = Color.FromArgb(245, 240, 220);
        static readonly Color C_ROUGE = Color.FromArgb(218, 46, 46);
        static readonly Color C_VERT = Color.FromArgb(46, 206, 76);
        static readonly Color C_BLEU = Color.FromArgb(55, 135, 225);
        static readonly Color C_ORANGE = Color.FromArgb(230, 145, 26);

        // ── Polices ───────────────────────────────────────────────────
        static readonly Font F_TITRE = new Font("Georgia", 20, FontStyle.Bold | FontStyle.Italic);
        static readonly Font F_UI = new Font("Segoe UI", 11, FontStyle.Bold);
        static readonly Font F_UI_SM = new Font("Segoe UI", 10);
        static readonly Font F_MSG = new Font("Georgia", 17, FontStyle.Bold | FontStyle.Italic);
        static readonly Font F_VAL = new Font("Georgia", 13, FontStyle.Bold);
        static readonly Font F_SYM_SM = new Font("Segoe UI Symbol", 9);
        static readonly Font F_SYM_LG = new Font("Segoe UI Symbol", 15, FontStyle.Bold);
        static readonly Font F_BTN = new Font("Georgia", 11, FontStyle.Bold);
        static readonly Font F_HIST = new Font("Consolas", 9);
        static readonly Font F_PHASE = new Font("Segoe UI", 8, FontStyle.Bold);
        static readonly Font F_CHIP = new Font("Segoe UI", 8, FontStyle.Bold);

        // ── Dimensions cartes ─────────────────────────────────────────
        const int CW = 66;
        const int CH = 98;
        const int CGAP = 10;

        // ── Layout panneau bas ─────────────────────────────────────────
        // BAS_H = 140px total
        // Ligne 1 (y=10) : info pot (texte)
        // Ligne 2 (y=32) : boutons action h=68 → bottom y=100
        // Ligne 3 (y=108): chips relance h=26  → bottom y=134
        // Séparateur vertical entre actions et relance
        //
        // 4 boutons action : chacun 140px, gap=10  → total = 4×140+3×10 = 590px
        // Départ x=20 → fin x=610
        // Séparateur à x=626
        // Zone relance : x=642 à x=642+3×60+2×8=242 → x=884 < 1060 ✓
        const int BAS_H = 140;
        const int BTN_H = 68;
        const int BTN_W = 140;
        const int BTN_GAP = 10;
        const int BTN_Y = 32;    // y des boutons dans le panneau bas
        const int SEP_X = 620;   // x du séparateur vertical
        const int REL_X = 640;   // x zone relance
        const int CHIP_S = 52;    // taille des chips
        const int CHIP_G = 8;     // gap entre chips
        const int HIST_W = 210;

        // ── État ──────────────────────────────────────────────────────
        private THPaquet _paquet;
        private List<THCarte> _holeJoueur = new List<THCarte>();
        private List<THCarte> _holeIA = new List<THCarte>();
        private List<THCarte> _community = new List<THCarte>();
        private THPhase _phase = THPhase.Attente;

        private int _jetons = 1000;
        private int _jetonsIA = 1000;
        private int _pot = 0;
        private int _smallBlind = 10;
        private int _bigBlind = 20;
        private int _miseActuelle = 0;
        private int _miseJoueur = 0;
        private int _miseIA = 0;
        private int _relance = 20;

        private bool _joueurDoit = false;
        private bool _iaEnCours = false;
        private string _message = "";
        private string _sousMsg = "";
        private string _msgIA = "";
        private string _handJoueur = "";
        private string _handIA = "";

        private readonly List<THRecord> _historique = new List<THRecord>();
        private bool _showHistorique = false;
        private int _parties = 0, _victoires = 0, _defaites = 0, _egalites = 0;

        private Timer _timerIA;

        [DllImport("kernel32.dll")] static extern bool Beep(int f, int d);

        // ── Contrôles ─────────────────────────────────────────────────
        // Persistants (toujours visibles hors du panneau bas)
        private Button _btnAccueil, _btnHistorique;
        private Panel _pnlHistorique;

        // Panneau bas commun
        private Panel _pnlBas;

        // Sous-panneau A : phase Attente / Showdown  → bouton DEAL centré
        private Panel _pnlZoneAttente;
        private Button _btnDeal;

        // Sous-panneau B : phase de jeu → 4 actions + zone relance
        private Panel _pnlZoneJeu;
        private Button _btnFold, _btnCheck, _btnCall, _btnRaise;
        private Button _btnChip10, _btnChip25, _btnChip50, _btnChipReset;
        private Label _lblRelance;   // affiche la valeur courante de _relance

        // ════════════════════════════════════════════════════════
        public string Joueur = "Joueur";

        // ════════════════════════════════════════════════════════
        public Poker(string joueur)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(joueur)) Joueur = joueur;
            Text = $"\u2660 Texas Hold'em Poker ({Joueur}) \u2665";
            Size = new Size(1060, 760);
            MinimumSize = new Size(1060, 760);
            BackColor = C_FOND;
            DoubleBuffered = true;
            BuildUI();
            _timerIA = new Timer { Interval = 900 };
            _timerIA.Tick += TimerIA_Tick;
            _message = "Bienvenue au Texas Hold'em !";
            _sousMsg = "Cliquez DEAL pour commencer.";
            SyncPhase();
        }

        public Poker() : this("Joueur") { }

        private void Poker_Load(object sender, EventArgs e) { }
        #endregion

        // ════════════════════════════════════════════════════════
    #region Construction UI
        private void BuildUI()
        {
            // ─────────────────────────────────────────────────────
            // 1. BOUTONS PERSISTANTS (hors panneau bas)
            // ─────────────────────────────────────────────────────
            _btnAccueil = BtnPersistant("\u2302  Accueil", 10, 10, 118, 34,
                Color.FromArgb(48, 78, 48), Color.FromArgb(200, 230, 180));
            _btnAccueil.Click += BtnAccueil_Click;
            Controls.Add(_btnAccueil);

            _btnHistorique = BtnPersistant("\u2630  Historique", Width - 152, 10, 140, 34,
                Color.FromArgb(26, 55, 26), Color.FromArgb(175, 210, 145));
            _btnHistorique.Click += (_, __) => ToggleHistorique();
            Controls.Add(_btnHistorique);

            _pnlHistorique = new Panel
            {
                Bounds = new Rectangle(Width - HIST_W - 10, 52, HIST_W, Height - 220),
                BackColor = Color.FromArgb(10, 28, 10),
                Visible = false
            };
            _pnlHistorique.Paint += PaintHistorique;
            Controls.Add(_pnlHistorique);

            // ─────────────────────────────────────────────────────
            // 2. PANNEAU BAS (fond commun)
            // ─────────────────────────────────────────────────────
            _pnlBas = new Panel
            {
                Bounds = new Rectangle(0, Height - BAS_H, Width, BAS_H),
                BackColor = Color.FromArgb(8, 22, 8)
            };
            _pnlBas.Paint += PaintPanneauBas;
            Controls.Add(_pnlBas);

            // ─────────────────────────────────────────────────────
            // 3. SOUS-PANNEAU A : ATTENTE / SHOWDOWN
            //    Contenu : bouton DEAL centré horizontalement
            // ─────────────────────────────────────────────────────
            _pnlZoneAttente = new Panel
            {
                Bounds = new Rectangle(0, 0, Width, BAS_H),
                BackColor = Color.Transparent
            };
            _pnlBas.Controls.Add(_pnlZoneAttente);

            _btnDeal = new Button
            {
                Text = "◎  DEAL",
                Size = new Size(200, 76),
                // X sera recalculé dans RecalcLayout, Y centré verticalement
                Location = new Point((Width - 200) / 2, (BAS_H - 76) / 2),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(18, 12, 0),
                Font = new Font("Georgia", 15, FontStyle.Bold),
                BackColor = Color.FromArgb(210, 168, 18),
                Cursor = Cursors.Hand
            };
            _btnDeal.FlatAppearance.BorderSize = 0;
            _btnDeal.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 198, 50);
            _btnDeal.FlatAppearance.MouseDownBackColor = Color.FromArgb(178, 138, 8);
            AppliquerRegionArrondie(_btnDeal, 200, 76, 13);
            _btnDeal.Click += (_, __) => Distribuer();
            _pnlZoneAttente.Controls.Add(_btnDeal);

            // ─────────────────────────────────────────────────────
            // 4. SOUS-PANNEAU B : JEU
            //    Layout (dans BAS_H=140px) :
            //
            //    y=0..9   : bande info (peinte, pas de contrôle)
            //    y=BTN_Y  : 4 boutons action (h=BTN_H=68)
            //    y=BTN_Y+BTN_H+4 = 104 : chips relance (h=CHIP_S=52→32) → on garde chips à y=BTN_Y
            //
            //    x des 4 boutons (tous BTN_W=140, gap BTN_GAP=10) :
            //       COUCHER : x=20         → droite=160
            //       CHECK   : x=170        → droite=310
            //       SUIVRE  : x=320        → droite=460
            //       RELANCER: x=470        → droite=610
            //
            //    Séparateur vertical : x=SEP_X=620, y=BTN_Y..BTN_Y+BTN_H
            //
            //    Zone relance démarre à x=REL_X=640
            //       Label "Relance:" y=BTN_Y
            //       Valeur relance  y=BTN_Y+18
            //       Chips +10 +25 +50 ↺ : y=BTN_Y+38, taille 36×36
            //          x=640, 684, 728, 772  → droite=808 < 1060 ✓
            // ─────────────────────────────────────────────────────
            _pnlZoneJeu = new Panel
            {
                Bounds = new Rectangle(0, 0, Width, BAS_H),
                BackColor = Color.Transparent,
                Visible = false
            };
            _pnlBas.Controls.Add(_pnlZoneJeu);

            // ── 4 boutons action ──────────────────────────────────
            int bx = 20;

            _btnFold = BtnAction("\u2716 COUCHER", bx, BTN_Y, BTN_W, BTN_H, Color.FromArgb(178, 34, 34), Color.White);
            bx += BTN_W + BTN_GAP;
            _btnCheck = BtnAction("\u2714 CHECK", bx, BTN_Y, BTN_W, BTN_H, Color.FromArgb(32, 152, 58), Color.Black);
            bx += BTN_W + BTN_GAP;
            _btnCall = BtnAction("\u25cf SUIVRE", bx, BTN_Y, BTN_W, BTN_H, Color.FromArgb(38, 112, 208), Color.White);
            bx += BTN_W + BTN_GAP;
            _btnRaise = BtnAction("\u25b2 RELANCER", bx, BTN_Y, BTN_W, BTN_H, Color.FromArgb(190, 130, 14), Color.Black);
            // bx après : 470+140 = 610  → SEP_X=620 ✓ pas de chevauchement

            _btnFold.Click += (_, __) => ActionFold();
            _btnCheck.Click += (_, __) => ActionCheck();
            _btnCall.Click += (_, __) => ActionCall();
            _btnRaise.Click += (_, __) => ActionRaise();

            foreach (var b in new[] { _btnFold, _btnCheck, _btnCall, _btnRaise })
                _pnlZoneJeu.Controls.Add(b);

            // ── Séparateur visuel ─────────────────────────────────
            var sep = new Panel
            {
                Bounds = new Rectangle(SEP_X, BTN_Y + 4, 1, BTN_H - 8),
                BackColor = Color.FromArgb(55, 255, 255, 255)
            };
            _pnlZoneJeu.Controls.Add(sep);

            // ── Zone relance ──────────────────────────────────────
            //    Titre
            var lblTitre = new Label
            {
                Text = "Relance",
                ForeColor = Color.FromArgb(170, C_OR),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(REL_X, BTN_Y + 2),
                AutoSize = true
            };
            _pnlZoneJeu.Controls.Add(lblTitre);

            //    Valeur courante (mise en valeur)
            _lblRelance = new Label
            {
                Text = "20",
                ForeColor = C_OR,
                Font = new Font("Georgia", 16, FontStyle.Bold),
                Location = new Point(REL_X, BTN_Y + 18),
                Size = new Size(68, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _pnlZoneJeu.Controls.Add(_lblRelance);

            //    Chips : taille 36×36, gap=8, départ x=REL_X+72
            int chipX = REL_X + 72;
            int chipY = BTN_Y + 12;          // centré verticalement dans BTN_H
            int chipSz = 44;

            _btnChip10 = MkChip("+10", chipX, chipY, chipSz, Color.FromArgb(178, 40, 40));
            _btnChip25 = MkChip("+25", chipX + 52, chipY, chipSz, Color.FromArgb(40, 65, 178));
            _btnChip50 = MkChip("+50", chipX + 104, chipY, chipSz, Color.FromArgb(145, 92, 8));
            _btnChipReset = MkChip("\u21ba", chipX + 156, chipY, chipSz, Color.FromArgb(55, 55, 55));
            // droite = REL_X+72+156+44 = 640+72+200 = 912 < 1060 ✓

            _btnChip10.Click += (_, __) => ModifierRelance(+10);
            _btnChip25.Click += (_, __) => ModifierRelance(+25);
            _btnChip50.Click += (_, __) => ModifierRelance(+50);
            _btnChipReset.Click += (_, __) => ModifierRelance(0, reset: true);

            foreach (var b in new[] { _btnChip10, _btnChip25, _btnChip50, _btnChipReset })
                _pnlZoneJeu.Controls.Add(b);

            Resize += (_, __) => RecalcLayout();
            RecalcLayout();
        }

        private void RecalcLayout()
        {
            _pnlBas.Width = Width;
            _pnlBas.Top = Height - BAS_H;
            _pnlZoneAttente.Width = Width;
            _pnlZoneJeu.Width = Width;
            _btnHistorique.Left = Width - 152;
            _pnlHistorique.Left = Width - HIST_W - 10;
            _pnlHistorique.Height = Height - 220;
            // Recentrer le DEAL
            _btnDeal.Location = new Point((Width - 200) / 2, (BAS_H - 76) / 2);
            Invalidate();
            _pnlHistorique.Invalidate();
        }

        // ── Helpers création boutons ──────────────────────────────
        private Button BtnPersistant(string txt, int x, int y, int w, int h, Color bg, Color fg)
        {
            var b = new Button
            {
                Text = txt,
                Bounds = new Rectangle(x, y, w, h),
                FlatStyle = FlatStyle.Flat,
                ForeColor = fg,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = bg,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.3f);
            AppliquerRegionArrondie(b, w, h, 8);
            return b;
        }

        private Button BtnAction(string txt, int x, int y, int w, int h, Color bg, Color fg)
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
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.28f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bg, 0.14f);
            AppliquerRegionArrondie(b, w, h, 10);
            return b;
        }

        private Button MkChip(string txt, int x, int y, int sz, Color bg)
        {
            var b = new Button
            {
                Text = txt,
                Location = new Point(x, y),
                Size = new Size(sz, sz),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = F_CHIP,
                BackColor = bg,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = ControlPaint.Light(bg, 0.65f);
            b.FlatAppearance.BorderSize = 2;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.38f);
            b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bg, 0.2f);
            b.Region = new Region(EllipsePath(b.ClientRectangle));
            return b;
        }

        private void AppliquerRegionArrondie(Button b, int w, int h, int r)
        {
            var p = new GraphicsPath();
            p.AddArc(0, 0, r * 2, r * 2, 180, 90); p.AddArc(w - r * 2, 0, r * 2, r * 2, 270, 90);
            p.AddArc(w - r * 2, h - r * 2, r * 2, r * 2, 0, 90); p.AddArc(0, h - r * 2, r * 2, r * 2, 90, 90);
            p.CloseFigure(); b.Region = new Region(p);
        }
        private GraphicsPath EllipsePath(Rectangle r) { var p = new GraphicsPath(); p.AddEllipse(r); return p; }

        // ── Synchronisation UI selon la phase ────────────────────
        private void SyncPhase()
        {
            bool attente = (_phase == THPhase.Attente || _phase == THPhase.Showdown);
            bool enJeu = !attente && _joueurDoit;

            _pnlZoneAttente.Visible = attente;
            _pnlZoneJeu.Visible = enJeu;

            if (!enJeu) return;

            bool peutCheck = (_miseActuelle <= _miseJoueur);
            _btnCheck.Visible = peutCheck;
            _btnCall.Visible = !peutCheck;

            if (_btnCall.Visible)
                _btnCall.Text = "\u25cf SUIVRE\n(" + (_miseActuelle - _miseJoueur) + ")";
            else
                _btnCheck.Text = "\u2714 CHECK";

            _btnRaise.Text = "\u25b2 RELANCER\n(+" + _relance + ")";
            _lblRelance.Text = _relance.ToString();
        }
        #endregion

        // ════════════════════════════════════════════════════════
    #region Logique
        private void ModifierRelance(int delta, bool reset = false)
        {
            if (_phase == THPhase.Attente || _phase == THPhase.Showdown) return;
            _relance = reset ? _bigBlind : Math.Max(_bigBlind, Math.Min(_relance + delta, _jetons));
            _lblRelance.Text = _relance.ToString();
            _btnRaise.Text = "\u25b2 RELANCER\n(+" + _relance + ")";
            try { Beep(880, 45); } catch { }
        }

        private void Distribuer()
        {
            if (_jetons < _bigBlind || _jetonsIA < _bigBlind) { _message = "Pas assez de jetons !"; Invalidate(); return; }
            _paquet = new THPaquet(); _paquet.Melanger();
            _holeJoueur.Clear(); _holeIA.Clear(); _community.Clear();
            _pot = 0; _miseJoueur = 0; _miseIA = 0; _miseActuelle = 0; _relance = _bigBlind;
            _message = ""; _sousMsg = ""; _msgIA = ""; _handJoueur = ""; _handIA = "";

            PrendreBlinde(ref _jetons, ref _miseJoueur, _smallBlind);
            PrendreBlinde(ref _jetonsIA, ref _miseIA, _bigBlind);
            _pot = _smallBlind + _bigBlind; _miseActuelle = _bigBlind;

            _holeJoueur.Add(_paquet.Tirer(true)); _holeJoueur.Add(_paquet.Tirer(true));
            _holeIA.Add(_paquet.Tirer(false)); _holeIA.Add(_paquet.Tirer(false));

            _phase = THPhase.PreFlop; _joueurDoit = true;
            _message = "PRE-FLOP  |  SB:" + _smallBlind + "  BB:" + _bigBlind;
            _sousMsg = "Pot : " + _pot + "  ·  À vous de jouer !";
            _handJoueur = EvalMain(_holeJoueur, _community);
            try { Beep(660, 80); Beep(880, 80); } catch { }
            SyncPhase(); Invalidate();
        }

        private void PrendreBlinde(ref int j, ref int m, int v) { int n = Math.Min(v, j); j -= n; m += n; }

        private void ActionCheck()
        {
            if (!_joueurDoit) return;
            _joueurDoit = false; _msgIA = "\u25a0 Vous checkez.";
            SyncPhase(); LancerIA();
        }
        private void ActionCall()
        {
            if (!_joueurDoit) return;
            int n = Math.Min(_miseActuelle - _miseJoueur, _jetons);
            _jetons -= n; _miseJoueur += n; _pot += n;
            _joueurDoit = false; _msgIA = "\u25a0 Vous suivez (+" + n + ").";
            try { Beep(660, 70); } catch { }
            SyncPhase(); LancerIA();
        }
        private void ActionRaise()
        {
            if (!_joueurDoit) return;
            int total = Math.Min(_miseActuelle - _miseJoueur + _relance, _jetons);
            _jetons -= total; _miseJoueur += total; _pot += total; _miseActuelle = _miseJoueur;
            _joueurDoit = false; _msgIA = "\u25a0 Vous relancez de " + _relance + "."; _relance = _bigBlind;
            try { Beep(880, 80); Beep(1100, 80); } catch { }
            SyncPhase(); LancerIA();
        }
        private void ActionFold()
        {
            if (!_joueurDoit) return;
            _joueurDoit = false; _jetonsIA += _pot; _pot = 0;
            _message = "\u2639 Vous vous couchez. L'IA remporte le pot !";
            _sousMsg = "Jetons : " + _jetons;
            _phase = THPhase.Showdown; _defaites++; _parties++;
            EnregistrerPartie("Couché", _miseJoueur, -_miseJoueur);
            try { Beep(300, 200); Beep(220, 300); } catch { }
            SyncPhase(); Invalidate();
        }

        private void LancerIA() { _iaEnCours = true; _timerIA.Start(); }
        private void TimerIA_Tick(object sender, EventArgs e) { _timerIA.Stop(); _iaEnCours = false; IAJouer(); }

        private void IAJouer()
        {
            var allIA = new List<THCarte>(_holeIA); allIA.AddRange(_community);
            foreach (var c in _holeIA) c.FaceVisible = true;
            var eval = THEvaluateur.MeilleureDe(allIA);
            foreach (var c in _holeIA) c.FaceVisible = false;

            var rng = new Random();
            bool doit = _miseActuelle > _miseIA;
            IAAction action;

            if (eval.Type >= THMainType.Brelan)
                action = rng.Next(100) < 80 ? IAAction.Raise : IAAction.Call;
            else if (eval.Type == THMainType.DoublePaire)
                action = doit ? (rng.Next(100) < 70 ? IAAction.Call : IAAction.Fold) : IAAction.Check;
            else if (eval.Type == THMainType.Paire)
                action = doit ? (rng.Next(100) < 55 ? IAAction.Call : IAAction.Fold) : (rng.Next(100) < 30 ? IAAction.Raise : IAAction.Check);
            else
                action = doit ? (rng.Next(100) < 35 ? IAAction.Call : IAAction.Fold) : IAAction.Check;

            if (_jetonsIA <= 0) action = IAAction.Check;

            switch (action)
            {
                case IAAction.Fold:
                    _jetons += _pot; _pot = 0;
                    _message = "\u265a L'IA se couche ! Vous remportez le pot !";
                    _sousMsg = "Jetons : " + _jetons; _phase = THPhase.Showdown;
                    _victoires++; _parties++;
                    EnregistrerPartie("IA couchée", _miseJoueur, _pot);
                    try { Beep(880, 90); Beep(1100, 140); } catch { }
                    SyncPhase(); Invalidate(); return;

                case IAAction.Call:
                    int cm = Math.Min(_miseActuelle - _miseIA, _jetonsIA);
                    _jetonsIA -= cm; _miseIA += cm; _pot += cm;
                    _msgIA = "\u25a0 L'IA suit (+" + cm + ").";
                    try { Beep(660, 70); } catch { }
                    break;

                case IAAction.Raise:
                    int rm = Math.Min(_miseActuelle - _miseIA + _relance, _jetonsIA);
                    _jetonsIA -= rm; _miseIA += rm; _pot += rm; _miseActuelle = _miseIA;
                    _msgIA = "\u25a0 L'IA relance de " + _relance + " !";
                    try { Beep(880, 80); } catch { }
                    _joueurDoit = true; SyncPhase(); Invalidate(); return;

                default:
                    _msgIA = "\u25a0 L'IA checke.";
                    break;
            }
            PasserPhase();
        }

        private void PasserPhase()
        {
            _miseJoueur = 0; _miseIA = 0; _miseActuelle = 0; _relance = _bigBlind;
            switch (_phase)
            {
                case THPhase.PreFlop:
                    _community.Add(_paquet.Tirer()); _community.Add(_paquet.Tirer()); _community.Add(_paquet.Tirer());
                    _phase = THPhase.Flop; _message = "FLOP";
                    try { Beep(660, 80); Beep(880, 100); } catch { }
                    break;
                case THPhase.Flop:
                    _community.Add(_paquet.Tirer()); _phase = THPhase.Turn; _message = "TURN";
                    try { Beep(660, 80); Beep(1000, 100); } catch { }
                    break;
                case THPhase.Turn:
                    _community.Add(_paquet.Tirer()); _phase = THPhase.River; _message = "RIVER";
                    try { Beep(660, 80); Beep(1100, 100); } catch { }
                    break;
                case THPhase.River:
                    RevelerIA(); return;
            }
            _handJoueur = EvalMain(_holeJoueur, _community);
            _sousMsg = "Pot : " + _pot + "  ·  À vous !";
            _joueurDoit = true; SyncPhase(); Invalidate();
        }

        private void RevelerIA()
        {
            foreach (var c in _holeIA) c.FaceVisible = true;
            _phase = THPhase.Showdown;
            var allJ = new List<THCarte>(_holeJoueur); allJ.AddRange(_community);
            var allI = new List<THCarte>(_holeIA); allI.AddRange(_community);
            var eJ = THEvaluateur.MeilleureDe(allJ);
            var eI = THEvaluateur.MeilleureDe(allI);
            _handJoueur = eJ.Description; _handIA = eI.Description;

            if (eJ.Score > eI.Score)
            {
                _jetons += _pot; _pot = 0;
                _message = "\u265a VOUS GAGNEZ !  " + eJ.Description; _sousMsg = "Jetons : " + _jetons;
                _victoires++; _parties++; EnregistrerPartie("Gagné", _miseJoueur, _pot);
                try { Beep(880, 90); Beep(1100, 140); Beep(1320, 200); } catch { }
            }
            else if (eI.Score > eJ.Score)
            {
                _jetonsIA += _pot; _pot = 0;
                _message = "\u2639 L'IA gagne !  " + eI.Description; _sousMsg = "Jetons : " + _jetons;
                _defaites++; _parties++; EnregistrerPartie("Perdu", _miseJoueur, -_miseJoueur);
                try { Beep(300, 190); Beep(220, 280); } catch { }
            }
            else
            {
                int s = _pot / 2; _jetons += s; _jetonsIA += s; _pot = 0;
                _message = "\u2764 Égalité !  Pot partagé."; _sousMsg = "Jetons : " + _jetons;
                _egalites++; _parties++; EnregistrerPartie("Égalité", _miseJoueur, 0);
            }
            if (_jetons <= 0) { _message = "\u26a0 GAME OVER !"; _jetons = 1000; _sousMsg = "Nouveau départ : 1000 j."; }
            if (_jetonsIA <= 0) { _message = "\u2665 L'IA est ruinée !"; _jetonsIA = 1000; }
            SyncPhase(); Invalidate();
        }

        private string EvalMain(List<THCarte> h, List<THCarte> c)
        {
            var all = new List<THCarte>(h); all.AddRange(c);
            if (all.Count < 2) return "";
            return THEvaluateur.MeilleureDe(all).Description;
        }
        private void EnregistrerPartie(string r, int m, int g)
        {
            _historique.Add(new THRecord { Phase = r, Mise = m, Gain = g });
            if (_historique.Count > 50) _historique.RemoveAt(0);
            if (_pnlHistorique.Visible) _pnlHistorique.Invalidate();
        }
        private void ToggleHistorique() { _showHistorique = !_showHistorique; _pnlHistorique.Visible = _showHistorique; _pnlHistorique.Invalidate(); }
        private void BtnAccueil_Click(object s, EventArgs e)
        {
            _timerIA.Stop();
            try { var a = new Accueil(); a.Show(); this.Close(); }
            catch { MessageBox.Show("Impossible d'ouvrir Accueil.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        #endregion

        // ════════════════════════════════════════════════════════
    #region Dessin
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Tapis dégradé
            using (var br = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), C_TAPIS, C_TAPIS2, 128f))
                g.FillRectangle(br, 0, 0, Width, Height);

            // Texture de points
            using (var brPt = new SolidBrush(Color.FromArgb(9, 255, 255, 255)))
                for (int px = 30; px < Width; px += 48) for (int py = 56; py < Height - BAS_H - 10; py += 48)
                        g.FillEllipse(brPt, px, py, 3, 3);

            // Table ovale décorative
            int tw = 680, th = 310;
            int otx = Width / 2 - tw / 2, oty = Height / 2 - th / 2 - 18;
            using (var brT = new SolidBrush(Color.FromArgb(16, C_OR))) g.FillEllipse(brT, otx, oty, tw, th);
            using (var pen = new Pen(Color.FromArgb(52, C_OR), 2.5f)) g.DrawEllipse(pen, otx, oty, tw, th);
            using (var pen = new Pen(Color.FromArgb(18, C_OR), 1f)) g.DrawEllipse(pen, otx - 10, oty - 8, tw + 20, th + 16);

            // Titre
            string titre = "\u2660  TEXAS HOLD'EM  \u2665";
            SizeF szT = g.MeasureString(titre, F_TITRE);
            float ttx = Width / 2f - szT.Width / 2f;
            using (var brG = new LinearGradientBrush(new RectangleF(ttx, 9, szT.Width, szT.Height), C_OR, C_OR2, 90f))
                g.DrawString(titre, F_TITRE, brG, ttx, 9);

            // Badges jetons
            DrawBadge(g, "\u25cf " + Joueur + " : " + _jetons + " j.", 148, 14, _jetons < 150 ? C_ROUGE : C_VERT);
            string iaS = "\u25cf IA : " + _jetonsIA + " j.";
            SizeF szIA = g.MeasureString(iaS, F_UI);
            DrawBadge(g, iaS, Width - szIA.Width - 58, 14, _jetonsIA < 150 ? C_ROUGE : C_ROUGE);

            // Pot
            if (_pot > 0)
            {
                string pt = "\u265a  Pot : " + _pot + " j.";
                SizeF szP = g.MeasureString(pt, F_UI);
                DrawBadge(g, pt, Width / 2f - szP.Width / 2f, Height / 2f - 26, C_OR);
            }

            // Phase badge
            DrawPhaseBadge(g);
            DrawStats(g);

            // Sections cartes
            DrawSectionIA(g);
            DrawCommunity(g);
            DrawSectionJoueur(g);

            // Messages
            if (!string.IsNullOrEmpty(_message)) DrawMessage(g, _message, Height / 2f + 8, F_MSG, C_OR);
            if (!string.IsNullOrEmpty(_sousMsg)) DrawMessage(g, _sousMsg, Height / 2f + 46, F_UI_SM, C_TEXTE);
            if (!string.IsNullOrEmpty(_msgIA))
            {
                SizeF sz = g.MeasureString(_msgIA, F_UI_SM);
                float ix = Width / 2f - sz.Width / 2f, iy = Height / 2f - 72;
                FillRR(g, new SolidBrush(Color.FromArgb(178, 0, 0, 0)), ix - 10, iy - 5, sz.Width + 20, sz.Height + 10, 8);
                g.DrawString(_msgIA, F_UI_SM, new SolidBrush(C_TEXTE), ix, iy);
            }
        }

        private void DrawPhaseBadge(Graphics g)
        {
            if (_phase == THPhase.Attente || _phase == THPhase.Showdown) return;
            string[] noms = { "", "PRE-FLOP", "FLOP", "TURN", "RIVER" };
            Color[] cols = { Color.Gray, C_BLEU, C_VERT, C_ORANGE, C_ROUGE };
            int pIdx = (int)_phase; if (pIdx < 1 || pIdx > 4) return;

            // Centrer horizontalement
            float total = 0;
            for (int i = 1; i <= 4; i++)
            {
                SizeF s = g.MeasureString(noms[i], F_PHASE); total += s.Width + 22 + 10;
            }
            float bx = Width / 2f - total / 2f;
            float py = Height - BAS_H - 26;

            for (int i = 1; i <= 4; i++)
            {
                bool actif = (i == pIdx);
                Color col = actif ? cols[i] : Color.FromArgb(52, 255, 255, 255);
                SizeF sz = g.MeasureString(noms[i], F_PHASE);
                float pw = sz.Width + 22, ph = sz.Height + 8;
                FillRR(g, new SolidBrush(Color.FromArgb(actif ? 210 : 65, 0, 0, 0)), bx, py, pw, ph, 5);
                if (actif) using (var pen = new Pen(Color.FromArgb(125, col), 1.2f)) DrawRR(g, pen, bx, py, pw, ph, 5);
                g.DrawString(noms[i], F_PHASE, new SolidBrush(col), bx + 11, py + 4);
                bx += pw + 8;
            }
        }

        private void DrawSectionIA(Graphics g)
        {
            if (_holeIA.Count == 0) return;
            int y = 52; int tot = _holeIA.Count * CW + (_holeIA.Count - 1) * CGAP; int sx = Width / 2 - tot / 2;
            g.DrawString("Adversaire (IA)", F_UI_SM, new SolidBrush(Color.FromArgb(110, 255, 255, 255)), sx, y - 18);
            for (int i = 0; i < _holeIA.Count; i++) DrawCarte(g, _holeIA[i], sx + i * (CW + CGAP), y);
            if (_phase == THPhase.Showdown && !string.IsNullOrEmpty(_handIA))
                DrawEvalBadge(g, _handIA, sx + tot + 12, y, false);
        }

        private void DrawCommunity(Graphics g)
        {
            if (_community.Count == 0) return;
            int tot = _community.Count * CW + (_community.Count - 1) * CGAP;
            int sx = Width / 2 - tot / 2; int y = Height / 2 - CH / 2 - 20;
            for (int i = 0; i < _community.Count; i++) DrawCarte(g, _community[i], sx + i * (CW + CGAP), y);
        }

        private void DrawSectionJoueur(Graphics g)
        {
            if (_holeJoueur.Count == 0) return;
            int y = Height - BAS_H - CH - 30;
            int tot = _holeJoueur.Count * CW + (_holeJoueur.Count - 1) * CGAP; int sx = Width / 2 - tot / 2;
            g.DrawString("Votre main", F_UI_SM, new SolidBrush(Color.FromArgb(110, 255, 255, 255)), sx, y - 18);
            for (int i = 0; i < _holeJoueur.Count; i++) DrawCarte(g, _holeJoueur[i], sx + i * (CW + CGAP), y, true);
            if (!string.IsNullOrEmpty(_handJoueur)) DrawEvalBadge(g, _handJoueur, sx + tot + 12, y, true);
        }

        private void DrawEvalBadge(Graphics g, string txt, int x, int y, bool joueur)
        {
            Color col = joueur ? C_VERT : C_ROUGE;
            SizeF sz = g.MeasureString(txt, F_UI_SM);
            FillRR(g, new SolidBrush(Color.FromArgb(195, 0, 0, 0)), x - 4, y + CH / 2 - 14, sz.Width + 14, sz.Height + 8, 7);
            using (var pen = new Pen(Color.FromArgb(80, col), 1f)) DrawRR(g, pen, x - 4, y + CH / 2 - 14, sz.Width + 14, sz.Height + 8, 7);
            g.DrawString(txt, F_UI_SM, new SolidBrush(col), x + 3, y + CH / 2 - 11);
        }

        private void DrawBadge(Graphics g, string txt, float x, float y, Color col)
        {
            SizeF sz = g.MeasureString(txt, F_UI);
            FillRR(g, new SolidBrush(Color.FromArgb(142, 0, 0, 0)), x - 10, y - 5, sz.Width + 20, sz.Height + 10, 10);
            using (var pen = new Pen(Color.FromArgb(55, col), 1f)) DrawRR(g, pen, x - 10, y - 5, sz.Width + 20, sz.Height + 10, 10);
            g.DrawString(txt, F_UI, new SolidBrush(col), x, y);
        }

        private void DrawStats(Graphics g)
        {
            float wr = _parties > 0 ? (_victoires * 100f / _parties) : 0f;
            string st = $"Parties:{_parties}  V:{_victoires}  D:{_defaites}  E:{_egalites}  WR:{wr:0}%";
            SizeF sz = g.MeasureString(st, F_HIST);
            FillRR(g, new SolidBrush(Color.FromArgb(105, 0, 0, 0)), Width / 2f - sz.Width / 2f - 10, Height - BAS_H - 20, sz.Width + 20, sz.Height + 4, 5);
            g.DrawString(st, F_HIST, new SolidBrush(Color.FromArgb(172, 255, 255, 255)), Width / 2f - sz.Width / 2f, Height - BAS_H - 18);
        }

        private void DrawMessage(Graphics g, string txt, float y, Font font, Color col)
        {
            SizeF sz = g.MeasureString(txt, font); float x = Width / 2f - sz.Width / 2f;
            FillRR(g, new SolidBrush(Color.FromArgb(212, 0, 0, 0)), x - 22, y - 10, sz.Width + 44, sz.Height + 20, 14);
            using (var pen = new Pen(Color.FromArgb(48, col), 1f)) DrawRR(g, pen, x - 22, y - 10, sz.Width + 44, sz.Height + 20, 14);
            g.DrawString(txt, font, new SolidBrush(col), x, y);
        }

        private void DrawCarte(Graphics g, THCarte carte, int x, int y, bool hl = false)
        {
            using (var brO = new SolidBrush(Color.FromArgb(65, 0, 0, 0))) FillRR(g, brO, x + 4, y + 6, CW, CH, 8);

            if (!carte.FaceVisible)
            {
                using (var brD = new LinearGradientBrush(new Rectangle(x, y, CW, CH),
                    Color.FromArgb(20, 40, 150), Color.FromArgb(8, 16, 80), 45f)) FillRR(g, brD, x, y, CW, CH, 8);
                using (var pm = new Pen(Color.FromArgb(48, 100, 210), 1.2f))
                    for (int iy = y + 8; iy < y + CH - 6; iy += 12) for (int ix = x + 5; ix < x + CW - 4; ix += 12) g.DrawRectangle(pm, ix, iy, 5, 5);
                using (var pb = new Pen(Color.FromArgb(72, 120, 225), 2f)) DrawRR(g, pb, x, y, CW, CH, 8);
                return;
            }
            FillRR(g, new SolidBrush(hl ? Color.FromArgb(255, 248, 215) : Color.FromArgb(252, 248, 242)), x, y, CW, CH, 8);
            using (var border = hl ? new Pen(C_OR, 2.5f) : new Pen(Color.FromArgb(85, 0, 0, 0), 1.5f)) DrawRR(g, border, x, y, CW, CH, 8);
            if (hl) using (var pg = new Pen(Color.FromArgb(62, C_OR), 5f)) DrawRR(g, pg, x - 2, y - 2, CW + 4, CH + 4, 10);

            Color cC = carte.EstRouge ? Color.FromArgb(198, 24, 24) : Color.FromArgb(14, 14, 14);
            using (var brC = new SolidBrush(cC))
            {
                g.DrawString(carte.ValeurTexte, F_VAL, brC, x + 3, y + 2);
                g.DrawString(carte.Symbole, F_SYM_SM, brC, x + 3, y + 19);
                SizeF szS = g.MeasureString(carte.Symbole, F_SYM_LG);
                g.DrawString(carte.Symbole, F_SYM_LG, brC, x + (CW - szS.Width) / 2f, y + (CH - szS.Height) / 2f);
                var st = g.Save(); g.TranslateTransform(x + CW - 3, y + CH - 4); g.RotateTransform(180);
                g.DrawString(carte.ValeurTexte, F_VAL, brC, 0, 0); g.DrawString(carte.Symbole, F_SYM_SM, brC, 0, 18);
                g.Restore(st);
            }
        }

        private void PaintPanneauBas(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var br = new LinearGradientBrush(_pnlBas.ClientRectangle,
                Color.FromArgb(13, 32, 13), Color.FromArgb(5, 15, 5), 90f)) g.FillRectangle(br, _pnlBas.ClientRectangle);
            using (var pen = new Pen(Color.FromArgb(62, C_OR), 1.5f)) g.DrawLine(pen, 0, 0, _pnlBas.Width, 0);

            // Ligne info en haut du panneau bas
            if (_pnlZoneJeu.Visible && _pot > 0)
            {
                string info = "Pot : " + _pot + "  |  À suivre : " + Math.Max(0, _miseActuelle - _miseJoueur);
                g.DrawString(info, new Font("Segoe UI", 9), new SolidBrush(Color.FromArgb(135, C_OR)), 18, 8);
            }
        }

        private void PaintHistorique(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            FillRR(g, new SolidBrush(Color.FromArgb(228, 5, 20, 5)), 0, 0, _pnlHistorique.Width, _pnlHistorique.Height, 10);
            using (var pen = new Pen(Color.FromArgb(62, C_OR), 1.5f)) DrawRR(g, pen, 0, 0, _pnlHistorique.Width, _pnlHistorique.Height, 10);
            g.DrawString("HISTORIQUE", F_UI, new SolidBrush(C_OR), 12, 8);
            int yPos = 34, start = Math.Max(0, _historique.Count - 14);
            for (int i = _historique.Count - 1; i >= start; i--)
            {
                var rec = _historique[i];
                Color col = rec.Gain > 0 ? C_VERT : (rec.Gain < 0 ? C_ROUGE : Color.Gray);
                g.DrawString($"{rec.Phase,-11} {(rec.Gain >= 0 ? "+" : "")}{rec.Gain}", F_HIST, new SolidBrush(col), 8, yPos);
                yPos += 18;
            }
            if (_historique.Count == 0) g.DrawString("Aucune partie.", F_UI_SM, new SolidBrush(Color.Gray), 8, 36);
        }
        #endregion

    #region Helpers graphiques
        private static GraphicsPath MkPath(float x, float y, float w, float h, float r)
        {
            var p = new GraphicsPath();
            p.AddArc(x, y, r * 2, r * 2, 180, 90); p.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            p.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90); p.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            p.CloseFigure(); return p;
        }
        private static void FillRR(Graphics g, Brush br, float x, float y, float w, float h, float r)
        { using (var p = MkPath(x, y, w, h, r)) g.FillPath(br, p); }
        private static void DrawRR(Graphics g, Pen pen, float x, float y, float w, float h, float r)
        { using (var p = MkPath(x, y, w, h, r)) g.DrawPath(pen, p); }
        #endregion
    }
}