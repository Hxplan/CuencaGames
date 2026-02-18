namespace Jeux_Multiples
{
    partial class Accueil
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.quitterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nouveauToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connexionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inscriptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnPuissance4 = new System.Windows.Forms.Button();
            this.btnMortPion = new System.Windows.Forms.Button();
            this.btnSnake = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPker = new System.Windows.Forms.Button();
            this.btnBJ = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtPseudo = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.btnHost = new System.Windows.Forms.Button();
            this.btnJoin = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitterToolStripMenuItem,
            this.nouveauToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(450, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // quitterToolStripMenuItem
            // 
            this.quitterToolStripMenuItem.Name = "quitterToolStripMenuItem";
            this.quitterToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.quitterToolStripMenuItem.Text = "Quitter";
            this.quitterToolStripMenuItem.Click += new System.EventHandler(this.quitterToolStripMenuItem_Click);
            // 
            // nouveauToolStripMenuItem
            // 
            this.nouveauToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connexionsToolStripMenuItem,
            this.inscriptionToolStripMenuItem});
            this.nouveauToolStripMenuItem.Name = "nouveauToolStripMenuItem";
            this.nouveauToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.nouveauToolStripMenuItem.Text = "Nouveau";
            // 
            // connexionsToolStripMenuItem
            // 
            this.connexionsToolStripMenuItem.Name = "connexionsToolStripMenuItem";
            this.connexionsToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.connexionsToolStripMenuItem.Text = "Connexions";
            // 
            // inscriptionToolStripMenuItem
            // 
            this.inscriptionToolStripMenuItem.Name = "inscriptionToolStripMenuItem";
            this.inscriptionToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.inscriptionToolStripMenuItem.Text = "Inscription";
            // 
            // btnPuissance4
            // 
            this.btnPuissance4.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnPuissance4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPuissance4.ForeColor = System.Drawing.Color.Blue;
            this.btnPuissance4.Location = new System.Drawing.Point(45, 180);
            this.btnPuissance4.Margin = new System.Windows.Forms.Padding(2);
            this.btnPuissance4.Name = "btnPuissance4";
            this.btnPuissance4.Size = new System.Drawing.Size(96, 24);
            this.btnPuissance4.TabIndex = 2;
            this.btnPuissance4.Text = "Puissance 4";
            this.btnPuissance4.UseVisualStyleBackColor = false;
            this.btnPuissance4.Click += new System.EventHandler(this.btnPuissance4_Click);
            // 
            // btnMortPion
            // 
            this.btnMortPion.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnMortPion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMortPion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnMortPion.Location = new System.Drawing.Point(170, 180);
            this.btnMortPion.Margin = new System.Windows.Forms.Padding(2);
            this.btnMortPion.Name = "btnMortPion";
            this.btnMortPion.Size = new System.Drawing.Size(96, 24);
            this.btnMortPion.TabIndex = 3;
            this.btnMortPion.Text = "MortPion";
            this.btnMortPion.UseVisualStyleBackColor = false;
            this.btnMortPion.Click += new System.EventHandler(this.btnMortPion_Click);
            // 
            // btnSnake
            // 
            this.btnSnake.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnSnake.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSnake.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnSnake.Location = new System.Drawing.Point(306, 180);
            this.btnSnake.Margin = new System.Windows.Forms.Padding(2);
            this.btnSnake.Name = "btnSnake";
            this.btnSnake.Size = new System.Drawing.Size(96, 24);
            this.btnSnake.TabIndex = 4;
            this.btnSnake.Text = "Snake";
            this.btnSnake.UseVisualStyleBackColor = false;
            this.btnSnake.Click += new System.EventHandler(this.btnSnake_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(39, 89);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(378, 31);
            this.label1.TabIndex = 5;
            this.label1.Text = "Bienvenue sur le Multi Jeux ";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnPker
            // 
            this.btnPker.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnPker.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPker.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPker.Location = new System.Drawing.Point(170, 223);
            this.btnPker.Margin = new System.Windows.Forms.Padding(2);
            this.btnPker.Name = "btnPker";
            this.btnPker.Size = new System.Drawing.Size(96, 24);
            this.btnPker.TabIndex = 7;
            this.btnPker.Text = "Poker";
            this.btnPker.UseVisualStyleBackColor = false;
            this.btnPker.Click += new System.EventHandler(this.btnPker_Click);
            // 
            // btnBJ
            // 
            this.btnBJ.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnBJ.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBJ.ForeColor = System.Drawing.Color.Cyan;
            this.btnBJ.Location = new System.Drawing.Point(45, 223);
            this.btnBJ.Margin = new System.Windows.Forms.Padding(2);
            this.btnBJ.Name = "btnBJ";
            this.btnBJ.Size = new System.Drawing.Size(96, 24);
            this.btnBJ.TabIndex = 8;
            this.btnBJ.Text = "BlackJack";
            this.btnBJ.UseVisualStyleBackColor = false;
            this.btnBJ.Click += new System.EventHandler(this.btnBJ_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Yellow;
            this.button1.Location = new System.Drawing.Point(306, 223);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(96, 24);
            this.button1.TabIndex = 10;
            this.button1.Text = "Dame";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPseudo
            // 
            this.txtPseudo.ForeColor = System.Drawing.Color.Gray;
            this.txtPseudo.Location = new System.Drawing.Point(15, 32);
            this.txtPseudo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtPseudo.Name = "txtPseudo";
            this.txtPseudo.Size = new System.Drawing.Size(151, 20);
            this.txtPseudo.TabIndex = 100;
            this.txtPseudo.Text = "Entrez votre pseudo";
            this.txtPseudo.GotFocus += new System.EventHandler(this.txtPseudo_GotFocus);
            this.txtPseudo.LostFocus += new System.EventHandler(this.txtPseudo_LostFocus);
            // 
            // txtIP
            // 
            this.txtIP.ForeColor = System.Drawing.Color.Gray;
            this.txtIP.Location = new System.Drawing.Point(180, 32);
            this.txtIP.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(91, 20);
            this.txtIP.TabIndex = 101;
            this.txtIP.Text = "IP de l\'hôte";
            this.txtIP.GotFocus += new System.EventHandler(this.txtIP_GotFocus);
            this.txtIP.LostFocus += new System.EventHandler(this.txtIP_LostFocus);
            // 
            // btnHost
            // 
            this.btnHost.Location = new System.Drawing.Point(278, 32);
            this.btnHost.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnHost.Name = "btnHost";
            this.btnHost.Size = new System.Drawing.Size(75, 24);
            this.btnHost.TabIndex = 102;
            this.btnHost.Text = "Héberger partie";
            this.btnHost.UseVisualStyleBackColor = true;
            this.btnHost.Click += new System.EventHandler(this.btnHost_Click);
            // 
            // btnJoin
            // 
            this.btnJoin.Location = new System.Drawing.Point(360, 32);
            this.btnJoin.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(75, 24);
            this.btnJoin.TabIndex = 103;
            this.btnJoin.Text = "Rejoindre partie";
            this.btnJoin.UseVisualStyleBackColor = true;
            this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(15, 57);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 104;
            // 
            // Accueil
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 284);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnJoin);
            this.Controls.Add(this.btnHost);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.txtPseudo);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnBJ);
            this.Controls.Add(this.btnPker);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSnake);
            this.Controls.Add(this.btnMortPion);
            this.Controls.Add(this.btnPuissance4);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Accueil";
            this.Text = " ";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem quitterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nouveauToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connexionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inscriptionToolStripMenuItem;
        private System.Windows.Forms.Button btnPuissance4;
        private System.Windows.Forms.Button btnMortPion;
        private System.Windows.Forms.Button btnSnake;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnPker;
        private System.Windows.Forms.Button btnBJ;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtPseudo;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.Label lblStatus;
    }
}