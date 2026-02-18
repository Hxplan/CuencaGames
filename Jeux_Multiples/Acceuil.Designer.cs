namespace Jeux_Multiples
{
    partial class Acceuil
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
            this.btnEchec = new System.Windows.Forms.Button();
            this.btn4cheveaux = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
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
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(600, 24);
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
            this.connexionsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.connexionsToolStripMenuItem.Text = "Connexions";
            // 
            // inscriptionToolStripMenuItem
            // 
            this.inscriptionToolStripMenuItem.Name = "inscriptionToolStripMenuItem";
            this.inscriptionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.inscriptionToolStripMenuItem.Text = "Inscription";
            // 
            // btnPuissance4
            // 
            this.btnPuissance4.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnPuissance4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPuissance4.ForeColor = System.Drawing.Color.Blue;
            this.btnPuissance4.Location = new System.Drawing.Point(64, 171);
            this.btnPuissance4.Margin = new System.Windows.Forms.Padding(2);
            this.btnPuissance4.Name = "btnPuissance4";
            this.btnPuissance4.Size = new System.Drawing.Size(128, 29);
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
            this.btnMortPion.Location = new System.Drawing.Point(230, 171);
            this.btnMortPion.Margin = new System.Windows.Forms.Padding(2);
            this.btnMortPion.Name = "btnMortPion";
            this.btnMortPion.Size = new System.Drawing.Size(128, 29);
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
            this.btnSnake.Location = new System.Drawing.Point(412, 171);
            this.btnSnake.Margin = new System.Windows.Forms.Padding(2);
            this.btnSnake.Name = "btnSnake";
            this.btnSnake.Size = new System.Drawing.Size(128, 29);
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
            this.label1.Location = new System.Drawing.Point(126, 108);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(378, 31);
            this.label1.TabIndex = 5;
            this.label1.Text = "Bienvenue sur le Multi Jeux ";
            // 
            // btnPker
            // 
            this.btnPker.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnPker.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPker.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnPker.Location = new System.Drawing.Point(230, 225);
            this.btnPker.Margin = new System.Windows.Forms.Padding(2);
            this.btnPker.Name = "btnPker";
            this.btnPker.Size = new System.Drawing.Size(128, 29);
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
            this.btnBJ.Location = new System.Drawing.Point(64, 225);
            this.btnBJ.Margin = new System.Windows.Forms.Padding(2);
            this.btnBJ.Name = "btnBJ";
            this.btnBJ.Size = new System.Drawing.Size(128, 29);
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
            this.button1.Location = new System.Drawing.Point(412, 225);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 29);
            this.button1.TabIndex = 10;
            this.button1.Text = "Dame";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnEchec
            // 
            this.btnEchec.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btnEchec.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEchec.ForeColor = System.Drawing.Color.Blue;
            this.btnEchec.Location = new System.Drawing.Point(64, 277);
            this.btnEchec.Margin = new System.Windows.Forms.Padding(2);
            this.btnEchec.Name = "btnEchec";
            this.btnEchec.Size = new System.Drawing.Size(128, 29);
            this.btnEchec.TabIndex = 12;
            this.btnEchec.Text = "Echec";
            this.btnEchec.UseVisualStyleBackColor = false;
            this.btnEchec.Click += new System.EventHandler(this.btnEchec_Click);
            // 
            // btn4cheveaux
            // 
            this.btn4cheveaux.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.btn4cheveaux.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn4cheveaux.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btn4cheveaux.Location = new System.Drawing.Point(230, 277);
            this.btn4cheveaux.Margin = new System.Windows.Forms.Padding(2);
            this.btn4cheveaux.Name = "btn4cheveaux";
            this.btn4cheveaux.Size = new System.Drawing.Size(128, 29);
            this.btn4cheveaux.TabIndex = 13;
            this.btn4cheveaux.Text = "4 Cheveaux";
            this.btn4cheveaux.UseVisualStyleBackColor = false;
            this.btn4cheveaux.Click += new System.EventHandler(this.btn4cheveaux_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.ForeColor = System.Drawing.Color.Green;
            this.button4.Location = new System.Drawing.Point(412, 277);
            this.button4.Margin = new System.Windows.Forms.Padding(2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(128, 29);
            this.button4.TabIndex = 14;
            this.button4.UseVisualStyleBackColor = false;
            // 
            // Acceuil
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 366);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btn4cheveaux);
            this.Controls.Add(this.btnEchec);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnBJ);
            this.Controls.Add(this.btnPker);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSnake);
            this.Controls.Add(this.btnMortPion);
            this.Controls.Add(this.btnPuissance4);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Acceuil";
            this.Text = "Acceuil";
            this.Load += new System.EventHandler(this.Acceuil_Load);
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
        private System.Windows.Forms.Button btnEchec;
        private System.Windows.Forms.Button btn4cheveaux;
        private System.Windows.Forms.Button button4;
    }
}